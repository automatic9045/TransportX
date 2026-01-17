using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Collections;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkEdge : NetworkElement
    {
        public abstract NetworkPort Inlet { get; }
        public abstract NetworkPort Outlet { get; }
        public override IReadOnlyKeyedList<string, NetworkPort> Ports { get; }

        public NetworkEdge(int plateX, int plateZ, Pose pose) : base(plateX, plateZ, pose)
        {
            Ports = new PortSet(this);
        }

        public virtual void SetChild(NetworkEdge child)
        {
            Outlet.ConnectTo(child.Inlet);
        }


        protected class PortSet : IReadOnlyKeyedList<string, NetworkPort>
        {
            private readonly NetworkEdge Parent;

            private IReadOnlyList<NetworkPort> List => [Parent.Inlet, Parent.Outlet];

            public NetworkPort this[string key] => TryGetValue(key, out NetworkPort? item) ? item : throw new KeyNotFoundException();
            public NetworkPort this[int index] => List[index];
            public int Count => List.Count;

            public PortSet(NetworkEdge parent)
            {
                Parent = parent;
            }

            public bool Contains(string key) => TryGetValue(key, out _);
            public IEnumerator<NetworkPort> GetEnumerator() => List.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out NetworkPort item)
            {
                item = key == Parent.Inlet.Name ? Parent.Inlet
                    : key == Parent.Outlet.Name ? Parent.Outlet
                    : null;

                return item is not null;
            }
        }
    }
}
