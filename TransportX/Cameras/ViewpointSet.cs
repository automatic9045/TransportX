using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Avatars;

namespace TransportX.Cameras
{
    public class ViewpointSet
    {
        public FreeViewpoint Free { get; }

        public AvatarBase? AttachedTo
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                Update();
            }
        } = null;

        public ViewpointType Type
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                Update();
            }
        } = ViewpointType.Free;

        public Viewpoint Current { get; private set; }

        public event Action? Updated;

        public ViewpointSet()
        {
            Free = new FreeViewpoint();
            Update();
        }

        [MemberNotNull(nameof(Current))]
        private void Update()
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

            Current = current;
            Updated?.Invoke();
        }
    }
}
