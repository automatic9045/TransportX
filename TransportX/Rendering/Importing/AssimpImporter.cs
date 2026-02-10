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
            if (isForVisual) steps |= PostProcessSteps.GenerateNormals;

            if (makeLH)
            {
                steps |= PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipWindingOrder;
                if (isForVisual) steps |= PostProcessSteps.FlipUVs;
            }

            try
            {
                byte[] dataf = File.ReadAllBytes(path);
                using MemoryStream stream = new(dataf);
                string extension = Path.GetExtension(path).TrimStart('.');
                Scene scene = Importer.ImportFileFromStream(stream, steps, extension);

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

                    Vector3[] normals = assimpMesh.Normals.ToArray();

                    Vector4[]? colors = null;
                    if (assimpMesh.HasVertexColors(0))
                    {
                        List<Vector4> channel = assimpMesh.VertexColorChannels[0];
                        colors = channel.ToArray();
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
                        Colors = colors,
                        TextureCoords = textureCoords,
                        MaterialIndex = assimpMesh.MaterialIndex,
                    };
                }

                Material[] materials = new Material[scene.MaterialCount];
                for (int i = 0; i < materials.Length; i++)
                {
                    Assimp.Material assimpMaterial = scene.Materials[i];

                    List<TextureReference> textureRefs = [];
                    int textureCount = assimpMaterial.GetMaterialTextureCount(TextureType.Diffuse);
                    for (int j = 0; j < textureCount; j++)
                    {
                        if (assimpMaterial.GetMaterialTexture(TextureType.Diffuse, j, out TextureSlot slot))
                        {
                            if (scene.GetEmbeddedTexture(slot.FilePath) is not null)
                            {
                                textureRefs.Add(new TextureReference(TextureReference.TextureType.Embedded, slot.FilePath));
                            }
                            else
                            {
                                textureRefs.Add(new TextureReference(TextureReference.TextureType.File, slot.FilePath));
                            }
                        }
                    }

                    materials[i] = new Material()
                    {
                        Name = assimpMaterial.Name,
                        BaseColor = assimpMaterial.ColorDiffuse,
                        Textures = textureRefs.ToArray(),
                    };
                }

                Dictionary<string, Texture> embeddedTextures = [];
                for (int i = 0; i < scene.TextureCount; i++)
                {
                    EmbeddedTexture assimpTexture = scene.Textures[i];

                    byte[] data;
                    int width = assimpTexture.Width;
                    int height = assimpTexture.Height;
                    string formatHint = assimpTexture.CompressedFormatHint;

                    if (assimpTexture.IsCompressed)
                    {
                        data = assimpTexture.CompressedData;
                        width = 0;
                        height = 0;
                    }
                    else
                    {
                        ReadOnlySpan<Texel> texelSpan = assimpTexture.NonCompressedData.AsSpan();
                        data = MemoryMarshal.AsBytes(texelSpan).ToArray();
                        formatHint = "argb8888";
                    }

                    Texture texture = new Texture()
                    {
                        Key = $"*{i}",
                        Data = data,
                        Width = width,
                        Height = height,
                        FormatHint = formatHint,
                    };
                    embeddedTextures[texture.Key] = texture;
                }

                return new Model(meshes, materials, embeddedTextures);
            }
            catch (FileNotFoundException ex)
            {
                return ReportError($"3D モデル '{path}' が見つかりませんでした。", ex);
            }
            catch (Exception ex)
            {
                return ReportError($"3D モデル '{path}' を読み込めませんでした。", ex);
            }


            Model ReportError(string message, Exception exception)
            {
                ModelLoadErrorTypes errorTypes = ModelLoadErrorTypes.Critical | (isForVisual ? ModelLoadErrorTypes.Visual : ModelLoadErrorTypes.Collision);
                ModelLoadError error = new(errorTypes, ErrorLevel.Error, message, path)
                {
                    Exception = exception,
                };
                ErrorCollector.Report(error);

                return new Model([], [], new Dictionary<string, Texture>());
            }
        }
    }
}
