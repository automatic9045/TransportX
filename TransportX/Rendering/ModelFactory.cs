using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using CollisionMesh = BepuPhysics.Collidables.Mesh;
using BepuUtilities.Memory;
using Vortice.Direct3D11;

using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering.Importing;

namespace TransportX.Rendering
{
    internal class ModelFactory : IDisposable
    {
        private readonly ID3D11DeviceContext Context;
        private readonly Simulation? Simulation;
        private readonly Importing.IModelImporter Importer;
        private readonly IErrorCollector ErrorCollector;

        public bool IsCollisionSupported => Simulation is not null;

        public ModelFactory(ID3D11DeviceContext context, Simulation? simulation, Importing.IModelImporter importer, IErrorCollector errorCollector)
        {
            Context = context;
            Simulation = simulation;
            Importer = importer;
            ErrorCollector = errorCollector;
        }

        public void Dispose()
        {
            Importer.Dispose();
        }

        public Model Load(string visualModelPath, bool makeLH)
        {
            string baseDirectory = Path.GetDirectoryName(visualModelPath)!;
            Importing.Model modelData = Importer.Import(visualModelPath, true, makeLH);

            ModelBuilder builder = new(Context, ErrorCollector);
            return builder.Create(modelData, baseDirectory, visualModelPath);
        }

        private void CheckCollisionSupported()
        {
            if (!IsCollisionSupported) throw new NotSupportedException($"{nameof(Simulation)} が指定されていないため、衝突判定を読み込むことはできません。");
        }

        public CollidableModel LoadWithBoundingBox(string visualModelPath, bool makeLH, ColliderMaterial material)
        {
            CheckCollisionSupported();

            string baseDirectory = Path.GetDirectoryName(visualModelPath)!;
            Importing.Model modelData = Importer.Import(visualModelPath, true, makeLH);

            ModelBuilder builder = new(Context, ErrorCollector);
            Model baseModel = builder.Create(modelData, baseDirectory, visualModelPath);

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (Importing.Mesh mesh in modelData.Meshes)
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
            Vector3 center = (min + max) / 2;
            Pose colliderOffset = new(center);
            ColliderBase<Box> collider = ColliderFactory.Box(Simulation!, box, material, colliderOffset);

            return new CollidableModel(baseModel, collider);
        }

        public CollidableModel LoadWithConvexHull(string visualModelPath, bool makeLH, ColliderMaterial material)
        {
            CheckCollisionSupported();

            string baseDirectory = Path.GetDirectoryName(visualModelPath)!;
            Importing.Model modelData = Importer.Import(visualModelPath, true, makeLH);

            ModelBuilder builder = new(Context, ErrorCollector);
            Model baseModel = builder.Create(modelData, baseDirectory, visualModelPath);

            Simulation!.BufferPool.Take(modelData.Meshes.Sum(mesh => mesh.Vertices.Length), out Buffer<Vector3> pointBuffer);
            try
            {
                int i = 0;
                foreach (Importing.Mesh mesh in modelData.Meshes)
                {
                    foreach (Vector3 vertex in mesh.Vertices)
                    {
                        pointBuffer[i] = vertex;
                        i++;
                    }
                }

                ConvexHullHelper.CreateShape(pointBuffer, Simulation.BufferPool, out Vector3 center, out ConvexHull convexHull);
                Pose colliderOffset = new(center);
                ColliderBase<ConvexHull> collider = ColliderFactory.ConvexHull(Simulation, convexHull, material, colliderOffset);

                return new CollidableModel(baseModel, collider);
            }
            finally
            {
                Simulation.BufferPool.Return(ref pointBuffer);
            }
        }

        public CollidableModel LoadWithCollisionModel(
            string visualModelPath, bool makeVisualLH, string collisionModelPath, bool makeCollisionLH, ColliderMaterial material, bool isOpen)
        {
            CheckCollisionSupported();

            Model baseModel = Load(visualModelPath, makeVisualLH);

            Importing.Model collisionModelData = Importer.Import(collisionModelPath, false, makeCollisionLH);
            Simulation!.BufferPool.Take(collisionModelData.Meshes.Sum(mesh => mesh.Indices.Length / 3), out Buffer<Triangle> triangles);

            int i = 0;
            foreach (Importing.Mesh mesh in collisionModelData.Meshes)
            {
                for (int j = 0; j < mesh.Indices.Length; j += 3)
                {
                    triangles[i] = new Triangle(mesh.Vertices[mesh.Indices[j + 2]], mesh.Vertices[mesh.Indices[j + 1]], mesh.Vertices[mesh.Indices[j]]);
                    i++;
                }
            }

            CollisionMesh collisionMesh = new CollisionMesh(triangles, Vector3.One, Simulation.BufferPool);
            Vector3 center = isOpen ? collisionMesh.ComputeOpenCenterOfMass() : collisionMesh.ComputeClosedCenterOfMass();
            if (float.IsNaN(center.X + center.Y + center.Z))
            {
                ModelLoadError error = new ModelLoadError(
                    ModelLoadErrorTypes.Collision | ModelLoadErrorTypes.Skipped, ErrorLevel.Error, "モデルの重心を特定できません。", collisionModelPath);
                ErrorCollector.Report(error);
                center = Vector3.Zero;
            }

            collisionMesh.Recenter(center);
            Pose colliderOffset = new(center);
            ColliderBase<CollisionMesh> collider = ColliderFactory.Mesh(Simulation!, collisionMesh, material, colliderOffset, isOpen);

            return new CollidableModel(baseModel, collider);
        }
    }
}
