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

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class MergedKinematicLocatedModel : KinematicLocatedModel
    {
        private readonly List<LocatedModel> Children;

        protected MergedKinematicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, List<LocatedModel> children)
            : base(simulation, model, handle, Pose.Identity)
        {
            Children = children;
        }

        public static MergedKinematicLocatedModel Create(IPhysicsHost physicsHost, IEnumerable<KinematicLocatedModelTemplate> sources)
        {
            List<KinematicLocatedModelTemplate> sourceList = sources.ToList();
            if (sourceList.Count == 0) throw new ArgumentException("結合するモデルがありません。", nameof(sources));

            int totalTriangles = sourceList.Sum(m => ((Collider<ColliderMesh>)m.Model.Collider).Shape.Triangles.Length);
            physicsHost.Simulation.BufferPool.Take(totalTriangles, out Buffer<Triangle> combinedTriangles);

            List<LocatedModel> children = [];
            int writeIndex = 0;
            foreach (KinematicLocatedModelTemplate model in sourceList)
            {
                Pose pose = model.ColliderToBase;

                if (model.Model.Collider is Collider<ColliderMesh> meshCollider)
                {
                    for (int i = 0; i < meshCollider.Shape.Triangles.Length; i++)
                    {
                        Triangle triangle = meshCollider.Shape.Triangles[i];
                        combinedTriangles[writeIndex] = new Triangle(
                            Pose.Transform(triangle.A, pose),
                            Pose.Transform(triangle.B, pose),
                            Pose.Transform(triangle.C, pose)
                        );

                        writeIndex++;
                    }
                }

                LocatedModel visualChild = new(model.Model, model.Pose);
                children.Add(visualChild);
            }

            ColliderMesh newMesh = new(combinedTriangles, Vector3.One, physicsHost.Simulation.BufferPool);
            Vector3 center = newMesh.ComputeOpenCenterOfMass();
            newMesh.Recenter(center);

            ColliderMaterial material = sourceList[0].Model.Collider.Material;
            Collider<ColliderMesh> newCollider = ColliderFactory.Mesh(physicsHost.Simulation, newMesh, material, new Pose(center), true);
            CollidableModel physicsWrapper = new(newCollider)
            {
                DebugName = $"Merged{{{sourceList[0].Model.DebugName}, others: {sourceList.Count - 1}}}",
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

        public override void Draw(LocatedDrawContext context)
        {
            foreach (LocatedModel child in Children)
            {
                child.Pose = child.BasePose * Pose;
                child.Draw(context);
            }

            base.Draw(context);
        }
    }
}
