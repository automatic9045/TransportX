using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common
{
    public struct SixDoF
    {
        public Vector3 Translation { get; }
        public Vector3 Rotation { get; }

        public SixDoF(Vector3 translation, Vector3 rotation)
        {
            Translation = translation;
            Rotation = rotation;
        }

        public SixDoF(float x, float y, float z, float rotationX, float rotationY, float rotationZ)
            : this(new Vector3(x, y, z), new Vector3(rotationX, rotationY, rotationZ))
        {
        }

        public SixDoF(Vector3 translation) : this(translation, Vector3.Zero)
        {
        }

        public SixDoF(float x, float y, float z) : this(new Vector3(x, y, z))
        {
        }

        public Matrix4x4 CreateTransform()
        {
            Matrix4x4 translation = Matrix4x4.CreateTranslation(Translation);
            Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

            return rotation * translation;
        }
    }
}
