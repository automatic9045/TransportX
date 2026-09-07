using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Scripting.Avatars;
using AvatarInput = TransportX.Scripting.Avatars.Commands.Input;
using TransportX.Scripting.Commands;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Extensions
{
    public static class AxisFactoryExtensions
    {
        public static AxisFactory<AvatarInput> BindPlusMinusForSteering(this AxisFactory<AvatarInput> factory,
            string plusKey, Key defaultPlus, string minusKey, Key defaultMinus, double a, double b, double c, double d)
        {
            (float floatA, float floatB, float floatC, float floatD) = ((float)a / 3.6f, (float)b / 3.6f, (float)c, (float)d);

            factory.BindPlus(plusKey, defaultPlus, (instance, observer) => CalcSteeringSpeed(factory.Parent.Avatar, floatA, floatB, floatC, floatD));
            factory.BindMinus(minusKey, defaultMinus, (instance, observer) => CalcSteeringSpeed(factory.Parent.Avatar, floatA, floatB, floatC, floatD));

            return factory;
        }

        public static AxisFactory<AvatarInput> BindPlusMinusForSteering(this AxisFactory<AvatarInput> factory,
            string plusKey, string defaultPlusCode, string minusKey, string defaultMinusCode, double a, double b, double c, double d)
        {
            if (!factory.ParseKeyOrReport(defaultPlusCode, out Key defaultPlus)) return factory;
            if (!factory.ParseKeyOrReport(defaultMinusCode, out Key defaultMinus)) return factory;

            return factory.BindPlusMinusForSteering(plusKey, defaultPlus, minusKey, defaultMinus, a, b, c, d);
        }

        public static AxisFactory<AvatarInput> BindPlusMinusForSteering(this AxisFactory<AvatarInput> factory,
            Key defaultPlus, Key defaultMinus, double a, double b, double c, double d)
            => factory.BindPlusMinusForSteering(string.Empty, defaultPlus, string.Empty, defaultMinus, a, b, c, d);

        public static AxisFactory<AvatarInput> BindPlusMinusForSteering(this AxisFactory<AvatarInput> factory,
            string defaultPlusCode, string defaultMinusCode, double a, double b, double c, double d)
            => factory.BindPlusMinusForSteering(string.Empty, defaultPlusCode, string.Empty, defaultMinusCode, a, b, c, d);

        public static AxisFactory<AvatarInput> BindResetForSteering(this AxisFactory<AvatarInput> factory, string key, Key defaultBinding, double a, double b, double c, double d)
        {
            (float floatA, float floatB, float floatC, float floatD) = ((float)a / 3.6f, (float)b / 3.6f, (float)c, (float)d);

            factory.BindReset(key, defaultBinding, (instance, observer) => CalcSteeringSpeed(factory.Parent.Avatar, floatA, floatB, floatC, floatD));

            return factory;
        }

        public static AxisFactory<AvatarInput> BindResetForSteering(this AxisFactory<AvatarInput> factory, string key, string defaultBindingCode, double a, double b, double c, double d)
        {
            if (!factory.ParseKeyOrReport(defaultBindingCode, out Key defaultBinding)) return factory;

            return factory.BindResetForSteering(key, defaultBinding, a, b, c, d);
        }

        public static AxisFactory<AvatarInput> BindResetForSteering(this AxisFactory<AvatarInput> factory, Key defaultBinding, double a, double b, double c, double d)
            => factory.BindResetForSteering(string.Empty, defaultBinding, a, b, c, d);

        public static AxisFactory<AvatarInput> BindResetForSteering(this AxisFactory<AvatarInput> factory, string defaultBindingCode, double a, double b, double c, double d)
            => factory.BindResetForSteering(string.Empty, defaultBindingCode, a, b, c, d);

        private static float CalcSteeringSpeed(ScriptAvatar avatar, float a, float b, float c, float d)
        {
            float vehicleSpeed = Vector3.Dot(avatar.Velocity, avatar.WorldPose.Pose.Direction);
            float rate = float.Clamp((vehicleSpeed - a) / (b - a), 0, 1);
            float speed = float.Lerp(c, d, rate);
            return speed;
        }
    }
}
