using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public interface IPoseCurve
    {
        public static readonly IPoseCurve Empty = new EmptyCurve();


        float Length { get; }

        Pose StartPose => GetPose(0);
        Pose EndPose => GetPose(Length);

        Pose GetPose(float s);


        private class EmptyCurve : IPoseCurve
        {
            public float Length => 0;
            public Pose GetPose(float s) => Pose.Identity;
        }
    }
}
