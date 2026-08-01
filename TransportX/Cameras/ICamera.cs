using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Spatial;

namespace TransportX.Cameras
{
    public interface ICamera : IWorldObject
    {
        float Perspective { get; set; }
        VisualLayers VisibleLayers { get; set; }

        void UpdateView(in WorldPose worldPose);
        ViewContext CreateViewContext(SizeI clientSize);


        [Flags]
        public enum VisualLayers
        {
            None = 0b0000,
            Normal = 0x0001,
            Colliders = 0b0010,
            Network = 0b0100,
            Traffic = 0b1000,
        }
    }
}
