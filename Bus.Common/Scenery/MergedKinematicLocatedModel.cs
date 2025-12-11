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

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class MergedKinematicLocatedModel : KinematicLocatedModel
    {
        private readonly List<LocatedModel> Children;

        protected MergedKinematicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, List<LocatedModel> children)
            : base(simulation, model, handle, Matrix4x4.Identity)
        {
            Children = children;
        }

        public static MergedKinematicLocatedModel Create(IPhysicsHost physicsHost, IEnumerable<KinematicLocatedModel> sources)
        {
            List<KinematicLocatedModel> sourceList = sources.ToList();
            if (sourceList.Count == 0) throw new ArgumentException("結合するモデルがありません。", nameof(sources));

            int totalTriangles = sourceList.Sum(m => ((Collider<ColliderMesh>)m.Model.Collider).Shape.Triangles.Length);
            physicsHost.Simulation.BufferPool.Take(totalTriangles, out Buffer<Triangle> combinedTriangles);

            List<LocatedModel> children = [];
            int writeIndex = 0;
            foreach (KinematicLocatedModel model in sourceList)
            {
                Matrix4x4 transform = model.ColliderToBase;

                if (model.Model.Collider is Collider<ColliderMesh> meshCollider)
                {
                    for (int i = 0; i < meshCollider.Shape.Triangles.Length; i++)
                    {
                        Triangle triangle = meshCollider.Shape.Triangles[i];
                        combinedTriangles[writeIndex] = new Triangle(
                            Vector3.Transform(triangle.A, transform),
                            Vector3.Transform(triangle.B, transform),
                            Vector3.Transform(triangle.C, transform)
                        );

                        writeIndex++;
                    }
                }

                LocatedModel visualChild = new(model.Model, model.BaseTransform);
                children.Add(visualChild);
            }

            ColliderMesh newMesh = new(combinedTriangles, Vector3.One, physicsHost.Simulation.BufferPool);
            Vector3 center = newMesh.ComputeOpenCenterOfMass();
            newMesh.Recenter(center);

            ColliderMaterial material = sourceList[0].Model.Collider.Material;
            Collider<ColliderMesh> newCollider = ColliderFactory.Mesh(physicsHost.Simulation, newMesh, material, Matrix4x4.CreateTranslation(center), true);
            CollidableModel physicsWrapper = new(newCollider);

            BodyDescription desc = BodyDescription.CreateKinematic(newCollider.Offset.ToRigidPose(), newCollider.ShapeIndex, 0.01f);
            BodyHandle handle = physicsHost.Simulation.Bodies.Add(desc);
            physicsHost.SetMaterial(handle, material);

            return new MergedKinematicLocatedModel(physicsHost.Simulation, physicsWrapper, handle, children);
        }

        public override void Draw(LocatedDrawContext context)
        {
            foreach (LocatedModel child in Children)
            {
                child.Transform = child.BaseTransform * Transform;
                child.Draw(context);
            }

            base.Draw(context);
        }

        public void CreateColliderDebugModel(ID3D11Device device, Vector4 color)
        {
            Model.Collider.CreateDebugModel(device, color);
        }
    }
}
