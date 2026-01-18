using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common
{
    public readonly struct SixDoF
    {
        private const float RadPerDeg = float.Pi / 180;

        public static readonly SixDoF Zero = default;


        public readonly Vector3 Translation { get; }
        public readonly Vector3 Rotation { get; }

        public SixDoF(Vector3 translation, Vector3 rotation)
        {
            Translation = translation;
            Rotation = rotation;
        }

        public SixDoF(float x, float y, float z, float rotationX, float rotationY, float rotationZ)
            : this(new Vector3(x, y, z), new Vector3(rotationX, rotationY, rotationZ))
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixDoF FromDegrees(float x, float y, float z, float rotationX, float rotationY, float rotationZ)
        {
            return new SixDoF(x, y, z, rotationX * RadPerDeg, rotationY * RadPerDeg, rotationZ * RadPerDeg);
        }

        public SixDoF(Vector3 translation) : this(translation, Vector3.Zero)
        {
        }

        public SixDoF(float x, float y, float z) : this(new Vector3(x, y, z))
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pose ToPose()
        {
            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            return new Pose(Translation, rotation);
        }
    }
}
