using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Bus.Common.Scenery.Networks
{
    public class NetworkPort
    {
        public string Name { get; }
        public NetworkElement Owner { get; }
        public Matrix4x4 Offset { get; }
        public LaneLayout Layout { get; }
        public IReadOnlyList<LanePin> Pins { get; }

        public NetworkPort? ConnectedPort { get; private set; } = null;
        public bool IsConnected => ConnectedPort is not null;

        public NetworkPort(string name, NetworkElement owner, Matrix4x4 offset, LaneLayout layout)
        {
            Name = name;
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
    }
}
