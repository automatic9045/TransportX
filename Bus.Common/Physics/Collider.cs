using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Physics
{
    public class Collider<TShape> : ICollider where TShape : unmanaged, IShape
    {
        protected readonly Func<TShape, float, BodyInertia> InertiaFactory;

        public TShape Shape { get; }
        IShape ICollider.Shape => Shape;
        public TypedIndex ShapeIndex { get; }
        public ColliderMaterial Material { get; }
        public Pose Offset { get; }
        public Pose OffsetInverse { get; }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                DebugModel?.DebugName = value is null ? null : $"{value}_Collider";
            }
        } = null;

        public bool CanDrawDebug => DebugModel is not null;
        public virtual Vector4 DebugColor { get; set; } = Vector4.One;
        public IModel? DebugModel { get; protected set; } = null;

        public Collider(TShape shape, TypedIndex shapeIndex, ColliderMaterial material, Pose offset, Func<TShape, float, BodyInertia> inertiaFactory)
        {
            Shape = shape;
            ShapeIndex = shapeIndex;
            Material = material;
            Offset = offset;
            OffsetInverse = Pose.Inverse(Offset);
            InertiaFactory = inertiaFactory;
        }

        public virtual void Dispose()
        {
            DebugModel?.Dispose();
        }

        public BodyInertia ComputeInertia(float mass)
        {
            return InertiaFactory(Shape, mass);
        }

        public virtual void CreateDebugResources(ID3D11Device device)
        {
        }

        public void DrawDebug(DrawContext context)
        {
            if (DebugModel is null) throw new InvalidOperationException("デバッグモデルが作成されていません。");

            DebugModel.Draw(context);
        }
    }
}
