using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Cameras
{
    public readonly record struct ViewpointPreset(Pose Offset, float FieldOfView)
    {
        public static ViewpointPreset Lerp(in ViewpointPreset from, in ViewpointPreset to, float t)
        {
            return new ViewpointPreset(Pose.Lerp(from.Offset, to.Offset, t), float.Lerp(from.FieldOfView, to.FieldOfView, t));
        }
    }
}
