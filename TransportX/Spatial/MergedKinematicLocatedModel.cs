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
using Vortice.Mathematics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class MergedKinematicLocatedModel : KinematicLocatedModel
    {
        protected readonly IReadOnlyList<LocatedModel> Children;

        protected MergedKinematicLocatedModel(Simulation simulation, ICollidableModel physicsWrapper, BodyHandle handle, List<LocatedModel> children)
            : base(simulation, physicsWrapper, handle, Pose.Identity)
        {
            Children = children;
        }

        public static MergedKinematicLocatedModel Create(IPhysicsHost physicsHost, IReadOnlyList<KinematicLocatedModelTemplate> sources)
        {
            if (sources.Count == 0) throw new ArgumentException("結合するモデルがありません。", nameof(sources));

            int triangleCount = sources.Sum(m => m.Model.Collider is ColliderBase<ColliderMesh> meshCollider ? meshCollider.Shape.Triangles.Length : 0);
            physicsHost.Simulation.BufferPool.Take(triangleCount, out Buffer<Triangle> combinedTriangles);

            List<LocatedModel> children = [];
            int writeIndex = 0;
            for (int i = 0; i < sources.Count; i++)
            {
                KinematicLocatedModelTemplate source = sources[i];

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

                LocatedModel visualChild = new(source.Model, source.Pose);
                children.Add(visualChild);
            }

            for (int i = triangleCount; i < combinedTriangles.Length; i++)
            {
                combinedTriangles[i] = new Triangle();
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

            BodyDescription desc = BodyDescription.CreateKinematic(newCollider.Offset.ToRigidPose(), newCollider.ShapeIndex, 0.01f);
            BodyHandle handle = physicsHost.Simulation.Bodies.Add(desc);
            physicsHost.SetMaterial(handle, material);

            return new MergedKinematicLocatedModel(physicsHost.Simulation, physicsWrapper, handle, children);
        }

        public override void Dispose()
        {
            base.Dispose();
            Model.Dispose();
        }

        public override void Draw(in LocatedDrawContext context)
        {
            if (IsVisible && context.Pass == RenderPass.Normal)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    LocatedModel child = Children[i];
                    child.Pose = child.BasePose * Pose;

                    Matrix4x4 world = (child.Pose * context.PlateOffset.Pose).ToMatrix4x4();
                    BoundingBox worldBox = BoundingBox.Transform(child.Model.BoundingBox, world);
                    if (context.Frustum.Contains(worldBox) == ContainmentType.Disjoint) continue;

                    InstanceData instanceData = new()
                    {
                        World = Matrix4x4.Transpose(world),
                    };
                    context.RenderQueue.Submit(context.Pass, child.Model, instanceData);
                }
            }

            base.Draw(context);
        }
    }
}
