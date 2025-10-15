using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery
{
    public readonly struct PlateOffset
    {
        public static readonly PlateOffset Identity = new PlateOffset();


        public int DeltaX { get; }
        public int DeltaZ { get; }
        public Matrix4x4 Transform { get; }
        public Matrix4x4 TransformInverse { get; }

        public readonly bool IsZero => DeltaX == 0 && DeltaZ == 0;

        public PlateOffset() : this(0, 0)
        {
        }

        public PlateOffset(int deltaX, int deltaZ)
        {
            DeltaX = deltaX;
            DeltaZ = deltaZ;

            Vector3 platePosition = Plate.Size * new Vector3(deltaX, 0, deltaZ);
            Transform = Matrix4x4.CreateTranslation(platePosition);
            TransformInverse = Matrix4x4.CreateTranslation(-platePosition);
        }

        public static bool operator ==(PlateOffset left, PlateOffset right) => left.DeltaX == right.DeltaX && left.DeltaZ == right.DeltaZ;
        public static bool operator !=(PlateOffset left, PlateOffset right) => !(left == right);

        public override bool Equals(object? obj) => obj is PlateOffset offset && this == offset;
        public override int GetHashCode() => HashCode.Combine(DeltaX, DeltaZ);
    }
}
