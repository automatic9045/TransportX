using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Vehicles;

namespace Bus.Common.Rendering
{
    public class ViewpointSet
    {
        private readonly Viewpoint Free = new FreeViewpoint(new LocatableObject(0, 0, new SixDoF(0, 10, 0)));

        public VehicleBase? AttachedTo { get; set; } = null;
        public ViewpointType Type { get; set; } = ViewpointType.Free;
        public Viewpoint Current
        {
            get
            {
                Viewpoint? current = Type switch
                {
                    ViewpointType.Driver => AttachedTo?.DriverViewpoint,
                    ViewpointType.Passenger => null,
                    ViewpointType.Bird => AttachedTo?.BirdViewpoint,
                    ViewpointType.Free => Free,
                    _ => throw new InvalidOperationException(),
                };

                if (current is null)
                {
                    Type = ViewpointType.Free;
                    current = Free;
                }

                return current;
            }
        }
    }
}
