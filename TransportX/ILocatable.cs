using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public interface ILocatable
    {
        int PlateX { get; }
        int PlateZ { get; }
        Pose Pose { get; }
        Vector3 Velocity { get; }

        sealed Vector3 PositionInWorld => Pose.Position + new Vector3(PlateX, 0, PlateZ) * Plate.Size; // 注意: 原点から離れたプレート上では、誤差が大きい可能性あり

        event Action<PlateOffset>? Moved;

        sealed PlateOffset GetPlateOffset(ILocatable to)
        {
            return new PlateOffset(to.PlateX - PlateX, to.PlateZ - PlateZ);
        }
    }
}
