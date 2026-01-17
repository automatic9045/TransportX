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

        public Pose Pose { get; private set; }
        public Vector3 PositionInWorld => Pose.Position + Origin.GetPlateOffset(this).Position; // 注意: 原点から離れたプレート上では、誤差が大きい可能性あり
        public Matrix4x4 Transform => Pose.ToMatrix4x4();

        public Vector3 Direction => Vector3.Transform(Vector3.UnitZ, Pose.Orientation);
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Pose.Orientation);

        public event EventHandler? Moved;

        public LocatableObject(int plateX, int plateZ, Pose pose)
        {
            Locate(plateX, plateZ, pose);
        }

        public LocatableObject() : this(0, 0, Pose.Identity)
        {
        }

        public LocatableObject(int plateX, int plateZ, SixDoF position)
        {
            Locate(plateX, plateZ, position);
        }

        protected PlateOffset Locate(int plateX, int plateZ, Pose pose, bool normalize)
        {
            PlateOffset plateOffset = PlateOffset.Identity;
            if (normalize)
            {
                int dx = GetPlateDelta(pose.Position.X);
                int dz = GetPlateDelta(pose.Position.Z);

                pose = new(pose.Position - new Vector3(dx, 0, dz) * Plate.Size, pose.Orientation);

                plateX += dx;
                plateZ += dz;

                plateOffset = new PlateOffset(dx, dz);
            }

            PlateX = plateX;
            PlateZ = plateZ;
            Pose = pose;

            Moved?.Invoke(this, EventArgs.Empty);

            return plateOffset;


            static int GetPlateDelta(float delta)
            {
                return (int)float.Floor(delta / Plate.Size);
            }
        }

        protected PlateOffset Locate(int plateX, int plateZ, Pose pose)
        {
            return Locate(plateX, plateZ, pose, true);
        }

        protected PlateOffset Locate(int plateX, int plateZ, Matrix4x4 transform)
        {
            Pose pose = Pose.FromMatrix4x4(transform);
            return Locate(plateX, plateZ, pose);
        }

        protected PlateOffset Locate(int plateX, int plateZ, SixDoF position)
        {
            Pose pose = position.ToPose();
            return Locate(plateX, plateZ, pose);
        }

        protected PlateOffset Locate(LocatableObject attachTo, Pose pose)
        {
            return Locate(attachTo.PlateX, attachTo.PlateZ, pose);
        }

        protected PlateOffset Move(Pose delta)
        {
            Pose pose = delta * Pose;
            return Locate(PlateX, PlateZ, pose);
        }

        protected PlateOffset Move(Matrix4x4 offset)
        {
            Pose pose = Pose.FromMatrix4x4(offset);
            return Move(pose);
        }

        public PlateOffset GetPlateOffset(LocatableObject to)
        {
            return new PlateOffset(to.PlateX - PlateX, to.PlateZ - PlateZ);
        }
    }
}
