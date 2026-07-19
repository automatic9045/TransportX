using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Scripting.Avatars;
using TransportX.Scripting.Commands;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Extensions
{
    public static class AxisFactoryExtensions
    {
        public static AxisFactory BindPlusMinusForSteering(this AxisFactory factory, Key plusKey, Key minusKey, double a, double b, double c, double d)
        {
            ScriptAvatar avatar = (ScriptAvatar)factory.Context;
            (float floatA, float floatB, float floatC, float floatD) = ((float)a / 3.6f, (float)b / 3.6f, (float)c, (float)d);

            factory.BindPlus(plusKey, (instance, observer) => CalcSteeringSpeed(avatar, floatA, floatB, floatC, floatD));
            factory.BindMinus(minusKey, (instance, observer) => CalcSteeringSpeed(avatar, floatA, floatB, floatC, floatD));

            return factory;
        }

        public static AxisFactory BindPlusMinusForSteering(this AxisFactory factory, string plusKeyCode, string minusKeyCode, double a, double b, double c, double d)
        {
            if (!factory.ParseKeyOrReport(plusKeyCode, out Key plusKey)) return factory;
            if (!factory.ParseKeyOrReport(minusKeyCode, out Key minusKey)) return factory;

            return factory.BindPlusMinusForSteering(plusKey, minusKey, a, b, c, d);
        }

        public static AxisFactory BindResetForSteering(this AxisFactory factory, Key key, double a, double b, double c, double d)
        {
            ScriptAvatar avatar = (ScriptAvatar)factory.Context;
            (float floatA, float floatB, float floatC, float floatD) = ((float)a / 3.6f, (float)b / 3.6f, (float)c, (float)d);

            factory.BindReset(key, (instance, observer) => CalcSteeringSpeed(avatar, floatA, floatB, floatC, floatD));

            return factory;
        }

        public static AxisFactory BindResetForSteering(this AxisFactory factory, string keyCode, double a, double b, double c, double d)
        {
            if (!factory.ParseKeyOrReport(keyCode, out Key key)) return factory;

            return factory.BindResetForSteering(key, a, b, c, d);
        }

        private static float CalcSteeringSpeed(ScriptAvatar avatar, float a, float b, float c, float d)
        {
            float vehicleSpeed = Vector3.Dot(avatar.Velocity, avatar.WorldPose.Pose.Direction);
            float rate = float.Clamp((vehicleSpeed - a) / (b - a), 0, 1);
            float speed = float.Lerp(c, d, rate);
            return speed;
        }
    }
}
