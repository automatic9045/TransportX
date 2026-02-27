using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Assimp;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal class AssimpImporter : IModelImporter
    {
        // 参考: https://github.com/assimp/assimp/blob/master/samples/SimpleTexturedDirectx11/SimpleTexturedDirectx11/ModelLoader.cpp

        private readonly AssimpContext Importer = new();
        private readonly IErrorCollector ErrorCollector;

        public AssimpImporter(IErrorCollector errorCollector)
        {
            ErrorCollector = errorCollector;

            //Importer.SetConfig(new SortByPrimitiveTypeConfig(PrimitiveType.Point | PrimitiveType.Line));
        }

        public void Dispose()
        {
            Importer.Dispose();
        }

        public Model Import(string path, bool isForVisual, bool makeLH)
        {
            PostProcessSteps steps = PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.SortByPrimitiveType;
            if (isForVisual) steps |= PostProcessSteps.GenerateNormals | PostProcessSteps.CalculateTangentSpace;

            if (makeLH)
            {
                steps |= PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipWindingOrder;
                if (isForVisual) steps |= PostProcessSteps.FlipUVs;
            }

            try
            {
                Scene scene = Importer.ImportFile(path, steps);


                Mesh[] meshes = new Mesh[scene.MeshCount];
                for (int i = 0; i < meshes.Length; i++)
                {
                    Assimp.Mesh assimpMesh = scene.Meshes[i];

                    Vector3[] vertices = assimpMesh.Vertices.ToArray();

                    List<int> indices = new List<int>();
                    foreach (Face face in assimpMesh.Faces)
                    {
                        if (face.IndexCount != 3) throw new NotSupportedException();
                        indices.AddRange(face.Indices);
                    }

                    Vector3[]? normals = isForVisual ? assimpMesh.Normals.ToArray() : null;
                    Vector3[]? tangents = isForVisual ? assimpMesh.Tangents.ToArray() : null;

                    Vector4[]? colors = null;
                    if (assimpMesh.HasVertexColors(0))
                    {
                        List<Vector4> channel = assimpMesh.VertexColorChannels[0];

                        colors = new Vector4[channel.Count];
                        for (int j = 0; i < colors.Length; j++)
                        {
                            colors[j] = channel[j].ToLinear();
                        }
                    }

                    Vector2[]? textureCoords = null;
                    if (assimpMesh.HasTextureCoords(0))
                    {
                        textureCoords = new Vector2[assimpMesh.VertexCount];
                        List<Vector3> channel = assimpMesh.TextureCoordinateChannels[0];
                        for (int j = 0; j < textureCoords.Length; j++)
                        {
                            textureCoords[j] = channel[j].AsVector2();
                        }
                    }

                    meshes[i] = new Mesh()
                    {
                        Name = assimpMesh.Name,
                        Vertices = vertices,
                        Indices = indices.ToArray(),
                        Normals = normals,
                        Tangents = tangents,
                        Colors = colors,
                        TextureCoords = textureCoords,
                        MaterialIndex = assimpMesh.MaterialIndex,
                    };
                }


                Material[] materials = new Material[scene.MaterialCount];
                for (int i = 0; i < materials.Length; i++)
                {
                    Assimp.Material assimpMaterial = scene.Materials[i];

                    if (isForVisual)
                    {
                        Vector4 baseColor;
                        float metallic;
                        float roughness;
                        Vector4 emissive = Vector4.Zero;

                        TextureReference? baseColorTexture = null;
                        TextureReference? normalTexture = null;
                        TextureReference? occlusionTexture = null;
                        TextureReference? roughnessTexture = null;
                        TextureReference? metallicTexture = null;
                        TextureReference? emissiveTexture = null;

                        if (assimpMaterial.IsPBRMaterial)
                        {
                            if (!assimpMaterial.TryGetBaseColor(out baseColor))
                            {
                                baseColor = assimpMaterial.HasColorDiffuse ? assimpMaterial.ColorDiffuse : Vector4.One;
                            }

                            if (!assimpMaterial.TryGetMetallicFactor(out metallic))
                            {
                                metallic = 1;
                            }

                            if (!assimpMaterial.TryGetRoughnessFactor(out roughness))
                            {
                                roughness = 1;
                            }

                            baseColorTexture = assimpMaterial.PBR.HasTextureBaseColor ? CreateTextureRef(assimpMaterial.PBR.TextureBaseColor)
                                : assimpMaterial.HasTextureDiffuse ? CreateTextureRef(assimpMaterial.TextureDiffuse)
                                : null;

                            normalTexture = assimpMaterial.PBR.HasTextureNormalCamera ? CreateTextureRef(assimpMaterial.PBR.TextureNormalCamera)
                                : assimpMaterial.HasTextureNormal ? CreateTextureRef(assimpMaterial.TextureNormal)
                                : null;

                            roughnessTexture = assimpMaterial.PBR.HasTextureRoughness ? CreateTextureRef(assimpMaterial.PBR.TextureRoughness)
                                : null;

                            metallicTexture = assimpMaterial.PBR.HasTextureMetalness ? CreateTextureRef(assimpMaterial.PBR.TextureMetalness)
                                : null;
                        }
                        else
                        {
                            baseColor = assimpMaterial.HasColorDiffuse ? assimpMaterial.ColorDiffuse : Vector4.One;

                            metallic = 0;

                            roughness = 1;
                            if (assimpMaterial.HasShininess)
                            {
                                if (0 < assimpMaterial.Shininess)
                                {
                                    roughness = float.Clamp(1 - (float.Sqrt(assimpMaterial.Shininess) * 0.1f), 0.05f, 1);
                                }
                            }

                            baseColorTexture = assimpMaterial.HasTextureDiffuse ? CreateTextureRef(assimpMaterial.TextureDiffuse)
                                : null;

                            normalTexture = assimpMaterial.HasTextureNormal ? CreateTextureRef(assimpMaterial.TextureNormal)
                                : null;
                        }

                        occlusionTexture = assimpMaterial.HasTextureAmbientOcclusion ? CreateTextureRef(assimpMaterial.TextureAmbientOcclusion)
                            : null;

                        if (assimpMaterial.HasColorEmissive)
                        {
                            emissive = assimpMaterial.ColorEmissive;
                        }

                        emissiveTexture = assimpMaterial.HasTextureEmissive ? CreateTextureRef(assimpMaterial.TextureEmissive)
                            : null;

                        materials[i] = new Material()
                        {
                            Name = assimpMaterial.Name,

                            BaseColor = baseColor.ToLinear(),
                            Metallic = metallic,
                            Roughness = roughness,
                            Emissive = emissive.ToLinear().AsVector3(),

                            BaseColorTexture = baseColorTexture,
                            NormalTexture = normalTexture,
                            OcclusionTexture = occlusionTexture,
                            RoughnessTexture = roughnessTexture,
                            MetallicTexture = metallicTexture,
                            EmissiveTexture = emissiveTexture,
                        };
                    }
                    else
                    {
                        materials[i] = new Material()
                        {
                            Name = assimpMaterial.Name,

                            BaseColor = Vector4.Zero,
                            Metallic = 0,
                            Roughness = 0,
                            Emissive = Vector3.Zero,

                            BaseColorTexture = null,
                            NormalTexture = null,
                            OcclusionTexture = null,
                            RoughnessTexture = null,
                            MetallicTexture = null,
                            EmissiveTexture = null,
                        };
                    }
                }

                TextureReference? CreateTextureRef(TextureSlot slot)
                {
                    if (string.IsNullOrEmpty(slot.FilePath)) return null;

                    if (scene.GetEmbeddedTexture(slot.FilePath) is not null)
                    {
                        return new TextureReference(TextureReference.TextureType.Embedded, slot.FilePath);
                    }
                    else
                    {
                        return new TextureReference(TextureReference.TextureType.File, slot.FilePath);
                    }
                }


                Dictionary<string, EmbeddedTexture> embeddedTextures = [];
                if (isForVisual)
                {
                    for (int i = 0; i < scene.TextureCount; i++)
                    {
                        Assimp.EmbeddedTexture assimpTexture = scene.Textures[i];

                        byte[] textureData;
                        int width, height;
                        TextureFormat format;

                        if (assimpTexture.IsCompressed)
                        {
                            textureData = assimpTexture.CompressedData;

                            format = textureData switch
                            {
                                _ when textureData.AsSpan().StartsWith("DDS "u8) => TextureFormat.DDS,
                                _ => TextureFormat.WIC,
                            };

                            width = 0;
                            height = 0;
                        }
                        else
                        {
                            ReadOnlySpan<Texel> texelSpan = assimpTexture.NonCompressedData.AsSpan();
                            textureData = MemoryMarshal.AsBytes(texelSpan).ToArray();
                            format = TextureFormat.Uncompressed;
                            width = assimpTexture.Width;
                            height = assimpTexture.Height;
                        }

                        EmbeddedTexture texture = new()
                        {
                            Key = $"*{i}",
                            Data = textureData,
                            Format = format,
                            Width = width,
                            Height = height,
                        };
                        embeddedTextures[texture.Key] = texture;

                        if (!string.IsNullOrEmpty(assimpTexture.Filename) && !embeddedTextures.ContainsKey(assimpTexture.Filename))
                        {
                            embeddedTextures[assimpTexture.Filename] = texture;
                        }
                    }
                }

                return new Model(meshes, materials, embeddedTextures);
            }
            catch (FileNotFoundException ex)
            {
                return ReportError(ModelLoadError.ErrorSource.Reference, ErrorLevel.Error, $"3D モデル '{path}' が見つかりませんでした。", ex);
            }
            catch (Exception ex)
            {
                return ReportError(ModelLoadError.ErrorSource.Reference, ErrorLevel.Error, $"3D モデル '{path}' を読み込めませんでした。", ex);
            }


            Model ReportError(ModelLoadError.ErrorSource source, ErrorLevel level, string message, Exception exception)
            {
                ModelLoadError.ErrorTarget target = isForVisual ? ModelLoadError.ErrorTarget.Visual : ModelLoadError.ErrorTarget.Collision;
                ModelLoadError error = new(source, target, level, message, path)
                {
                    Exception = exception,
                };
                ErrorCollector.Report(error);

                return new Model([], [], new Dictionary<string, EmbeddedTexture>());
            }
        }
    }
}
