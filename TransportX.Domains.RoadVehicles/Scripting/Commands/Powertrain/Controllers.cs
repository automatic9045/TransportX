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
using TransportX.Domains.RoadVehicles.Powertrain.Controllers;
using TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class Controllers
    {
        private readonly ScriptAvatar Avatar;
        private readonly PowertrainComponent Source;

        public bool IsBuilt { get; private set; } = false;

        private readonly ScriptKeyedList<string, ControllerFactoryBase> FactoriesKey;
        public IReadOnlyScriptKeyedList<string, ControllerFactoryBase> Factories => FactoriesKey;

        private readonly ScriptKeyedList<string, ControllerCommand> AllKey;
        public IReadOnlyScriptKeyedList<string, ControllerCommand> All => AllKey;

        public Controllers(ScriptAvatar avatar, PowertrainComponent source)
        {
            Avatar = avatar;
            Source = source;

            FactoriesKey = new ScriptKeyedList<string, ControllerFactoryBase>(
                Controller => Controller.Key, Avatar.ErrorCollector, "駆動系コントローラーファクトリー", key => ControllerFactoryBase.InvalidEmpty(Avatar, key));
            AllKey = new ScriptKeyedList<string, ControllerCommand>(
                Controller => Controller.Key, Avatar.ErrorCollector, "駆動系コントローラー", key => ControllerCommand.InvalidEmpty(Avatar, key));
        }

        public void AddFactory(ControllerFactoryBase factory)
        {
            if (IsBuilt)
            {
                ScriptError error = new(ErrorLevel.Error, "既にビルド済の駆動系へ新たにコントローラーファクトリーを追加することはできません。");
                Avatar.ErrorCollector.Report(error);
                return;
            }

            FactoriesKey.Add(factory);
        }

        public EcuFactory AddEcu(string key)
        {
            EcuFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        public ShifterFactory<TKey> AddShifter<TKey>(string key, TKey defaultKey, Func<TKey, ShifterDirection, TKey> neighborKeyFactoryFallback) where TKey : notnull
        {
            ShifterFactory<TKey> factory = new(Avatar, key, defaultKey, neighborKeyFactoryFallback);
            AddFactory(factory);
            return factory;
        }

        public ShifterFactory<string> AddShifter(string key) => AddShifter(key, string.Empty, (key, direction) => $"{key}_{direction}");

        public ScriptableControllerFactory AddScriptable(string key)
        {
            ScriptableControllerFactory factory = new(Avatar, key);
            AddFactory(factory);
            return factory;
        }

        internal void Build()
        {
            if (IsBuilt) return;
            IsBuilt = true;

            foreach (ControllerFactoryBase factory in FactoriesKey)
            {
                factory.Build();
            }
        }

        public ControllerCommand Add(string key, IController controller)
        {
            ControllerCommand command = new(Avatar, key, controller);
            AllKey.Add(command);
            Source.Simulation.AddController(controller);
            return command;
        }
    }
}
