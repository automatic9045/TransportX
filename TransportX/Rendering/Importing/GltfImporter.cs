using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using GltfMaterial = SharpGLTF.Schema2.Material;
using GltfTexture = SharpGLTF.Schema2.Texture;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal class GltfImporter : IModelImporter
    {
        private readonly IErrorCollector ErrorCollector;

        public GltfImporter(IErrorCollector errorCollector)
        {
            ErrorCollector = errorCollector;
        }

        public void Dispose()
        {
        }

        public Model Import(string path, bool isForVisual, bool makeLH)
        {
            if (!makeLH) throw new NotSupportedException();

            try
            {
                ModelRoot modelRoot;
                try
                {
                    modelRoot = ModelRoot.Load(path);
                }
                catch (FileNotFoundException ex)
                {
                    return Abort($"3D モデル '{path}' が見つかりませんでした。", ex);
                }
                catch (Exception ex)
                {
                    return Abort($"3D モデル '{path}' を読み込めませんでした。", ex);
                }


                List<Mesh> meshes = new List<Mesh>();
                foreach (Node node in modelRoot.DefaultScene.VisualChildren)
                {
                    CreateMeshes(node, Matrix4x4.Identity);
                }

                void CreateMeshes(Node node, Matrix4x4 parentTransform)
                {
                    Matrix4x4 transform = node.LocalMatrix * parentTransform;

                    foreach (MeshPrimitive primitive in node.Mesh.Primitives)
                    {
                        Mesh? meshData = CreateMesh(primitive, node.Name, transform);
                        if (meshData is not null)
                        {
                            meshes.Add(meshData.Value);
                        }
                    }

                    foreach (Node child in node.VisualChildren)
                    {
                        CreateMeshes(child, transform);
                    }
                }

                Mesh? CreateMesh(MeshPrimitive primitive, string nodeName, Matrix4x4 transform)
                {
                    if (primitive.DrawPrimitiveType != PrimitiveType.TRIANGLES) return null;


                    if (!primitive.VertexAccessors.TryGetValue("POSITION", out Accessor? positionAccessor))
                    {
                        ReportError(ModelLoadError.ErrorSource.Data, ErrorLevel.Error, "モデルのフォーマットが不正です。頂点情報が定義されていません。");
                        return null;
                    }

                    IAccessorArray<Vector3> rawVertices = positionAccessor.AsVector3Array();
                    Vector3[] vertices = new Vector3[rawVertices.Count];
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 vertex = Vector3.Transform(rawVertices[i], transform);
                        vertex.Z = -vertex.Z; // 左手系に変換
                        vertices[i] = vertex;
                    }


                    int[] indices;
                    if (primitive.IndexAccessor is not null)
                    {
                        IAccessorArray<uint> rawIndices = primitive.IndexAccessor.AsIndicesArray();
                        indices = new int[rawIndices.Count];
                        for (int i = 0; i < indices.Length; i += 3) // 左手系に変換
                        {
                            indices[i] = (int)rawIndices[i];
                            indices[i + 1] = (int)rawIndices[i + 2];
                            indices[i + 2] = (int)rawIndices[i + 1];
                        }
                    }
                    else
                    {
                        indices = new int[vertices.Length];
                        for (int i = 0; i < indices.Length; i += 3) // 左手系に変換
                        {
                            indices[i] = i;
                            indices[i + 1] = i + 2;
                            indices[i + 2] = i + 1;
                        }
                    }


                    Vector2[]? textureCoords = null;
                    if (isForVisual && primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out Accessor? texcoordAccessor))
                    {
                        IAccessorArray<Vector2> rawTextureCoords = texcoordAccessor.AsVector2Array();
                        textureCoords = rawTextureCoords.ToArray();
                    }


                    Vector3[]? normals = null;
                    if (isForVisual)
                    {
                        if (primitive.VertexAccessors.TryGetValue("NORMAL", out Accessor? normalAccessor))
                        {
                            IAccessorArray<Vector3> rawNormals = normalAccessor.AsVector3Array();
                            normals = new Vector3[rawNormals.Count];
                            for (int i = 0; i < vertices.Length; i++)
                            {
                                Vector3 normal = Vector3.TransformNormal(rawNormals[i], transform);
                                normal.Z = -normal.Z; // 左手系に変換
                                normals[i] = normal;
                            }
                        }
                        else
                        {
                            normals = new Vector3[vertices.Length];

                            for (int i = 0; i < indices.Length; i += 3)
                            {
                                int index0 = indices[i];
                                int index1 = indices[i + 1];
                                int index2 = indices[i + 2];

                                Vector3 edge01 = vertices[index1] - vertices[index0];
                                Vector3 edge02 = vertices[index2] - vertices[index0];

                                Vector3 faceNormal = Vector3.Cross(edge01, edge02);

                                normals[index0] += faceNormal;
                                normals[index1] += faceNormal;
                                normals[index2] += faceNormal;
                            }

                            for (int i = 0; i < normals.Length; i++)
                            {
                                normals[i] = 1e-6f < normals[i].LengthSquared() ? Vector3.Normalize(normals[i]) : Vector3.UnitY;
                            }
                        }
                    }


                    Vector3[]? tangents = null;
                    if (isForVisual)
                    {
                        if (primitive.VertexAccessors.TryGetValue("TANGENT", out Accessor? tangentAccessor))
                        {
                            IAccessorArray<Vector4> rawTangents = tangentAccessor.AsVector4Array();
                            tangents = new Vector3[rawTangents.Count];
                            for (int i = 0; i < vertices.Length; i++)
                            {
                                Vector3 tangent = Vector3.TransformNormal(rawTangents[i].AsVector3(), transform);
                                tangent.Z = -tangent.Z; // 左手系に変換
                                tangents[i] = tangent;
                            }
                        }
                        else
                        {
                            tangents = new Vector3[vertices.Length];

                            if (textureCoords is null)
                            {
                                Array.Fill(tangents, Vector3.UnitX);
                            }
                            else
                            {
                                for (int i = 0; i < indices.Length; i += 3)
                                {
                                    int index0 = indices[i];
                                    int index1 = indices[i + 1];
                                    int index2 = indices[i + 2];

                                    Vector3 vertex0 = vertices[index0];
                                    Vector3 vertex1 = vertices[index1];
                                    Vector3 vertex2 = vertices[index2];

                                    Vector3 edge01 = vertex1 - vertex0;
                                    Vector3 edge02 = vertex2 - vertex0;

                                    Vector2 uv0 = textureCoords[index0];
                                    Vector2 uv1 = textureCoords[index1];
                                    Vector2 uv2 = textureCoords[index2];

                                    Vector2 uvEdge01 = uv1 - uv0;
                                    Vector2 uvEdge02 = uv2 - uv0;

                                    float r = 1 / (uvEdge01.X * uvEdge02.Y - uvEdge02.X * uvEdge01.Y);
                                    if (float.IsNaN(r) || float.IsInfinity(r)) r = 1;

                                    Vector3 uDirection = new Vector3(
                                        uvEdge02.Y * edge01.X - uvEdge01.Y * edge02.X,
                                        uvEdge02.Y * edge01.Y - uvEdge01.Y * edge02.Y,
                                        uvEdge02.Y * edge01.Z - uvEdge01.Y * edge02.Z) * r;

                                    tangents[index0] += uDirection;
                                    tangents[index1] += uDirection;
                                    tangents[index2] += uDirection;
                                }

                                for (int i = 0; i < tangents.Length; i++)
                                {
                                    Vector3 n = normals![i];
                                    Vector3 t = tangents[i];

                                    Vector3 tangent = t - n * Vector3.Dot(n, t);
                                    if (tangent.LengthSquared() < 1e-6f)
                                    {
                                        tangents[i] = Vector3.UnitX;
                                    }
                                    else
                                    {
                                        tangents[i] = Vector3.Normalize(tangent);
                                    }
                                }
                            }
                        }
                    }


                    Vector4[] colors;
                    if (isForVisual && primitive.VertexAccessors.TryGetValue("COLOR_0", out Accessor? colorAccessor))
                    {
                        colors = colorAccessor.AsVector4Array().ToArray();
                    }
                    else
                    {
                        colors = new Vector4[vertices.Length];
                        Array.Fill(colors, Vector4.One);
                    }


                    return new Mesh()
                    {
                        Name = nodeName ?? primitive.LogicalParent.Name,
                        Vertices = vertices,
                        Indices = indices,
                        Normals = normals,
                        Tangents = tangents,
                        Colors = colors,
                        TextureCoords = textureCoords,
                        MaterialIndex = primitive.Material?.LogicalIndex ?? -1,
                    };
                }


                Material[] materials = new Material[modelRoot.LogicalMaterials.Count];
                for (int i = 0; i < materials.Length; i++)
                {
                    GltfMaterial gltfMaterial = modelRoot.LogicalMaterials[i];

                    if (isForVisual)
                    {
                        Vector4 baseColor = Vector4.One;
                        TextureReference? baseColorTexture = null;
                        MaterialChannel? baseColorChannel = gltfMaterial.FindChannel("BaseColor");
                        if (baseColorChannel.HasValue)
                        {
                            baseColor = (Vector4)baseColorChannel.Value.Parameters[0].Value;
                            baseColorTexture = CreateTextureRef(baseColorChannel.Value.Texture);
                        }

                        TextureReference? occlusionTexture = null;
                        MaterialChannel? occlusionChannel = gltfMaterial.FindChannel("Occlusion");
                        if (occlusionChannel.HasValue)
                        {
                            occlusionTexture = CreateTextureRef(occlusionChannel.Value.Texture);
                        }

                        float metallic = 1;
                        float roughness = 1;
                        TextureReference? metallicRoughnessTexture = null;
                        MaterialChannel? metallicRoughnessChannel = gltfMaterial.FindChannel("MetallicRoughness");
                        if (metallicRoughnessChannel.HasValue)
                        {
                            metallic = (float)metallicRoughnessChannel.Value.Parameters[0].Value;
                            roughness = (float)metallicRoughnessChannel.Value.Parameters[1].Value;
                            metallicRoughnessTexture = CreateTextureRef(metallicRoughnessChannel.Value.Texture);
                        }

                        TextureReference? normalTexture = null;
                        MaterialChannel? normalChannel = gltfMaterial.FindChannel("Normal");
                        if (normalChannel.HasValue)
                        {
                            normalTexture = CreateTextureRef(normalChannel.Value.Texture);
                        }

                        Vector3 emissive = Vector3.Zero;
                        TextureReference? emissiveTexture = null;
                        MaterialChannel? emissiveChannel = gltfMaterial.FindChannel("Emissive");
                        if (emissiveChannel.HasValue)
                        {
                            emissive = (Vector3)emissiveChannel.Value.Parameters[0].Value;
                            emissiveTexture = CreateTextureRef(emissiveChannel.Value.Texture);
                        }

                        materials[i] = new Material()
                        {
                            Name = gltfMaterial.Name,

                            BaseColor = baseColor,
                            Roughness = roughness,
                            Metallic = metallic,
                            Emissive = emissive,

                            BaseColorTexture = baseColorTexture,
                            NormalTexture = normalTexture,
                            OcclusionTexture = occlusionTexture,
                            RoughnessTexture = metallicRoughnessTexture,
                            MetallicTexture = metallicRoughnessTexture,
                            EmissiveTexture = emissiveTexture,
                        };
                    }
                    else
                    {
                        materials[i] = new Material()
                        {
                            Name = gltfMaterial.Name,

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

                TextureReference? CreateTextureRef(GltfTexture? texture)
                {
                    if (texture is null) return null;

                    Image image = texture.PrimaryImage;
                    if (image.Content.IsEmpty)
                    {
                        if (!string.IsNullOrEmpty(image.Content.SourcePath))
                        {
                            return new TextureReference(TextureReference.TextureType.File, image.Content.SourcePath);
                        }
                    }
                    else
                    {
                        return new TextureReference(TextureReference.TextureType.Embedded, $"*{image.LogicalIndex}");
                    }

                    return null;
                }


                Dictionary<string, EmbeddedTexture> embeddedTextures = [];
                if (isForVisual)
                {
                    foreach (Image gltfImage in modelRoot.LogicalImages)
                    {
                        if (gltfImage.Content.IsEmpty) continue;

                        string key = $"*{gltfImage.LogicalIndex}";

                        TextureFormat format = gltfImage.Content switch
                        {
                            MemoryImage image when image.IsDds => TextureFormat.DDS,
                            _ => TextureFormat.WIC,
                        };

                        EmbeddedTexture texture = new()
                        {
                            Key = key,
                            Data = gltfImage.Content.Content,
                            Format = format,
                            Width = 0,
                            Height = 0,
                        };
                        embeddedTextures[key] = texture;
                    }
                }


                return new Model(meshes.ToArray(), materials, embeddedTextures);
            }
            catch (Exception ex)
            {
                return Abort($"glTF モデル '{path}' の解析中にエラーが発生しました。", ex);
            }


            Model Abort(string message, Exception exception)
            {
                ReportError(ModelLoadError.ErrorSource.Reference, ErrorLevel.Error, message, exception);
                return new Model([], [], new Dictionary<string, EmbeddedTexture>());
            }

            void ReportError(ModelLoadError.ErrorSource source, ErrorLevel level, string message, Exception? exception = null)
            {
                ModelLoadError.ErrorTarget target = isForVisual ? ModelLoadError.ErrorTarget.Visual : ModelLoadError.ErrorTarget.Collision;
                ModelLoadError error = new(source, target, level, message, path)
                {
                    Exception = exception,
                };
                ErrorCollector.Report(error);
            }
        }
    }
}
