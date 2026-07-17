using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Diagnostics;
using TransportX.Mathematics;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Collections;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Controllers;
using TransportX.Domains.RoadVehicles.Powertrain.Modules;
using TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories
{
    public class EcuFactory : ControllerFactoryBase
    {
        private Signal<float> PedalThrottleValue = new(0);
        private Signal<float> MinThrottleValue = new(0);
        private Signal<float> MaxThrottleValue = new(0);

        private EngineFactory? Engine = null;
        private ClutchFactoryBase? Clutch = null;
        private GearboxFactory? Gearbox = null;

        private bool AntiStallValue = false;
        private PidGains IdlingGainsValue = new(0, 0, 0);

        private float NIdlingRpm = 0;
        private float NonNIdlingRpm = 0;
        private float LimitRpmValue = float.MaxValue;

        public new Ecu? BuiltController { get; private set; } = null;

        internal EcuFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
        }

        public EcuFactory PedalThrottle(Signal<float> signal)
        {
            PedalThrottleValue = signal;
            return this;
        }

        public EcuFactory PedalThrottle(string signalKey)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return PedalThrottle(signal);
        }

        public EcuFactory MinMaxThrottle(Signal<float> minSignal, Signal<float> maxSignal)
        {
            MinThrottleValue = minSignal;
            MaxThrottleValue = maxSignal;
            return this;
        }

        public EcuFactory MinMaxThrottle(string minSignalKey, string maxSignalKey)
        {
            Signal<float> minSignal = Avatar.Commander.Signals.Float(minSignalKey);
            Signal<float> maxSignal = Avatar.Commander.Signals.Float(maxSignalKey);
            return MinMaxThrottle(minSignal, maxSignal);
        }

        public EcuFactory Modules(EngineFactory engine, ClutchFactoryBase clutch, GearboxFactory gearbox)
        {
            Engine = engine;
            Clutch = clutch;
            Gearbox = gearbox;

            return this;
        }

        public EcuFactory Modules(string engineKey, string clutchKey, string gearboxKey)
        {
            if (!TryGetFactoryAndCast(engineKey, out EngineFactory? engine)) return this;
            if (!TryGetFactoryAndCast(clutchKey, out ClutchFactoryBase? clutch)) return this;
            if (!TryGetFactoryAndCast(gearboxKey, out GearboxFactory? gearbox)) return this;

            return Modules(engine, clutch, gearbox);


            bool TryGetFactoryAndCast<TFactory>(string moduleKey, [NotNullWhen(true)] out TFactory? factory) where TFactory : ModuleFactoryBase
            {
                IReadOnlyScriptKeyedList<string, ModuleFactoryBase> moduleFactories = Avatar.Commander.Component<Powertrain>().Modules.Factories;

                if (!moduleFactories.GetValue(moduleKey, out ModuleFactoryBase factoryBase))
                {
                    factory = null;
                    return false;
                }

                if (factoryBase is not TFactory factoryCast)
                {
                    ScriptError error = new(ErrorLevel.Error, $"{moduleFactories.ItemName} '{moduleKey}' は {typeof(TFactory).Name} ではありません。");
                    Avatar.ErrorCollector.Report(error);

                    factory = null;
                    return false;
                }

                factory = factoryCast;
                return true;
            }
        }

        public EcuFactory AntiStall()
        {
            AntiStallValue = true;
            return this;
        }

        public EcuFactory IdlingGains(PidGains gains)
        {
            IdlingGainsValue = gains;
            return this;
        }

        public EcuFactory IdlingGains(double p, double i, double d)
        {
            PidGains gains = new((float)p, (float)i, (float)d);
            return IdlingGains(gains);
        }

        public EcuFactory IdlingRpm(double rpmForN, double rpmForNonN)
        {
            NIdlingRpm = (float)rpmForN;
            NonNIdlingRpm = (float)rpmForNonN;
            return this;
        }

        public EcuFactory LimitRpm(double rpm)
        {
            LimitRpmValue = (float)rpm;
            return this;
        }

        protected override IController OnBuild()
        {
            if (!TryGetBuiltModule(Engine, "エンジン", out Engine? engine)) return IController.Empty();
            if (!TryGetBuiltModule(Clutch, "クラッチ", out ClutchBase? clutch)) return IController.Empty();
            if (!TryGetBuiltModule(Gearbox, "変速機", out Gearbox? gearbox)) return IController.Empty();

            BuiltController = new Ecu(engine, clutch, gearbox)
            {
                PedalThrottle = PedalThrottleValue,
                MinThrottle = MinThrottleValue,
                MaxThrottle = MaxThrottleValue,

                AntiStall = AntiStallValue,
                IdlingGains = IdlingGainsValue,
                
                NIdlingRpm = NIdlingRpm,
                NonNIdlingRpm = NonNIdlingRpm,
                LimitRpm = LimitRpmValue,
            };
            return BuiltController;


            bool TryGetBuiltModule<T>(ModuleFactoryBase? factory, string name, [NotNullWhen(true)] out T? built) where T : class, IModule
            {
                if (factory is null)
                {
                    ScriptError error = new(ErrorLevel.Error, $"{name}が指定されていません。");
                    Avatar.ErrorCollector.Report(error);

                    built = null;
                    return false;
                }

                if (factory.BuiltModule is null)
                {
                    ScriptError error = new(ErrorLevel.Error, $"{name}がビルドされていません。");
                    Avatar.ErrorCollector.Report(error);

                    built = null;
                    return false;
                }

                built = factory.BuiltModule.Module as T ?? throw new InvalidOperationException();
                return true;
            }
        }
    }
}
