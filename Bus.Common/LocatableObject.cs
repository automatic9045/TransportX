using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;

namespace Bus.Common
{
    public class LocatableObject
    {
        public static readonly LocatableObject Origin = new LocatableObject();


        public int PlateX { get; private set; }
        public int PlateZ { get; private set; }
        public Matrix4x4 Transform { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 PositionInWorld => Position + Origin.GetPlateOffset(this).Position; // 注意: 原点から離れたプレート上では、誤差が大きい可能性あり
        public Quaternion Orientation { get; private set; }

        public Vector3 Direction => Vector3.Transform(Vector3.UnitZ, Orientation);
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Orientation);

        public event EventHandler? Moved;

        public LocatableObject(int plateX, int plateZ, Matrix4x4 transform)
        {
            Locate(plateX, plateZ, transform);
        }

        public LocatableObject() : this(0, 0, Matrix4x4.Identity)
        {
        }

        public LocatableObject(int plateX, int plateZ, SixDoF position)
        {
            Locate(plateX, plateZ, position);
        }

        protected PlateOffset Locate(int plateX, int plateZ, Matrix4x4 transform, bool normalize)
        {
            PlateOffset plateOffset = PlateOffset.Identity;
            if (normalize)
            {
                Vector3 translation = transform.Translation;
                int dx = GetPlateDelta(translation.X);
                int dz = GetPlateDelta(translation.Z);

                transform.M41 -= dx * Plate.Size;
                transform.M43 -= dz * Plate.Size;

                plateX += dx;
                plateZ += dz;

                plateOffset = new PlateOffset(dx, dz);
            }

            PlateX = plateX;
            PlateZ = plateZ;
            Transform = transform;

            Matrix4x4.Decompose(Transform, out _, out Quaternion orientation, out Vector3 position);
            Position = position;
            Orientation = orientation;

            Moved?.Invoke(this, EventArgs.Empty);

            return plateOffset;


            static int GetPlateDelta(float delta)
            {
                return (int)Math.Floor(delta / Plate.Size);
            }
        }

        protected PlateOffset Locate(int plateX, int plateZ, Matrix4x4 transform)
        {
            return Locate(plateX, plateZ, transform, true);
        }

        protected PlateOffset Locate(int plateX, int plateZ, SixDoF position)
        {
            Matrix4x4 transform = position.CreateTransform();
            return Locate(plateX, plateZ, transform);
        }

        protected PlateOffset Locate(LocatableObject attachTo, Matrix4x4 transform)
        {
            return Locate(attachTo.PlateX, attachTo.PlateZ, transform);
        }

        protected PlateOffset Locate(LocatableObject attachTo)
        {
            return Locate(attachTo, attachTo.Transform);
        }

        protected PlateOffset Move(Matrix4x4 offset)
        {
            Matrix4x4 newTransform = offset * Transform;
            return Locate(PlateX, PlateZ, newTransform);
        }

        public PlateOffset GetPlateOffset(LocatableObject to)
        {
            return new PlateOffset(to.PlateX - PlateX, to.PlateZ - PlateZ);
        }
    }
}
