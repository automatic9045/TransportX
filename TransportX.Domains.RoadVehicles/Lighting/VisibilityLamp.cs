using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public class VisibilityLamp : ILamp
    {
        public TransformedModel Model { get; }

        public VisibilityLamp(TransformedModel model)
        {
            Model = model;
        }

        public void Tick(bool isLighting, TimeSpan elapsed)
        {
            Model.IsVisible = isLighting;
        }

        public void Reset()
        {
            Model.IsVisible = false;
        }
    }
}
