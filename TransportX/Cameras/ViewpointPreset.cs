using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Cameras
{
    public readonly record struct ViewpointPreset(SixDoF Offset, float FieldOfView = float.Pi / 4)
    {
        public static ViewpointPreset Lerp(in ViewpointPreset from, in ViewpointPreset to, float t)
        {
            SixDoF offset = SixDoF.Lerp(from.Offset, to.Offset, t);
            float fieldOfView = float.Lerp(from.FieldOfView, to.FieldOfView, t);
            return new ViewpointPreset(offset, fieldOfView);
        }
    }
}
