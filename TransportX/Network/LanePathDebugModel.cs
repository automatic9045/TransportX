using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Network
{
    public class LanePathDebugModel : WireframeDebugModel
    {
        protected readonly Mesh SpineMesh;
        protected readonly Mesh WingMesh;

        public override Vector4 Color
        {
            get => field;
            set
            {
                field = value;
                Vector4 linearColor = field.ToLinear();
                SpineMesh.Material.BaseColor = linearColor;
                WingMesh.Material.BaseColor = new Vector4(linearColor.AsVector3(), value.W * 0.3f);
            }
        } = Vector4.One;

        public LanePathDebugModel(Mesh spineMesh, Mesh wingMesh) : base([spineMesh, wingMesh])
        {
            SpineMesh = spineMesh;
            WingMesh = wingMesh;
        }
    }
}
