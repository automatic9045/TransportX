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
using Vortice.Mathematics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class MergedStaticTransformedModel : StaticTransformedModel, IDisposable
    {
        protected readonly IReadOnlyList<TransformedModel> Children;

        protected MergedStaticTransformedModel(IPhysicsHost physicsHost, in ModelResourceSet wrapperModel, StaticDescription description, List<TransformedModel> children)
            : base(physicsHost, wrapperModel, description, Pose.Identity)
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

            int triangleCount = sources.Sum(m => m.Collider is ColliderBase<ColliderMesh> meshCollider ? meshCollider.Shape.Triangles.Length : 0);
            physicsHost.Simulation.BufferPool.Take(triangleCount, out Buffer<Triangle> combinedTriangles);

            List<TransformedModel> children = [];
            int writeIndex = 0;
            for (int i = 0; i < sources.Count; i++)
            {
                StaticTransformedModelTemplate source = sources[i];

                if (source.Collider is ColliderBase<ColliderMesh> meshCollider)
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

            BoundingBox boundingBox = BoundingBox.Zero;
            for (int i = 0; i < sources.Count; i++)
            {
                Matrix4x4 sourceMatrix = sources[i].ColliderToBase.ToMatrix4x4();
                BoundingBox sourceBox = BoundingBox.Transform(sources[i].Resource.Model.BoundingBox, sourceMatrix);

                boundingBox = i == 0 ? sourceBox : BoundingBox.CreateMerged(boundingBox, sourceBox);
            }

            ColliderMaterial material = sources[0].Collider.Material;
            ColliderBase<ColliderMesh> newCollider = ColliderFactory.Mesh(physicsHost.Simulation, newMesh, material, new Pose(center), true);

            ModelResourceSet wrapperModel = new()
            {
                Model = new Model([], [], boundingBox)
                {
                    DebugName = $"Merged{{{sources[0].Resource.Model.DebugName}, others: {sources.Count - 1}}}",
                },
                Collider = newCollider,
            };

            StaticDescription desc = new(newCollider.Offset.ToRigidPose(), newCollider.ShapeIndex);
            return new MergedStaticTransformedModel(physicsHost, wrapperModel, desc, children);
        }

        public override void Dispose()
        {
            base.Dispose();
            Resource.Dispose();
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
