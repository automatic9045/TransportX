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

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class MergedStaticTransformedModel : StaticTransformedModel, IDisposable
    {
        protected readonly IReadOnlyList<TransformedModel> Children;

        protected MergedStaticTransformedModel(IPhysicsHost physicsHost, ICollidableModel physicsWrapper, StaticDescription description, List<TransformedModel> children)
            : base(physicsHost, physicsWrapper, description, Pose.Identity)
        {
            Children = children;
        }

        public static bool CanMerge(ICollider collider)
        {
            return collider is ColliderBase<ColliderMesh>;
        }

        public static MergedStaticTransformedModel Create(IPhysicsHost physicsHost, IReadOnlyList<StaticTransformedModelTemplate> sources)
        {
            if (sources.Count == 0) throw new ArgumentException("結合するモデルがありません。", nameof(sources));

            int triangleCount = sources.Sum(m => m.Model.Collider is ColliderBase<ColliderMesh> meshCollider ? meshCollider.Shape.Triangles.Length : 0);
            physicsHost.Simulation.BufferPool.Take(triangleCount, out Buffer<Triangle> combinedTriangles);

            List<TransformedModel> children = [];
            int writeIndex = 0;
            for (int i = 0; i < sources.Count; i++)
            {
                StaticTransformedModelTemplate source = sources[i];

                if (source.Model.Collider is ColliderBase<ColliderMesh> meshCollider)
                {
                    for (int j = 0; j < meshCollider.Shape.Triangles.Length; j++)
                    {
                        Triangle triangle = meshCollider.Shape.Triangles[j];
                        combinedTriangles[writeIndex] = new Triangle(
                            Pose.Transform(triangle.A, source.ColliderToBase),
                            Pose.Transform(triangle.B, source.ColliderToBase),
                            Pose.Transform(triangle.C, source.ColliderToBase)
                        );

                        writeIndex++;
                    }
                }
                else
                {
                    physicsHost.Simulation.BufferPool.Return(ref combinedTriangles);
                    throw new NotSupportedException("メッシュ以外のコライダーを結合することはできません。");
                }

                TransformedModel visualChild = source.BuildVisual(pose => pose);
                children.Add(visualChild);
            }

            ColliderMesh newMesh = new(combinedTriangles, Vector3.One, physicsHost.Simulation.BufferPool);
            Vector3 center = newMesh.ComputeOpenCenterOfMass();
            newMesh.Recenter(center);

            ColliderMaterial material = sources[0].Model.Collider.Material;
            ColliderBase<ColliderMesh> newCollider = ColliderFactory.Mesh(physicsHost.Simulation, newMesh, material, new Pose(center), true);
            CollidableModel physicsWrapper = new(newCollider)
            {
                DebugName = $"Merged{{{sources[0].Model.DebugName}, others: {sources.Count - 1}}}",
            };

            StaticDescription desc = new(newCollider.Offset.ToRigidPose(), newCollider.ShapeIndex);
            return new MergedStaticTransformedModel(physicsHost, physicsWrapper, desc, children);
        }

        public override void Dispose()
        {
            base.Dispose();
            Model.Dispose();
        }

        public override void Draw(in TransformedDrawContext context)
        {
            if (IsVisible && context.Layer == RenderLayer.Normal)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Draw(context);
                }
            }

            base.Draw(context);
        }
    }
}
