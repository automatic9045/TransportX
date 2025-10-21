using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Assimp;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Vortice.Direct3D11;
using Vortice.DXGI;

using CollisionMesh = BepuPhysics.Collidables.Mesh;

using Bus.Common.Physics;

namespace Bus.Common.Rendering
{
    internal class AssimpModelFactory
    {
        // 参考: https://github.com/assimp/assimp/blob/master/samples/SimpleTexturedDirectx11/SimpleTexturedDirectx11/ModelLoader.cpp 

        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext Context;
        private readonly Simulation? Simulation;

        private readonly AssimpContext Importer = new AssimpContext();
        private readonly List<TextureInfo> LoadedTextures = new List<TextureInfo>();

        public bool IsCollisionSupported => Simulation is not null;

        public AssimpModelFactory(ID3D11Device device, ID3D11DeviceContext context, Simulation? simulation = null)
        {
            Device = device;
            Context = context;
            Simulation = simulation;
        }

        private Model Load(Scene visualScene, string baseDirectory)
        {
            List<Mesh> visualMeshes = visualScene.Meshes.ConvertAll(assimpMesh =>
            {
                Vertex[] vertices = assimpMesh.Vertices
                    .Select((assimpVertex, i) =>
                    {
                        Vertex vertex = new Vertex()
                        {
                            X = assimpVertex.X,
                            Y = assimpVertex.Y,
                            Z = assimpVertex.Z,
                        };

                        if (assimpMesh.HasTextureCoords(0))
                        {
                            Vector3 assimpTextureCoord = assimpMesh.TextureCoordinateChannels[0][i];
                            vertex.TextureCoord = new Vector2(assimpTextureCoord.X, assimpTextureCoord.Y);
                        }

                        return vertex;
                    })
                    .ToArray();

                List<int> indices = new List<int>();
                foreach (Face face in assimpMesh.Faces)
                {
                    indices.AddRange(face.Indices);
                }

                List<ID3D11ShaderResourceView> textures = new List<ID3D11ShaderResourceView>();
                if (0 <= assimpMesh.MaterialIndex)
                {
                    Assimp.Material material = visualScene.Materials[assimpMesh.MaterialIndex];

                    List<ID3D11ShaderResourceView> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse", visualScene, baseDirectory);
                    textures.AddRange(diffuseMaps);
                }

                return Mesh.Create(Device, vertices, indices.ToArray(), textures);
            });

            return new Model(visualMeshes, LoadedTextures.ConvertAll(x => x.Texture));
        }

        private Scene LoadVisualScene(string visualModelPath)
        {
            Scene visualScene = Importer.ImportFile(visualModelPath,
                PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);
            return visualScene;
        }

        public Model Load(string visualModelPath)
        {
            string baseDirectory = Path.GetDirectoryName(visualModelPath)!;
            Scene visualScene = LoadVisualScene(visualModelPath);

            return Load(visualScene, baseDirectory);
        }

        private void CheckCollisionSupported()
        {
            if (!IsCollisionSupported) throw new NotSupportedException($"{nameof(Simulation)} が指定されていないため、衝突判定を読み込むことはできません。");
        }

        public CollidableModel LoadWithBoundingBox(string visualModelPath)
        {
            CheckCollisionSupported();

            string baseDirectory = Path.GetDirectoryName(visualModelPath)!;
            Scene visualScene = LoadVisualScene(visualModelPath);
            Model baseModel = Load(visualScene, baseDirectory);

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (Assimp.Mesh mesh in visualScene.Meshes)
            {
                foreach (Vector3 vertex in mesh.Vertices)
                {
                    if (vertex.X < min.X) min.X = vertex.X;
                    if (vertex.Y < min.Y) min.Y = vertex.Y;
                    if (vertex.Z < min.Z) min.Z = vertex.Z;

                    if (max.X < vertex.X) max.X = vertex.X;
                    if (max.Y < vertex.Y) max.Y = vertex.Y;
                    if (max.Z < vertex.Z) max.Z = vertex.Z;
                }
            }

            Box box = new Box(max.X - min.X, max.Y - min.Y, max.Z - min.Z);
            Vector3 origin = (min + max) / 2;
            Matrix4x4 colliderOffset = Matrix4x4.CreateTranslation(origin);
            Collider<Box> collider = ColliderFactory.Box(Simulation!, box, colliderOffset);

            return new CollidableModel(baseModel, collider);
        }

        public CollidableModel LoadWithConvexHull(string visualModelPath)
        {
            CheckCollisionSupported();

            string baseDirectory = Path.GetDirectoryName(visualModelPath)!;
            Scene visualScene = LoadVisualScene(visualModelPath);
            Model baseModel = Load(visualScene, baseDirectory);

            Simulation!.BufferPool.Take(visualScene.Meshes.Sum(mesh => mesh.VertexCount), out Buffer<Vector3> pointBuffer);
            int i = 0;
            foreach (Assimp.Mesh mesh in visualScene.Meshes)
            {
                foreach (Vector3 vertex in mesh.Vertices)
                {
                    pointBuffer[i] = vertex;
                    i++;
                }
            }

            ConvexHullHelper.CreateShape(pointBuffer, Simulation.BufferPool, out Vector3 center, out ConvexHull convexHull);
            Collider<ConvexHull> collider = ColliderFactory.ConvexHull(Simulation, convexHull, Matrix4x4.CreateTranslation(center));

            return new CollidableModel(baseModel, collider);
        }

        public CollidableModel LoadWithCollisionModel(string visualModelPath, string collisionModelPath, bool isOpen)
        {
            CheckCollisionSupported();

            Model baseModel = Load(visualModelPath);

            Scene collisionScene = Importer.ImportFile(collisionModelPath,
                PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate);
            Simulation!.BufferPool.Take(collisionScene.Meshes.Sum(mesh => mesh.FaceCount), out Buffer<Triangle> triangles);

            int i = 0;
            foreach (Assimp.Mesh mesh in collisionScene.Meshes)
            {
                foreach (Face face in mesh.Faces)
                {
                    if (face.IndexCount != 3) throw new NotSupportedException("指定されたモデルは衝突判定に利用できません。");
                    triangles[i] = new Triangle(mesh.Vertices[face.Indices[2]], mesh.Vertices[face.Indices[1]], mesh.Vertices[face.Indices[0]]);
                    i++;
                }
            }

            CollisionMesh collisionMesh = new CollisionMesh(triangles, Vector3.One, Simulation.BufferPool);
            Vector3 center = isOpen ? collisionMesh.ComputeOpenCenterOfMass() : collisionMesh.ComputeClosedCenterOfMass();
            if (float.IsNaN(center.X + center.Y + center.Z)) throw new InvalidOperationException("モデルの重心を特定できません。");
            collisionMesh.Recenter(center);
            Collider<CollisionMesh> collider = ColliderFactory.Mesh(Simulation!, collisionMesh, Matrix4x4.CreateTranslation(center), isOpen);

            return new CollidableModel(baseModel, collider);
        }

        private List<ID3D11ShaderResourceView> LoadMaterialTextures(Assimp.Material material, TextureType type, string typeName, Scene scene, string baseDirectory)
        {
            List<ID3D11ShaderResourceView> textures = new List<ID3D11ShaderResourceView>();
            for (int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                material.GetMaterialTexture(type, i, out TextureSlot slot);

                TextureInfo? loadedTexture = LoadedTextures.Find(texture => texture.FilePath == slot.FilePath);
                if (loadedTexture is not null)
                {
                    textures.Add(loadedTexture.Texture);
                }
                else
                {
                    ID3D11ShaderResourceView texture;
                    EmbeddedTexture? embeddedTexture = scene.GetEmbeddedTexture(slot.FilePath);
                    if (embeddedTexture is not null)
                    {
                        texture = LoadEmbeddedTexture(embeddedTexture);
                    }
                    else
                    {
                        string filePath = Path.Combine(baseDirectory, slot.FilePath);

                        int hr = NativeMethods.CreateWICTextureFromFile_(Device.NativePointer, Context.NativePointer, filePath, out _, out nint textureView);
                        if (hr != 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }

                        texture = new ID3D11ShaderResourceView(textureView);
                    }

                    TextureInfo textureInfo = new TextureInfo(typeName, slot.FilePath, texture);

                    textures.Add(textureInfo.Texture);
                    LoadedTextures.Add(textureInfo);
                }
            }

            return textures;
        }

        private ID3D11ShaderResourceView LoadEmbeddedTexture(EmbeddedTexture embeddedTexture)
        {
            if (embeddedTexture.Height != 0)
            {
                Texture2DDescription desc = new Texture2DDescription()
                {
                    Width = (uint)embeddedTexture.Width,
                    Height = (uint)embeddedTexture.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                };

                nint nativeData = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(byte)) * embeddedTexture.CompressedData.Length);
                Marshal.Copy(embeddedTexture.CompressedData, 0, nativeData, embeddedTexture.CompressedData.Length);

                SubresourceData subresourceData = new SubresourceData()
                {
                    DataPointer = nativeData,
                    RowPitch = (uint)(embeddedTexture.Width * 4),
                    SlicePitch = (uint)(embeddedTexture.Width * embeddedTexture.Height * 4),
                };

                ID3D11Texture2D texture2D = Device.CreateTexture2D(desc, subresourceData);
                ID3D11ShaderResourceView texture = Device.CreateShaderResourceView(texture2D);
                
                return texture;
            }

            int hr = NativeMethods.CreateWICTextureFromMemory_(Device.NativePointer, Context.NativePointer, embeddedTexture.CompressedData, embeddedTexture.Width, out _, out nint textureView);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return new ID3D11ShaderResourceView(textureView);
        }


        private class TextureInfo
        {
            public string Type { get; }
            public string FilePath { get; }
            public ID3D11ShaderResourceView Texture { get; }

            public TextureInfo(string type, string filePath, ID3D11ShaderResourceView texture)
            {
                Type = type;
                FilePath = filePath;
                Texture = texture;
            }
        }
    }
}
