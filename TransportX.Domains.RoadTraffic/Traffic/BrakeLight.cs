using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    internal class BrakeLight
    {
        private readonly TransformedModel Model;

        public bool IsActive { get; set; } = false;

        public BrakeLight(TransformedModel model)
        {
            Model = model;
        }

        public void Tick(TimeSpan elapsed)
        {
            Model.IsVisible = IsActive;
        }
    }
}
