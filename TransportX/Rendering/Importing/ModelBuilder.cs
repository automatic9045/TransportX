using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

        private readonly IWICImagingFactory WIC;
        private readonly WICTextureFactory WICFactory;
        private readonly DDSTextureFactory DDSFactory;

        public ModelBuilder(ID3D11DeviceContext context, IWICImagingFactory wic, IErrorCollector errorCollector)
        {
            Context = context;
            ErrorCollector = errorCollector;

            WIC = wic;
            WICFactory = new WICTextureFactory(Context, WIC);
            DDSFactory = new DDSTextureFactory(Context.Device);
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
                        Color = (meshData.Colors is null ? Vector4.One : meshData.Colors[j]).ToLinear(),
                        Normal = meshData.Normals is null ? throw new ArgumentException("法線情報が定義されていません。", nameof(model)) : meshData.Normals[j],
                        Tangent = meshData.Tangents is null ? throw new ArgumentException("接線情報が定義されていません。", nameof(model)) : meshData.Tangents[j],
                        TextureCoord = meshData.TextureCoords is null ? default : meshData.TextureCoords[j],
                    };
                }

                Rendering.Material material = Rendering.Material.Default();
                if (0 <= meshData.MaterialIndex)
                {
                    Material materialData = model.Materials[meshData.MaterialIndex];

                    IErrorCollector textureErrorCollector = IErrorCollector.Default();
                    textureErrorCollector.Reported += (sender, e) =>
                    {
                        Error error = e.Error.ChangeSource(sourceLocation);
                        ErrorCollector.Report(error);
                    };

                    ID3D11ShaderResourceView? baseColorTexture = materialData.BaseColorTexture.HasValue
                        ? LoadTexture(materialData.BaseColorTexture.Value, false, model.EmbeddedTextures, textureErrorCollector) : null;

                    ID3D11ShaderResourceView? normalTexture = materialData.NormalTexture.HasValue
                        ? LoadTexture(materialData.NormalTexture.Value, true, model.EmbeddedTextures, textureErrorCollector) : null;

                    string? keyO = materialData.OcclusionTexture?.Key;
                    string? keyR = materialData.RoughnessTexture?.Key;
                    string? keyM = materialData.MetallicTexture?.Key;
                    string combinedKey = $"*orm|{keyO}|{keyR}|{keyM}";

                    ID3D11ShaderResourceView? ormTexture = null;
                    if (LoadedTextures.TryGetValue(combinedKey, out ID3D11ShaderResourceView? cachedTexture))
                    {
                        ormTexture = cachedTexture;
                    }
                    else if (materialData.MetallicTexture.HasValue && keyO == keyR && keyR == keyM)
                    {
                        ormTexture = LoadTexture(materialData.MetallicTexture.Value, true, model.EmbeddedTextures, textureErrorCollector);
                        if (ormTexture is not null) LoadedTextures.Add(combinedKey, ormTexture);
                    }
                    else
                    {
                        IWICStream? occlusionStream = null;
                        IWICStream? roughnessStream = null;
                        IWICStream? metallicStream = null;

                        try
                        {
                            if (materialData.OcclusionTexture.HasValue)
                            {
                                occlusionStream = CreateStream(materialData.OcclusionTexture.Value, model.EmbeddedTextures);
                            }

                            if (materialData.RoughnessTexture.HasValue)
                            {
                                roughnessStream = CreateStream(materialData.RoughnessTexture.Value, model.EmbeddedTextures);
                            }

                            if (materialData.MetallicTexture.HasValue)
                            {
                                metallicStream = CreateStream(materialData.MetallicTexture.Value, model.EmbeddedTextures);
                            }

                            if (occlusionStream is not null || roughnessStream is not null || metallicStream is not null)
                            {
                                try
                                {
                                    ormTexture = WICFactory.CreateFromMerged(occlusionStream, roughnessStream, metallicStream, true);
                                    LoadedTextures.Add(combinedKey, ormTexture);
                                }
                                catch (Exception ex)
                                {
                                    ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual, ErrorLevel.Error,
                                        $"PBR 遮蔽テクスチャ '{keyO}'、粗さテクスチャ '{keyR}'、金属感テクスチャ '{keyM}' の合成に失敗しました。", sourceLocation)
                                    {
                                        Exception = ex,
                                    };
                                    textureErrorCollector.Report(error);
                                }
                            }


                            IWICStream? CreateStream(TextureReference textureRef, IReadOnlyDictionary<string, EmbeddedTexture> embeddedTextures)
                            {
                                IWICStream stream = WIC.CreateStream();

                                if (textureRef.Type == TextureReference.TextureType.File)
                                {
                                    string extension = Path.GetExtension(textureRef.Key).ToLowerInvariant();
                                    switch (extension)
                                    {
                                        case ".dds":
                                        case ".tga":
                                        case ".ktx":
                                        case ".ktx2":
                                            ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual, ErrorLevel.Error,
                                                $"{extension} 形式の PBR テクスチャ '{textureRef.Key}' は PBR ORM テクスチャの合成に使用できません。" +
                                                $"ORM テクスチャの合成は WIC 形式のみ対応しています。", sourceLocation);
                                            textureErrorCollector.Report(error);

                                            stream.Dispose();
                                            return null;
                                    }

                                    string path = Path.Combine(baseDirectory, textureRef.Key);
                                    stream.Initialize(path, FileAccess.Read);
                                }
                                else if (textureRef.Type == TextureReference.TextureType.Embedded)
                                {
                                    if (embeddedTextures.TryGetValue(textureRef.Key, out EmbeddedTexture texture))
                                    {
                                        if (texture.Format != TextureFormat.WIC)
                                        {
                                            ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual, ErrorLevel.Error,
                                                $"{texture.Format} 形式の PBR テクスチャ '{textureRef.Key}' は PBR ORM テクスチャの合成に使用できません。" +
                                                $"ORM テクスチャの合成は WIC 形式のみ対応しています。", sourceLocation);
                                            textureErrorCollector.Report(error);

                                            stream.Dispose();
                                            return null;
                                        }

                                        stream.Initialize(texture.Data.Span);
                                    }
                                }

                                return stream;
                            }
                        }
                        finally
                        {
                            occlusionStream?.Dispose();
                            roughnessStream?.Dispose();
                            metallicStream?.Dispose();
                        }
                    }

                    ID3D11ShaderResourceView? emissiveTexture = materialData.EmissiveTexture.HasValue
                        ? LoadTexture(materialData.EmissiveTexture.Value, false, model.EmbeddedTextures, textureErrorCollector) : null;


                    material = new Rendering.Material()
                    {
                        BaseColor = materialData.BaseColor,
                        Metallic = materialData.Metallic,
                        Roughness = materialData.Roughness,
                        Emissive = materialData.Emissive,

                        BaseColorTexture = baseColorTexture,
                        NormalTexture = normalTexture,
                        ORMTexture = ormTexture,
                        EmissiveTexture = emissiveTexture,
                    };
                }

                meshes[i] = Rendering.Mesh.Create(Context.Device, vertices, meshData.Indices, material);


                ID3D11ShaderResourceView? LoadTexture(
                    TextureReference textureRef, bool isLinear, IReadOnlyDictionary<string, EmbeddedTexture> embeddedTextures, IErrorCollector errorCollector)
                {
                    if (LoadedTextures.TryGetValue(textureRef.Key, out ID3D11ShaderResourceView? texture))
                    {
                        return texture;
                    }

                    switch (textureRef.Type)
                    {
                        case TextureReference.TextureType.File:
                        {
                            string filePath = Path.Combine(baseDirectory, textureRef.Key);
                            try
                            {
                                IErrorCollector textureErrorCollector = IErrorCollector.Default();
                                textureErrorCollector.Reported += (sender, e) =>
                                {
                                    Error error = e.Error.ChangeSource(filePath);
                                    errorCollector.Report(error);
                                };

                                texture = Path.GetExtension(filePath).ToLowerInvariant() switch
                                {
                                    ".dds" => DDSFactory.CreateFromFile(filePath),
                                    _ => WICFactory.CreateFromFile(filePath, isLinear, textureErrorCollector),
                                };
                            }
                            catch (FileNotFoundException ex)
                            {
                                ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                    ErrorLevel.Error, $"テクスチャファイル '{filePath}' が見つかりませんでした。", filePath)
                                {
                                    Exception = ex,
                                };
                                errorCollector.Report(error);
                            }
                            catch (Exception ex)
                            {
                                ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                    ErrorLevel.Error, $"テクスチャファイル '{filePath}' を読み込めませんでした。", filePath)
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
                                EmbeddedTexture embeddedTexture = embeddedTextures[textureRef.Key];
                                switch (embeddedTexture.Format)
                                {
                                    case TextureFormat.Uncompressed:
                                        unsafe
                                        {
                                            using MemoryHandle handle = embeddedTexture.Data.Pin();

                                            Texture2DDescription desc = new()
                                            {
                                                Width = (uint)embeddedTexture.Width,
                                                Height = (uint)embeddedTexture.Height,
                                                MipLevels = 0,
                                                ArraySize = 1,
                                                SampleDescription = new SampleDescription(1, 0),
                                                Usage = ResourceUsage.Default,
                                                Format = isLinear ? Format.B8G8R8A8_UNorm : Format.B8G8R8A8_UNorm_SRgb,
                                                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                                                CPUAccessFlags = 0,
                                                MiscFlags = ResourceOptionFlags.GenerateMips,
                                            };

                                            using ID3D11Texture2D baseTexture = Context.Device.CreateTexture2D(desc);
                                            Context.UpdateSubresource(baseTexture, 0, null, (nint)handle.Pointer, (uint)embeddedTexture.Width * 4, 0);

                                            ID3D11ShaderResourceView view = Context.Device.CreateShaderResourceView(baseTexture);
                                            Context.GenerateMips(view);

                                            texture = view;
                                        }
                                        break;

                                    case TextureFormat.WIC:
                                        IErrorCollector textureErrorCollector = IErrorCollector.Default();
                                        textureErrorCollector.Reported += (sender, e) =>
                                        {
                                            Error error = e.Error.ChangeSource(sourceLocation);
                                            errorCollector.Report(error);
                                        };

                                        texture = WICFactory.CreateFromMemory(embeddedTexture.Data.Span, isLinear, textureErrorCollector);
                                        break;

                                    case TextureFormat.DDS:
                                        texture = DDSFactory.CreateFromMemory(embeddedTexture.Data.Span);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                    ErrorLevel.Error, $"埋め込みテクスチャ '{textureRef.Key}' を読み込めませんでした。", sourceLocation)
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
                        LoadedTextures.Add(textureRef.Key, texture);
                    }

                    return texture;
                }
            }

            return new Rendering.Model(meshes, LoadedTextures.Values.ToArray());
        }
    }
}
