using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common
{
    public class LocatableObject
    {
        public int PlateX { get; private set; }
        public int PlateZ { get; private set; }
        public Matrix4x4 Locator { get; private set; }

        public Vector3 Position => Locator.Translation;
        public Vector3 PositionInWorld => Position + Plate.Size * new Vector3(PlateX, 0, PlateZ); // 注意: 原点から離れたプレート上では、誤差が大きい可能性あり
        public Vector3 Direction { get; private set; }
        public Vector3 Up { get; private set; }

        public event EventHandler? Moved;

        public LocatableObject(int plateX, int plateZ, Matrix4x4 locator)
        {
            Locate(plateX, plateZ, locator);
        }

        public LocatableObject() : this(0, 0, Matrix4x4.Identity)
        {
        }

        public LocatableObject(int plateX, int plateZ, SixDoF position)
        {
            Locate(plateX, plateZ, position);
        }

        protected void Locate(int plateX, int plateZ, Matrix4x4 locator, bool normalize)
        {
            if (normalize)
            {
                Vector3 translation = locator.Translation;
                int dx = GetPlateDelta(translation.X);
                int dz = GetPlateDelta(translation.Z);

                locator.M41 -= dx * Plate.Size;
                locator.M43 -= dz * Plate.Size;

                plateX += dx;
                plateZ += dz;
            }

            PlateX = plateX;
            PlateZ = plateZ;
            Locator = locator;

            Direction = Vector4.Transform(Vector4.UnitZ, Locator).AsVector3();
            Up = Vector4.Transform(Vector4.UnitY, Locator).AsVector3();

            Moved?.Invoke(this, EventArgs.Empty);


            static int GetPlateDelta(float delta)
            {
                return delta < 0 ? (int)(-delta / Plate.Size) - 1 : (int)(delta / Plate.Size);
            }
        }

        protected void Locate(int plateX, int plateZ, Matrix4x4 locator)
        {
            Locate(plateX, plateZ, locator, true);
        }

        protected void Locate(int plateX, int plateZ, SixDoF position)
        {
            Matrix4x4 locator = position.CreateTransform();
            Locate(plateX, plateZ, locator);
        }

        protected void Locate(LocatableObject attachTo, Matrix4x4 locator)
        {
            Locate(attachTo.PlateX, attachTo.PlateZ, locator);
        }

        protected void Locate(LocatableObject attachTo)
        {
            Locate(attachTo, attachTo.Locator);
        }

        protected void Move(Matrix4x4 transform)
        {
            Matrix4x4 newLocator = transform * Locator;
            Locate(PlateX, PlateZ, newLocator);
        }
    }
}
