using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Avatars;

namespace TransportX.Rendering
{
    public class ViewpointSet
    {
        private readonly Viewpoint Free;

        public AvatarBase? AttachedTo { get; set; } = null;
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

        public ViewpointSet(int plateX, int plateZ, Vector3 position, Vector2 angle)
        {
            Free = new FreeViewpoint(plateX, plateZ, position, angle);
        }
    }
}
