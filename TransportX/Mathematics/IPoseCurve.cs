using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public interface IPoseCurve
    {
        float Length { get; }

        Pose StartPose => GetPose(0);
        Pose EndPose => GetPose(Length);

        Pose GetPose(float s);
    }
}
