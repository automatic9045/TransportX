using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class EmissiveLamp : ILamp
    {
        private readonly Vector3 InactiveEmissive;
        private readonly Vector3 ActiveEmissive;
        private readonly float TimeConstantInv;

        public Material Material { get; }
        public float ActiveRate { get; private set; } = 0;

        public EmissiveLamp(Material material, Vector3 emissive, float timeConstant)
        {
            Material = material;

            InactiveEmissive = material.Emissive;
            ActiveEmissive = emissive;
            TimeConstantInv = timeConstant < 1e-6f ? 1e6f : 1 / timeConstant;
        }

        public void Tick(bool isLighting, TimeSpan elapsed)
        {
            ActiveRate = float.Clamp(ActiveRate + TimeConstantInv * ((isLighting ? 1 : 0) - ActiveRate) * (float)elapsed.TotalSeconds, 0, 1);
            Material.Emissive = Vector3.Lerp(InactiveEmissive, ActiveEmissive, ActiveRate);
        }

        public void Reset()
        {
            ActiveRate = 0;
            Material.Emissive = InactiveEmissive;
        }
    }
}
