using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Diagnostics;
using TransportX.Physics;

namespace TransportX.Rendering
{
    public class Model : IMeshModel
    {
        public static Model Empty() => new([])
        {
            DebugName = "Empty",
        };


        private bool IsDisposed = false;

        public BoundingBox BoundingBox { get; }
        public IReadOnlyList<IMesh> VisualMeshes { get; }

        public virtual string? DebugName
        {
            get => field;
            set
            {
                field = value;

                if (value is null)
                {
                    for (int i = 0; i < VisualMeshes.Count; i++) VisualMeshes[i].DebugName = null;
                }
                else
                {

                    for (int i = 0; i < VisualMeshes.Count; i++) VisualMeshes[i].DebugName = $"{value}_Material";
                }
            }
        } = null;

        public Model(IReadOnlyList<IMesh> visualMeshes, BoundingBox boundingBox)
        {
            VisualMeshes = visualMeshes;
            BoundingBox = boundingBox;
        }

        public Model(IReadOnlyList<IMesh> visualMeshes) : this(visualMeshes, ComputeBoundingBox(visualMeshes))
        {
        }

        private static BoundingBox ComputeBoundingBox(IReadOnlyList<IMesh> visualMeshes)
        {
            BoundingBox boundingBox = visualMeshes.Count == 0 ? default : visualMeshes[0].BoundingBox;
            for (int i = 1; i < visualMeshes.Count; i++)
            {
                boundingBox = BoundingBox.CreateMerged(boundingBox, visualMeshes[i].BoundingBox);
            }
            return boundingBox;
        }

        public static Model Load(ID3D11DeviceContext context, IErrorCollector errorCollector, string visualModelPath, bool makeLH)
        {
            using ModelFactory factory = new(context, null, errorCollector);
            Model model = factory.Load(visualModelPath, makeLH);
            return model;
        }

        public virtual void Dispose()
        {
            if (IsDisposed) throw new InvalidOperationException();
            IsDisposed = true;

            for (int i = 0; i < VisualMeshes.Count; i++)
            {
                VisualMeshes[i].Dispose();
            }
        }

        public void Draw(in DrawContext context)
        {
            for (int i = 0; i < VisualMeshes.Count; i++)
            {
                VisualMeshes[i].Draw(context);
            }
        }
    }


    public class CollidableModel : Model, ICollidableModel
    {
        public ICollider Collider { get; }
        public IDebugModel? ColliderDebugModel { get; private set; } = null;

        public override string? DebugName
        {
            get => base.DebugName;
            set => ColliderDebugModel?.DebugName = base.DebugName = value;
        }

        public CollidableModel(IReadOnlyList<IMesh> visualMeshes, BoundingBox boundingBox, ICollider collider) : base(visualMeshes, boundingBox)
        {
            Collider = collider;
        }

        public CollidableModel(IReadOnlyList<IMesh> visualMeshes, ICollider collider) : base(visualMeshes)
        {
            Collider = collider;
        }

        public CollidableModel(Model baseModel, ICollider collider) : this(baseModel.VisualMeshes, collider)
        {
            DebugName = baseModel.DebugName;
        }

        public CollidableModel(BoundingBox boundingBox, ICollider collider) : this([], boundingBox, collider)
        {
        }

        public CollidableModel(ICollider collider) : this([], collider)
        {
        }

        public static CollidableModel Load(ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector,
            string visualModelPath, bool makeVisualLH, string collisionModelPath, bool makeCollisionLH, ColliderMaterial material, bool isOpen)
        {
            using ModelFactory factory = new(context, simulation, errorCollector);
            CollidableModel model = factory.LoadWithCollisionModel(visualModelPath, makeVisualLH, collisionModelPath, makeCollisionLH, material, isOpen);
            return model;
        }

        public static CollidableModel LoadWithBoundingBox(ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector,
            string visualModelPath, bool makeLH, ColliderMaterial material)
        {
            using ModelFactory factory = new(context, simulation, errorCollector);
            CollidableModel model = factory.LoadWithBoundingBox(visualModelPath, makeLH, material);
            return model;
        }

        public static CollidableModel LoadWithConvexHull(ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector,
            string visualModelPath, bool makeLH, ColliderMaterial material)
        {
            using ModelFactory factory = new(context, simulation, errorCollector);
            CollidableModel model = factory.LoadWithConvexHull(visualModelPath, makeLH, material);
            return model;
        }

        public override void Dispose()
        {
            base.Dispose();
            ColliderDebugModel?.Dispose();
        }

        public void CreateColliderDebugModel(ID3D11Device device)
        {
            ColliderDebugModel ??= Collider.CreateDebugModel(device);
        }
    }
}
