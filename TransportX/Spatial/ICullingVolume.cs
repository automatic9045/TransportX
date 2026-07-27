using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Spatial
{
    public interface ICullingVolume
    {
        ContainmentType Contains(in BoundingBox box);
        bool Intersects(in BoundingBox box);
    }


    public readonly struct FrustumCullingVolume : ICullingVolume
    {
        public BoundingFrustum Frustum { get; }

        public FrustumCullingVolume(BoundingFrustum frustum)
        {
            Frustum = frustum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ContainmentType Contains(in BoundingBox box) => Frustum.Contains(box);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(in BoundingBox box) => Frustum.Intersects(box);
    }


    public readonly struct SphereCullingVolume : ICullingVolume
    {
        public BoundingSphere Sphere { get; }

        public SphereCullingVolume(BoundingSphere sphere)
        {
            Sphere = sphere;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ContainmentType Contains(in BoundingBox box) => Sphere.Contains(box);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(in BoundingBox box) => Sphere.Intersects(box);
    }
}
