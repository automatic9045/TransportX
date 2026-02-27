using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class LocatedModel : IDrawable
    {
        public IModel Model { get; }

        public Pose BasePose
        {
            get => field;
            set
            {
                field = value;
                Pose basePoseInv = Pose.Inverse(value);
                BasePoseInverse = basePoseInv;
            }
        }
        public Pose BasePoseInverse { get; private set; }

        public virtual Pose Pose { get; set; } = Pose.Identity;
        public bool IsVisible { get; set; } = true;

        protected LocatedModel(IModel model, Pose basePose, bool setPose)
        {
            Model = model;
            BasePose = basePose;
            if (setPose) Pose = basePose;
        }

        public LocatedModel(IModel model, Pose basePose) : this(model, basePose, true)
        {
        }

        public virtual void Draw(in LocatedDrawContext context)
        {
            if (!IsVisible) return;
            if (context.Pass != RenderPass.Normal) return;

            Matrix4x4 world = (Pose * context.PlateOffset.Pose).ToMatrix4x4();
            BoundingBox worldBox = BoundingBox.Transform(Model.BoundingBox, world);
            if (context.Frustum.Contains(worldBox) == ContainmentType.Disjoint) return;

            InstanceData instanceData = new()
            {
                World = Matrix4x4.Transpose(world),
            };
            context.RenderQueue.Submit(context.Pass, Model, instanceData);
        }
    }
}
