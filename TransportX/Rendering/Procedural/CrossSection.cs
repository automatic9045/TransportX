using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using ColliderMesh = BepuPhysics.Collidables.Mesh;
using BepuUtilities.Memory;
using Vortice.Direct3D11;

using TransportX.Physics;

namespace TransportX.Rendering.Procedural
{
    public class CrossSection
    {
        public static readonly CrossSection Empty = new([]);


        public IReadOnlyList<CrossSectionSurface> Surfaces { get; }

        public CrossSection(IEnumerable<CrossSectionSurface> surfaces)
        {
            Surfaces = [.. surfaces];
        }

        public ModelResourceSet Build(ID3D11Device device, Simulation simulation,
            IReadOnlyList<(Pose Pose, float S)> pathSamples, Vector2 offset, float vIntervalScale = 1, float vOffset = 0)
        {
            if (pathSamples.Count < 2)
            {
                throw new ArgumentException("メッシュを生成するには最低 2 点のサンプルが必要です。", nameof(pathSamples));
            }

            List<IMesh> builtMeshes = new(Surfaces.Count);
            List<Material> materials = new(Surfaces.Count);

            Dictionary<ColliderMaterial, List<Triangle>> collisionTriangles = [];

            for (int i = 0; i < Surfaces.Count; i++)
            {
                CrossSectionSurface surface = Surfaces[i];
                float vInterval = float.Max(1e-3f, surface.VInterval * vIntervalScale);

                Vertex[] vertices = new Vertex[pathSamples.Count * surface.Vertices.Count];
                int quadCount = (pathSamples.Count - 1) * (surface.Vertices.Count - 1);
                int[] indices = new int[quadCount * 6];

                int vertexWriteIndex = 0;
                for (int j = 0; j < pathSamples.Count; j++)
                {
                    Pose pose = pathSamples[j].Pose;
                    float s = pathSamples[j].S;
                    float v = (s + surface.VOffset + vOffset) / vInterval;

                    Vector3 forward = Pose.TransformNormal(Vector3.UnitZ, pose);
                    Vector3 up = Pose.TransformNormal(Vector3.UnitY, pose);
                    Vector3 right = Pose.TransformNormal(Vector3.UnitX, pose);

                    for (int k = 0; k < surface.Vertices.Count; k++)
                    {
                        CrossSectionVertex crossSectionVertex = surface.Vertices[k];
                        Vector2 position = crossSectionVertex.Position + offset;

                        Vector3 worldPosition = pose.Position + (right * position.X) + (up * position.Y);
                        Vector3 worldNormal = Vector3.Normalize((right * crossSectionVertex.Normal.X) + (up * crossSectionVertex.Normal.Y));
                        Vector3 worldTangent = Vector3.Normalize(Vector3.Cross(worldNormal, forward));
                        Vector2 uv = new(crossSectionVertex.U, v);

                        vertices[vertexWriteIndex++] = new Vertex(worldPosition, worldNormal, worldTangent, crossSectionVertex.Color, uv);
                    }
                }

                int indexWriteIndex = 0;
                for (int j = 0; j < pathSamples.Count - 1; j++)
                {
                    int sliceStart = j * surface.Vertices.Count;
                    int nextSliceStart = (j + 1) * surface.Vertices.Count;

                    for (int k = 0; k < surface.Vertices.Count - 1; k++)
                    {
                        int p00 = sliceStart + k;
                        int p01 = sliceStart + k + 1;
                        int p10 = nextSliceStart + k;
                        int p11 = nextSliceStart + k + 1;

                        indices[indexWriteIndex++] = p00;
                        indices[indexWriteIndex++] = p10;
                        indices[indexWriteIndex++] = p01;

                        indices[indexWriteIndex++] = p01;
                        indices[indexWriteIndex++] = p10;
                        indices[indexWriteIndex++] = p11;

                        if (surface.ColliderMaterial is ColliderMaterial colliderMaterial)
                        {
                            if (!collisionTriangles.TryGetValue(colliderMaterial, out List<Triangle>? triangles))
                            {
                                if (0 < collisionTriangles.Count)
                                {
                                    throw new NotImplementedException("複数材質の組み合わせは未対応です。");
                                }

                                triangles = [];
                                collisionTriangles.Add(colliderMaterial, triangles);
                            }

                            triangles.Add(new Triangle(vertices[p00].Position, vertices[p01].Position, vertices[p10].Position));
                            triangles.Add(new Triangle(vertices[p01].Position, vertices[p11].Position, vertices[p10].Position));
                        }
                    }
                }

                Mesh mesh = Mesh.Create(device, $"Surface_{i}", vertices, indices, surface.Material);
                builtMeshes.Add(mesh);
                materials.Add(surface.Material);
            }

            Model model = new(builtMeshes, materials);

            List<ICollider> colliders = [];
            foreach ((ColliderMaterial colliderMaterial, List<Triangle> triangles) in collisionTriangles)
            {
                simulation.BufferPool.Take(triangles.Count, out Buffer<Triangle> triangleBuffer);
                for (int i = 0; i < triangles.Count; i++)
                {
                    triangleBuffer[i] = triangles[i];
                }

                ColliderMesh collisionMesh = new(triangleBuffer, Vector3.One, simulation.BufferPool);
                Vector3 centerOfMass = collisionMesh.ComputeOpenCenterOfMass();
                collisionMesh.Recenter(centerOfMass);

                ICollider collider = ColliderFactory.Mesh(simulation, collisionMesh, colliderMaterial, new Pose(centerOfMass), true);
                colliders.Add(collider);
            }

            return new ModelResourceSet(model)
            {
                Collider = colliders.FirstOrDefault(),
            };
        }
    }
}
