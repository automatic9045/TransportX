using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal class ModelBuilder
    {
        private readonly Dictionary<string, ID3D11ShaderResourceView> LoadedTextures = [];

        private readonly ID3D11DeviceContext Context;
        private readonly IErrorCollector ErrorCollector;

        public ModelBuilder(ID3D11DeviceContext context, IErrorCollector errorCollector)
        {
            Context = context;
            ErrorCollector = errorCollector;
        }

        public unsafe Rendering.Model Create(Model model, string baseDirectory, string sourceLocation)
        {
            Rendering.Mesh[] meshes = new Rendering.Mesh[model.Meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh meshData = model.Meshes[i];

                Vertex[] vertices = new Vertex[meshData.Vertices.Length];
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] = new Vertex()
                    {
                        Position = meshData.Vertices[j],
                        Normal = meshData.Normals[j],
                        Color = meshData.Colors is null ? Vector4.One : meshData.Colors[j],
                        TextureCoord = meshData.TextureCoords is null ? default : meshData.TextureCoords[j],
                    };
                }

                Rendering.Material material = Rendering.Material.Default;
                if (0 <= meshData.MaterialIndex)
                {
                    Material materialData = model.Materials[meshData.MaterialIndex];

                    IErrorCollector textureErrorCollector = IErrorCollector.Default();
                    textureErrorCollector.Reported += (sender, e) =>
                    {
                        Error error = e.Error.ChangeSource(sourceLocation);
                        ErrorCollector.Report(error);
                    };

                    List<ID3D11ShaderResourceView> textures = LoadMaterialTextures(materialData, model.EmbeddedTextures, baseDirectory, textureErrorCollector);
                    material = new(materialData.BaseColor, textures);
                }

                meshes[i] = Rendering.Mesh.Create(Context.Device, vertices, meshData.Indices, material);
            }

            return new Rendering.Model(meshes, LoadedTextures.Values);
        }

        private List<ID3D11ShaderResourceView> LoadMaterialTextures(
            Material material, IReadOnlyDictionary<string, Texture> embeddedTextures, string baseDirectory, IErrorCollector errorCollector)
        {
            List<ID3D11ShaderResourceView> textures = new List<ID3D11ShaderResourceView>();
            for (int i = 0; i < material.Textures.Length; i++)
            {
                TextureReference textureRef = material.Textures[i];

                if (LoadedTextures.TryGetValue(textureRef.Key, out ID3D11ShaderResourceView? texture))
                {
                    textures.Add(texture);
                }
                else
                {
                    switch (textureRef.Type)
                    {
                        case TextureReference.TextureType.File:
                        {
                            string filePath = Path.Combine(baseDirectory, textureRef.Key);

                            int hr = NativeMethods.CreateWICTextureFromFile_(Context.Device.NativePointer, Context.NativePointer, filePath, out _, out nint textureView);
                            if (hr != 0)
                            {
                                Exception? exception = Marshal.GetExceptionForHR(hr);
                                ModelLoadError error = exception switch
                                {
                                    FileNotFoundException => new ModelLoadError(ModelLoadErrorTypes.Visual | ModelLoadErrorTypes.Skipped,
                                        ErrorLevel.Error, $"テクスチャ '{filePath}' が見つかりません。", filePath)
                                    {
                                        Exception = exception,
                                    },
                                    _ => new ModelLoadError(ModelLoadErrorTypes.Visual | ModelLoadErrorTypes.Skipped,
                                        ErrorLevel.Error, $"テクスチャ '{filePath}' を読み込めませんでした。", filePath)
                                    {
                                        Exception = exception,
                                    },
                                };
                                errorCollector.Report(error);
                            }
                            else
                            {
                                texture = new ID3D11ShaderResourceView(textureView);
                            }
                            break;
                        }

                        case TextureReference.TextureType.Embedded:
                        {
                            try
                            {
                                Texture embeddedTexture = embeddedTextures[textureRef.Key];
                                texture = LoadEmbeddedTexture(embeddedTexture);
                            }
                            catch (Exception ex)
                            {
                                ModelLoadError error = new(ModelLoadErrorTypes.Visual | ModelLoadErrorTypes.Skipped, ErrorLevel.Error,
                                    $"埋め込みテクスチャ '{textureRef.Key}' を読み込めませんでした。", textureRef.Key)
                                {
                                    Exception = ex,
                                };
                                errorCollector.Report(error);
                            }
                            break;
                        }
                    }

                    if (texture is not null)
                    {
                        textures.Add(texture);
                        LoadedTextures.Add(textureRef.Key, texture);
                    }
                }
            }

            return textures;
        }

        private unsafe ID3D11ShaderResourceView LoadEmbeddedTexture(Texture texture)
        {
            ReadOnlySpan<byte> data = texture.Data.Span;
            fixed (byte* pData = data)
            {
                if (texture.IsCompressed)
                {
                    int hr = NativeMethods.CreateWICTextureFromMemory_(Context.Device.NativePointer, Context.NativePointer, (nint)pData, data.Length, out _, out nint textureView);
                    if (hr != 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    return new ID3D11ShaderResourceView(textureView);
                }
                else
                {
                    Texture2DDescription desc = new Texture2DDescription()
                    {
                        Width = (uint)texture.Width,
                        Height = (uint)texture.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        Format = Format.B8G8R8A8_UNorm,
                        BindFlags = BindFlags.ShaderResource,
                        CPUAccessFlags = 0,
                        MiscFlags = 0,
                    };

                    SubresourceData subresourceData = new SubresourceData()
                    {
                        DataPointer = (nint)pData,
                        RowPitch = (uint)(texture.Width * 4),
                        SlicePitch = (uint)data.Length,
                    };

                    ID3D11Texture2D texture2D = Context.Device.CreateTexture2D(desc, subresourceData);
                    return Context.Device.CreateShaderResourceView(texture2D);
                }
            }
        }
    }
}
