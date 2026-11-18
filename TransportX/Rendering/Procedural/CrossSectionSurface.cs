using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Physics;

namespace TransportX.Rendering.Procedural
{
    public class CrossSectionSurface
    {
        public IReadOnlyList<CrossSectionVertex> Vertices { get; }
        public Material Material { get; }

        public float VInterval { get; init; } = 1;
        public float VOffset { get; init; } = 0;

        public ColliderMaterial? ColliderMaterial { get; init; } = null;
        public bool IsCollidable => ColliderMaterial is not null;

        public CrossSectionSurface(IReadOnlyList<CrossSectionVertex> vertices, float vInterval, float vOffset, Material material, ColliderMaterial? colliderMaterial)
        {
            if (vertices.Count < 2) throw new ArgumentException("サーフェスを構成するには最低 2 つの頂点が必要です。", nameof(vertices));

            Vertices = vertices;
            VInterval = vInterval;
            VOffset = vOffset;
            Material = material;
            ColliderMaterial = colliderMaterial;
        }

        public CrossSectionSurface(IReadOnlyList<CrossSectionVertex> vertices, Material material, ColliderMaterial? colliderMaterial)
            : this(vertices, 1, 0, material, colliderMaterial)
        {
        }

        public CrossSectionSurface(IReadOnlyList<CrossSectionVertex> vertices, float vInterval, float vOffset, Material material)
            : this(vertices, vInterval, vOffset, material, null)
        {
        }

        public CrossSectionSurface(IReadOnlyList<CrossSectionVertex> vertices, Material material)
            : this(vertices, material, null)
        {
        }
    }
}
