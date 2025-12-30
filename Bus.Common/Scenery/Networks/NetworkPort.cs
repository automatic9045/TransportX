using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Bus.Common.Scenery.Networks
{
    public class NetworkPort
    {
        public NetworkElement Owner { get; }
        public Matrix4x4 Offset { get; }
        public LaneLayout Layout { get; }
        public IReadOnlyList<LanePin> Pins { get; }

        public NetworkElement? Child { get; private set; } = null;

        public NetworkPort(NetworkElement owner, Matrix4x4 offset, LaneLayout layout)
        {
            Owner = owner;
            Offset = offset;
            Layout = layout;
            Pins = Layout.CreatePins(this);
        }

        protected internal void SetChild(NetworkElement child)
        {
            if (child.IsRoot) throw new ArgumentException($"{nameof(NetworkElement.IsRoot)} が true の {nameof(NetworkElement)} を子に設定することはできません。", nameof(child));
            if (!Layout.CanConnectTo(child.Inlet.Layout)) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(child));

            Child = child;

            for (int i = 0; i < Pins.Count; i++)
            {
                LanePin connectedPin = Child.Inlet.Pins[Pins.Count - 1 - i];
                Pins[i].ConnectTo(connectedPin);
            }
        }


        public class Inlet : NetworkPort
        {
            public Inlet(NetworkElement owner, LaneLayout layout) : base(owner, Matrix4x4.CreateRotationY(float.Pi), layout)
            {
            }
        }
    }
}
