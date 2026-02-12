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


                    Vector3[] normals;
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


                    Vector2[]? textureCoords = null;
                    if (isForVisual && primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out Accessor? texcoordAccessor))
                    {
                        IAccessorArray<Vector2> rawTextureCoords = texcoordAccessor.AsVector2Array();
                        textureCoords = new Vector2[rawTextureCoords.Count];
                        for (int i = 0; i < textureCoords.Length; i++)
                        {
                            textureCoords[i] = new Vector2(rawTextureCoords[i].X, rawTextureCoords[i].Y); // UV 反転
                        }
                    }


                    return new Mesh()
                    {
                        Name = nodeName ?? primitive.LogicalParent.Name,
                        Vertices = vertices,
                        Indices = indices,
                        Normals = normals,
                        Colors = colors,
                        TextureCoords = textureCoords,
                        MaterialIndex = primitive.Material?.LogicalIndex ?? -1,
                    };
                }


                Material[] materials = new Material[modelRoot.LogicalMaterials.Count];
                for (int i = 0; i < materials.Length; i++)
                {
                    GltfMaterial gltfMaterial = modelRoot.LogicalMaterials[i];

                    Vector4 baseColor = Vector4.One;
                    MaterialChannel? pbr = gltfMaterial.FindChannel("BaseColor");
                    if (pbr.HasValue)
                    {
                        baseColor = (Vector4)pbr.Value.Parameters[0].Value;
                    }

                    List<TextureReference> textureRefs = [];
                    if (pbr.HasValue && pbr.Value.Texture is not null)
                    {
                        Image gltfImage = pbr.Value.Texture.PrimaryImage;
                        if (gltfImage.Content.IsEmpty)
                        {
                            if (gltfImage.Content.SourcePath != string.Empty)
                            {
                                textureRefs.Add(new TextureReference(TextureReference.TextureType.File, gltfImage.Content.SourcePath));
                            }
                        }
                        else
                        {
                            textureRefs.Add(new TextureReference(TextureReference.TextureType.Embedded, $"*{gltfImage.LogicalIndex}"));
                        }
                    }

                    materials[i] = new Material()
                    {
                        Name = gltfMaterial.Name,
                        BaseColor = baseColor,
                        Textures = textureRefs.ToArray(),
                    };
                }


                Dictionary<string, Texture> embeddedTextures = [];
                foreach (Image gltfImage in modelRoot.LogicalImages)
                {
                    if (gltfImage.Content.IsEmpty) continue;

                    string key = $"*{gltfImage.LogicalIndex}";

                    Texture texture = new()
                    {
                        Key = key,
                        Data = gltfImage.Content.Content,
                        Width = 0,
                        Height = 0,
                        FormatHint = gltfImage.Content.FileExtension,
                    };
                    embeddedTextures[key] = texture;
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
                return new Model([], [], new Dictionary<string, Texture>());
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
