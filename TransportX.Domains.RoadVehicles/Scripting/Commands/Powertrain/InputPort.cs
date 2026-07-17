using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class InputPort
    {
        public OutputPort? ConnectedTo { get; private set; } = null;

        public Shaft? BuiltShaft { get; private set; } = null;

        public void ConnectTo(OutputPort output)
        {
            ConnectedTo = output;
            output.ConnectedTo = this;
        }

        public Shaft Build()
        {
            BuiltShaft = ConnectedTo is null ? Shaft.Default()
                : ConnectedTo.BuiltShaft
                ?? throw new InvalidOperationException($"{nameof(ConnectedTo)} の {nameof(OutputPort.BuiltShaft)} が null です。接続先を先にビルドする必要があります。");

            return BuiltShaft;
        }
    }
}
