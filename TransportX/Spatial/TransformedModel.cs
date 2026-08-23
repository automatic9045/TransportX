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
    public class TransformedModel : IDrawable
    {
        public static TransformedModel Empty() => new(ModelResourceSet.Empty(), Pose.Identity);


        public ModelResourceSet Resource { get; set; }

        public Pose BasePose
        {
            get;
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

        protected TransformedModel(in ModelResourceSet resource, Pose basePose, bool setPose)
        {
            Resource = resource;
            BasePose = basePose;
            if (setPose) Pose = basePose;
        }

        public TransformedModel(in ModelResourceSet resource, Pose basePose) : this(resource, basePose, true)
        {
        }

        public virtual void Draw(in TransformedDrawContext context)
        {
            if (!IsVisible) return;
            if (context.Layer != RenderLayer.Normal) return;

            Matrix4x4 world = (Pose * context.ChunkOffset.Pose).ToMatrix4x4();
            context.DrawModel(Resource.Model, world);
        }
    }
}
