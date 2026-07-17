using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Modules;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories
{
    public class WheelAdapterFactory : ModuleFactoryBase
    {
        private bool ReverseDirection = false;
        private DynamicPart? WheelPartValue = null;

        public InputPort Input { get; } = new();

        public new WheelAdapter? BuiltModule { get; private set; } = null;

        internal WheelAdapterFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public WheelAdapterFactory Reverse()
        {
            ReverseDirection = true;
            return this;
        }

        public WheelAdapterFactory WheelPart(DynamicPart part)
        {
            WheelPartValue = part;
            return this;
        }

        public WheelAdapterFactory WheelPart(string partKey)
        {
            if (!Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part)) return this;
            if (part is not DynamicPart wheelPart)
            {
                ScriptError error = new(ErrorLevel.Error, "車輪にダイナミックでないパーツを指定することはできません。");
                Avatar.ErrorCollector.Report(error);
                return this;
            }

            return WheelPart(wheelPart);
        }

        protected override IModule OnBuild()
        {
            if (WheelPartValue is null)
            {
                ScriptError error = new(ErrorLevel.Error, "車輪パーツが指定されていません。");
                Avatar.ErrorCollector.Report(error);
                return IModule.Empty();
            }

            Shaft input = Input.Build();

            BuiltModule = new WheelAdapter(input, WheelPartValue.Model, ReverseDirection);
            return BuiltModule;
        }
    }
}
