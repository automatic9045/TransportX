using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

using TransportX.Scripting.Input;

namespace TransportX.Scripting.Commands
{
    public class AxisCommand
    {
        public static AxisCommand Empty(string key) => new(ScriptAxis.Empty(key));


        public IAxis Axis { get; }

        public string Key => Axis.Key;
        public float Min => Axis.Min;
        public float Neutral => Axis.Neutral;
        public float Max => Axis.Max;
        public float Value => Axis.Value;

        internal AxisCommand(IAxis axis)
        {
            Axis = axis;
        }
    }
}
