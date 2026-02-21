using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public static class BoundingFrustumExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ContainmentType Contains(this in BoundingFrustum frustum, in BoundingBox box)
        {
            ReadOnlySpan<Plane> planes = [frustum.Near, frustum.Far, frustum.Left, frustum.Right, frustum.Top, frustum.Bottom];

            bool intersects = false;
            for (int i = 0; i < 6; i++)
            {
                Plane plane = planes[i];

                Vector3 n = new(
                    0 <= plane.Normal.X ? box.Min.X : box.Max.X,
                    0 <= plane.Normal.Y ? box.Min.Y : box.Max.Y,
                    0 <= plane.Normal.Z ? box.Min.Z : box.Max.Z);

                if (0 < PlaneDotCoordinate(plane, n))
                {
                    return ContainmentType.Disjoint;
                }

                Vector3 p = new(
                    0 <= plane.Normal.X ? box.Max.X : box.Min.X,
                    0 <= plane.Normal.Y ? box.Max.Y : box.Min.Y,
                    0 <= plane.Normal.Z ? box.Max.Z : box.Min.Z);

                if (0 < PlaneDotCoordinate(plane, p))
                {
                    intersects = true;
                }
            }

            return intersects ? ContainmentType.Intersects : ContainmentType.Contains;


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float PlaneDotCoordinate(in Plane plane, in Vector3 point)
            {
                return (plane.Normal.X * point.X) + (plane.Normal.Y * point.Y) + (plane.Normal.Z * point.Z) + plane.D;
            }
        }
    }
}
