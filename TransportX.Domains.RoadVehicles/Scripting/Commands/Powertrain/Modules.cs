using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Collections;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain;
using TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class Modules
    {
        private readonly ScriptAvatar Avatar;
        private readonly PowertrainComponent Source;

        public bool IsBuilt { get; private set; } = false;

        private readonly ScriptKeyedList<string, ModuleFactoryBase> FactoriesKey;
        public IReadOnlyScriptKeyedList<string, ModuleFactoryBase> Factories => FactoriesKey;

        private readonly ScriptKeyedList<string, ModuleCommand> AllKey;
        public IReadOnlyScriptKeyedList<string, ModuleCommand> All => AllKey;

        public Modules(ScriptAvatar avatar, PowertrainComponent source)
        {
            Avatar = avatar;
            Source = source;

            FactoriesKey = new ScriptKeyedList<string, ModuleFactoryBase>(
                module => module.Key, Avatar.ErrorCollector, "駆動系モジュールファクトリー", key => ModuleFactoryBase.InvalidEmpty(Avatar, key));
            AllKey = new ScriptKeyedList<string, ModuleCommand>(
                module => module.Key, Avatar.ErrorCollector, "駆動系モジュール", key => ModuleCommand.InvalidEmpty(Avatar, key));
        }

        public void AddFactory(ModuleFactoryBase factory)
        {
            if (IsBuilt)
            {
                ScriptError error = new(ErrorLevel.Error, "既にビルド済の駆動系へ新たにモジュールファクトリーを追加することはできません。");
                Avatar.ErrorCollector.Report(error);
                return;
            }

            FactoriesKey.Add(factory);
        }

        public EngineFactory AddEngine(string key)
        {
            EngineFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        public FrictionClutchFactory AddFrictionClutch(string key)
        {
            FrictionClutchFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        public FluidClutchFactory AddFluidClutch(string key)
        {
            FluidClutchFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        public GearboxFactory AddGearbox(string key)
        {
            GearboxFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        public DifferentialFactory AddDifferential(string key)
        {
            DifferentialFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        public WheelAdapterFactory AddWheelAdapter(string key)
        {
            WheelAdapterFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        internal void Build()
        {
            if (IsBuilt) return;
            IsBuilt = true;

            foreach (ModuleFactoryBase factory in FactoriesKey)
            {
                factory.Build();
            }
        }

        public ModuleCommand Add(string key, IModule module)
        {
            ModuleCommand command = new(Avatar, key, module);
            AllKey.Add(command);
            Source.Simulation.AddModule(module);
            return command;
        }
    }
}
