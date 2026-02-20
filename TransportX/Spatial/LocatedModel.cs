using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

        protected LocatedModel(IModel model, Pose basePose, bool setPose)
        {
            Model = model;
            BasePose = basePose;
            if (setPose) Pose = basePose;
        }

        public LocatedModel(IModel model, Pose basePose) : this(model, basePose, true)
        {
        }

        public virtual void Draw(LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Normal) return;

            TransformConstants transformConstants = new()
            {
                World = Matrix4x4.Transpose((Pose * context.PlateOffset.Pose).ToMatrix4x4()),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
            };
            context.DeviceContext.UpdateSubresource(transformConstants, context.TransformBuffer);

            Model.Draw(new(context.DeviceContext, context.TransformBuffer, context.MaterialBuffer));
        }
    }
}
