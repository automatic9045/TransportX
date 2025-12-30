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

        public NetworkPort? ConnectedPort { get; private set; } = null;

        public NetworkPort(NetworkElement owner, Matrix4x4 offset, LaneLayout layout)
        {
            Owner = owner;
            Offset = offset;
            Layout = layout;
            Pins = Layout.CreatePins(this);
        }

        public void ConnectTo(NetworkPort port)
        {
            if (!Layout.CanConnectTo(port.Layout)) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(port));

            ConnectedPort = port;
            port.ConnectedPort = this;
        }


        public class Inlet : NetworkPort
        {
            public Inlet(NetworkElement owner, LaneLayout layout) : base(owner, Matrix4x4.CreateRotationY(float.Pi), layout)
            {
            }
        }
    }
}
