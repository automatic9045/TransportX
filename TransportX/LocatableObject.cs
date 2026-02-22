using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public class LocatableObject : ILocatable
    {
        public static readonly LocatableObject Origin = new();


        public int PlateX { get; private set; }
        public int PlateZ { get; private set; }
        public Pose Pose { get; private set; }
        public Vector3 PositionInWorld => ((ILocatable)this).PositionInWorld;

        public virtual Vector3 Velocity => Vector3.Zero;

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

        protected PlateOffset Locate(int plateX, int plateZ, Pose pose)
        {
            int dx = GetPlateDelta(pose.Position.X);
            int dz = GetPlateDelta(pose.Position.Z);

            pose = new(pose.Position - new Vector3(dx, 0, dz) * Plate.Size, pose.Orientation);

            plateX += dx;
            plateZ += dz;

            PlateOffset plateOffset = new(dx, dz);
            if (PlateX == plateX && PlateZ == plateZ && Pose == pose) return plateOffset;

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

        protected PlateOffset Locate(ILocatable attachTo, Pose pose)
        {
            return Locate(attachTo.PlateX, attachTo.PlateZ, pose);
        }

        protected PlateOffset Locate(ILocatable attachTo)
        {
            return Locate(attachTo.PlateX, attachTo.PlateZ, attachTo.Pose);
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

        public PlateOffset GetPlateOffset(ILocatable to) => ((ILocatable)this).GetPlateOffset(to);
    }
}
