using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public readonly struct PlateOffset
    {
        public static readonly PlateOffset Identity = new PlateOffset();


        public int DeltaX { get; }
        public int DeltaZ { get; }
        public Vector3 Position { get; }
        public Pose Pose => new(Position);
        public Pose PoseInverse => new(-Position);

        public readonly bool IsZero => DeltaX == 0 && DeltaZ == 0;

        public PlateOffset() : this(0, 0)
        {
        }

        public PlateOffset(int deltaX, int deltaZ)
        {
            DeltaX = deltaX;
            DeltaZ = deltaZ;

            Position = Plate.Size * new Vector3(deltaX, 0, deltaZ);
        }

        public static bool operator ==(PlateOffset left, PlateOffset right) => left.DeltaX == right.DeltaX && left.DeltaZ == right.DeltaZ;
        public static bool operator !=(PlateOffset left, PlateOffset right) => !(left == right);
        public static PlateOffset operator +(PlateOffset left, PlateOffset right) => new PlateOffset(left.DeltaX + right.DeltaX, left.DeltaZ + right.DeltaZ);
        public static PlateOffset operator -(PlateOffset left, PlateOffset right) => new PlateOffset(left.DeltaX - right.DeltaX, left.DeltaZ - right.DeltaZ);

        public override bool Equals(object? obj) => obj is PlateOffset offset && this == offset;
        public override int GetHashCode() => HashCode.Combine(DeltaX, DeltaZ);
    }
}
