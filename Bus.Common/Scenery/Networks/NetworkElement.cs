using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkElement : LocatableObject, IDrawable
    {
        public bool IsRoot { get; }
        public abstract LaneLayout InletLayout { get; }
        public abstract IReadOnlyList<LanePin> InletPins { get; }
        public abstract IReadOnlyList<NetworkOutlet> Outlets { get; }

        public virtual IReadOnlyList<LocatedModel> Models { get; } = [];

        public NetworkElement(int plateX, int plateZ, Matrix4x4 transform, bool isRoot) : base(plateX, plateZ, transform)
        {
            IsRoot = isRoot;
        }

        protected void SetChild(int outletIndex, NetworkElement element)
        {
            Outlets[outletIndex].SetChild(element);
        }

        public void Draw(LocatedDrawContext context)
        {
            foreach (LocatedModel model in Models)
            {
                model.Draw(context);
            }
        }


        public class NetworkOutlet
        {
            public Matrix4x4 Transition { get; }
            public LaneLayout Layout { get; }
            public IReadOnlyList<LanePin> Pins { get; }

            public NetworkElement? Child { get; private set; } = null;

            public NetworkOutlet(NetworkElement parent, Matrix4x4 transition, LaneLayout layout)
            {
                Transition = transition;
                Layout = layout;
                Pins = Layout.CreatePins(parent);
            }

            protected internal void SetChild(NetworkElement child)
            {
                if (child.IsRoot) throw new ArgumentException($"{nameof(IsRoot)} が true の {nameof(NetworkElement)} を子に設定することはできません。", nameof(child));
                if (!Layout.CanConnectTo(child.InletLayout)) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(child));

                Child = child;

                for (int i = 0; i < Pins.Count; i++)
                {
                    LanePin connectedPin = Child.InletPins[Pins.Count - 1 - i];
                    Pins[i].ConnectTo(connectedPin);
                }
            }
        }
    }
}
