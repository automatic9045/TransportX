using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common
{
    public readonly struct Pose
    {
        public static readonly Pose Identity = new Pose(Vector3.Zero);


        public readonly Vector3 Position { get; }
        public readonly Quaternion Orientation { get; }

        public Pose(Vector3 position, Quaternion orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public Pose(Vector3 position) : this(position, Quaternion.Identity)
        {
        }

        public Pose(float x, float y, float z, Quaternion orientation) : this(new Vector3(x, y, z), orientation)
        {
        }

        public Pose(float x, float y, float z) : this(x, y, z, Quaternion.Identity)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose operator *(in Pose a, in Pose b)
        {
            Vector3 position = b.Position + Vector3.Transform(a.Position, b.Orientation);
            Quaternion orientation = b.Orientation * a.Orientation;
            return new Pose(position, orientation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose FromMatrix4x4(Matrix4x4 matrix)
        {
            if (!Matrix4x4.Decompose(matrix, out _, out Quaternion orientation, out Vector3 position))
            {
                throw new ArgumentException("指定された行列は剛体変換ではありません。", nameof(matrix));
            }

            return new Pose(position, orientation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose CreateRotationX(float radians)
        {
            return new Pose(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitX, radians));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose CreateRotationY(float radians)
        {
            return new Pose(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitY, radians));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose CreateRotationZ(float radians)
        {
            return new Pose(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, radians));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose CreateWorldLH(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix4x4 world = Matrix4x4.CreateWorld(Vector3.Zero, -forward, up);
            Quaternion orientation = Quaternion.CreateFromRotationMatrix(world);
            return new Pose(position, orientation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(Vector3 value, in Pose pose)
        {
            return pose.Position + Vector3.Transform(value, pose.Orientation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransformNormal(Vector3 normal, in Pose pose)
        {
            return Vector3.Transform(normal, pose.Orientation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 result = Matrix4x4.CreateFromQuaternion(Orientation);
            result.Translation = Position;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pose Inverse()
        {
            Quaternion orientationInv = Quaternion.Inverse(Orientation);
            Vector3 positionInv = Vector3.Transform(-Position, orientationInv);
            return new Pose(positionInv, orientationInv);
        }
    }
}
