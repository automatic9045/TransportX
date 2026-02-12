using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal class ModelBuilder
    {
        private readonly Dictionary<string, ID3D11ShaderResourceView> LoadedTextures = [];

        private readonly ID3D11DeviceContext Context;
        private readonly IErrorCollector ErrorCollector;

        private readonly WICTextureFactory TextureFactory;

        public ModelBuilder(ID3D11DeviceContext context, IWICImagingFactory wicFactory, IErrorCollector errorCollector)
        {
            Context = context;
            ErrorCollector = errorCollector;

            TextureFactory = new WICTextureFactory(Context, wicFactory);
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

                    Vector4 linearBaseColor = materialData.BaseColor.ToLinear();

                    IErrorCollector textureErrorCollector = IErrorCollector.Default();
                    textureErrorCollector.Reported += (sender, e) =>
                    {
                        Error error = e.Error.ChangeSource(sourceLocation);
                        ErrorCollector.Report(error);
                    };
                    List<ID3D11ShaderResourceView> textures = LoadMaterialTextures(materialData, model.EmbeddedTextures, baseDirectory, textureErrorCollector);

                    material = new(linearBaseColor, textures);
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
                            try
                            {
                                texture = TextureFactory.CreateFromFile(filePath);
                            }
                            catch (FileNotFoundException ex)
                            {
                                ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                    ErrorLevel.Error, $"テクスチャ '{filePath}' が見つかりません。", filePath)
                                {
                                    Exception = ex,
                                };
                                errorCollector.Report(error);
                            }
                            catch (Exception ex)
                            {
                                ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                    ErrorLevel.Error, $"テクスチャ '{filePath}' を読み込めませんでした。", filePath)
                                {
                                    Exception = ex,
                                };
                                errorCollector.Report(error);
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
                                ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                    ErrorLevel.Error, $"埋め込みテクスチャ '{textureRef.Key}' を読み込めませんでした。", textureRef.Key)
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
            if (texture.IsCompressed)
            {
                return TextureFactory.CreateFromMemory(texture.Data.Span);
            }
            else
            {
                using MemoryHandle handle = texture.Data.Pin();

                Texture2DDescription desc = new()
                {
                    Width = (uint)texture.Width,
                    Height = (uint)texture.Height,
                    MipLevels = 0,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    Format = Format.B8G8R8A8_UNorm_SRgb,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    CPUAccessFlags = 0,
                    MiscFlags = ResourceOptionFlags.GenerateMips,
                };

                ID3D11Texture2D baseTexture = Context.Device.CreateTexture2D(desc);
                Context.UpdateSubresource(baseTexture, 0, null, (nint)handle.Pointer, (uint)(texture.Width * 4), 0);

                ID3D11ShaderResourceView view = Context.Device.CreateShaderResourceView(baseTexture);
                Context.GenerateMips(view);

                return view;
            }
        }
    }
}
