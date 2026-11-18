using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.WIC;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal class ModelBuilder
    {
        private readonly ID3D11DeviceContext Context;
        private readonly IErrorCollector ErrorCollector;

        private readonly IWICImagingFactory WIC;
        private readonly TextureFactory TextureFactory;

        private readonly Dictionary<string, ID3D11ShaderResourceView> SharedTextures = [];

        private readonly HashSet<ID3D11ShaderResourceView> TexturesKey = [];
        public IReadOnlyCollection<ID3D11ShaderResourceView> Textures => TexturesKey;

        public ModelBuilder(ID3D11DeviceContext context, IWICImagingFactory wic, IErrorCollector errorCollector)
        {
            Context = context;
            ErrorCollector = errorCollector;

            WIC = wic;
            TextureFactory = new TextureFactory(Context, WIC);
        }

        public ModelResourceSet Create(Model modelData, string baseDirectory, string sourceLocation)
        {
            IErrorCollector textureErrorCollector = IErrorCollector.Default();
            textureErrorCollector.Reported += (sender, e) =>
            {
                Error error = e.Error.Location is null ? e.Error.ChangeSource(sourceLocation) : e.Error;
                ModelLoadError modelError = new(error, ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual);
                ErrorCollector.Report(modelError);
            };

            Dictionary<string, ID3D11ShaderResourceView> loadedTextures = [];

            List<Rendering.Material> materials = new(modelData.Materials.Length);
            for (int i = 0; i < modelData.Materials.Length; i++)
            {
                Material materialData = modelData.Materials[i];

                ID3D11ShaderResourceView? baseColorTexture = materialData.BaseColorTexture.HasValue
                    ? LoadTexture(materialData.BaseColorTexture.Value, false, modelData.EmbeddedTextures) : null;

                ID3D11ShaderResourceView? normalTexture = materialData.NormalTexture.HasValue
                    ? LoadTexture(materialData.NormalTexture.Value, true, modelData.EmbeddedTextures) : null;

                string? keyO = materialData.OcclusionTexture?.Key;
                string? keyR = materialData.RoughnessTexture?.Key;
                string? keyM = materialData.MetallicTexture?.Key;
                string combinedKey = $"*orm|{keyO}|{keyR}|{keyM}";

                ID3D11ShaderResourceView? ormTexture = null;
                if (loadedTextures.TryGetValue(combinedKey, out ID3D11ShaderResourceView? cachedTexture))
                {
                    ormTexture = cachedTexture;
                }
                else if (materialData.MetallicTexture.HasValue && keyO == keyR && keyR == keyM)
                {
                    ormTexture = LoadTexture(materialData.MetallicTexture.Value, true, modelData.EmbeddedTextures);
                    if (ormTexture is not null) loadedTextures.Add(combinedKey, ormTexture);
                }
                else if (materialData.OcclusionTexture.HasValue || materialData.RoughnessTexture.HasValue || materialData.MetallicTexture.HasValue)
                {
                    try
                    {
                        ormTexture = TextureFactory.CreateMergedFromReferences(
                            materialData.OcclusionTexture, materialData.RoughnessTexture, materialData.MetallicTexture,
                            baseDirectory, modelData.EmbeddedTextures, true, textureErrorCollector);

                        if (ormTexture is not null)
                        {
                            loadedTextures.Add(combinedKey, ormTexture);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual, ErrorLevel.Error,
                            $"PBR 遮蔽テクスチャ '{keyO}'、粗さテクスチャ '{keyR}'、金属感テクスチャ '{keyM}' の合成に失敗しました。", sourceLocation)
                        {
                            Exception = ex,
                        };
                        ErrorCollector.Report(error);
                    }
                }

                ID3D11ShaderResourceView? emissiveTexture = materialData.EmissiveTexture.HasValue
                    ? LoadTexture(materialData.EmissiveTexture.Value, false, modelData.EmbeddedTextures)
                    : null;

                Rendering.Material material = new()
                {
                    Name = materialData.Name,

                    BaseColor = materialData.BaseColor,
                    Metallic = materialData.Metallic,
                    Roughness = materialData.Roughness,
                    Emissive = materialData.Emissive,

                    BaseColorTexture = baseColorTexture,
                    NormalTexture = normalTexture,
                    ORMTexture = ormTexture,
                    EmissiveTexture = emissiveTexture,

                    DebugName = materialData.Name,
                };
                materials.Add(material);
            }


            Rendering.Material? defaultMaterial = null;
            Rendering.Mesh[] meshes = new Rendering.Mesh[modelData.Meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh meshData = modelData.Meshes[i];

                Vertex[] vertices = new Vertex[meshData.Vertices.Length];
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] = new Vertex()
                    {
                        Position = meshData.Vertices[j],
                        Color = (meshData.Colors is null ? Vector4.One : meshData.Colors[j]).ToLinear(),
                        Normal = meshData.Normals is null ? throw new ArgumentException("法線情報が定義されていません。", nameof(modelData)) : meshData.Normals[j],
                        Tangent = meshData.Tangents is null ? throw new ArgumentException("接線情報が定義されていません。", nameof(modelData)) : meshData.Tangents[j],
                        UV = meshData.UVs is null ? default : meshData.UVs[j],
                    };
                }

                Rendering.Material material;
                if (0 <= meshData.MaterialIndex)
                {
                    material = materials[meshData.MaterialIndex];
                }
                else
                {
                    if (defaultMaterial is null)
                    {
                        defaultMaterial = Rendering.Material.Default();
                        materials.Add(defaultMaterial);
                    }

                    material = defaultMaterial;
                }

                meshes[i] = Rendering.Mesh.Create(Context.Device, meshData.Name, vertices, meshData.Indices, material);
            }

            TexturesKey.UnionWith(loadedTextures.Values);

            Rendering.Model model = new(meshes, materials);
            return new ModelResourceSet(model);


            ID3D11ShaderResourceView? LoadTexture(TextureReference textureRef, bool isLinear, IReadOnlyDictionary<string, EmbeddedTexture> embeddedTextures)
            {
                if (loadedTextures.TryGetValue(textureRef.Key, out ID3D11ShaderResourceView? texture))
                {
                    return texture;
                }

                if (textureRef.Type == TextureReference.TextureType.File && SharedTextures.TryGetValue(textureRef.Key, out texture))
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
                            texture = TextureFactory.CreateFromFile(filePath, isLinear, textureErrorCollector);
                        }
                        catch (FileNotFoundException ex)
                        {
                            ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                ErrorLevel.Error, $"テクスチャファイル '{filePath}' が見つかりませんでした。", sourceLocation)
                            {
                                Exception = ex,
                            };
                            ErrorCollector.Report(error);
                        }
                        catch (Exception ex)
                        {
                            ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                ErrorLevel.Error, $"テクスチャファイル '{filePath}' を読み込めませんでした。", filePath)
                            {
                                Exception = ex,
                            };
                            ErrorCollector.Report(error);
                        }
                        break;
                    }

                    case TextureReference.TextureType.Embedded:
                    {
                        try
                        {
                            texture = TextureFactory.CreateFromMemory(embeddedTextures[textureRef.Key], isLinear, textureErrorCollector);
                        }
                        catch (Exception ex)
                        {
                            ModelLoadError error = new(ModelLoadError.ErrorSource.Data, ModelLoadError.ErrorTarget.Visual,
                                ErrorLevel.Error, $"埋め込みテクスチャ '{textureRef.Key}' を読み込めませんでした。", sourceLocation)
                            {
                                Exception = ex,
                            };
                            ErrorCollector.Report(error);
                        }
                        break;
                    }
                }

                if (texture is not null)
                {
                    loadedTextures.Add(textureRef.Key, texture);

                    if (textureRef.Type == TextureReference.TextureType.File)
                    {
                        SharedTextures.Add(textureRef.Key, texture);
                    }
                }

                return texture;
            }
        }
    }
}
