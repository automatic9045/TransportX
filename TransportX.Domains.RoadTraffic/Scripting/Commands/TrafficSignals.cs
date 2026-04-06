using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Data;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public class TrafficSignals : IWorldInstantiable<TrafficSignals>, IComponentCommand
    {
        private readonly ScriptWorld World;

        public SignalCollectionComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        private readonly ScriptDictionary<string, ISignalController> ControllersKey;
        public IReadOnlyScriptDictionary<string, ISignalController> Controllers => ControllersKey;

        public TrafficSignals(ScriptWorld world)
        {
            World = world;
            Source = new SignalCollectionComponent(World);
            ControllersKey = new ScriptDictionary<string, ISignalController>(World.ErrorCollector, "信号制御機", key => SignalController.Empty);
        }

        public static TrafficSignals Create(ScriptWorld world) => new(world);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void AddController(string key, string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(BaseDirectory.Find() ?? World.BaseDirectory, path));
            if (!File.Exists(fullPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"信号制御機データファイル '{path}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
                return;
            }

            Data.Signals.TrafficSignalController data = XmlSerializer<Data.Signals.TrafficSignalController>.FromXml(fullPath);
            World.ErrorCollector.ReportRange(data.Errors);

            ScriptDictionary<string, SignalSchedule.Plan> allPlans = new(World.ErrorCollector, "信号制御プラン", _ => new SignalSchedule.Plan(TimeSpan.Zero, []));
            foreach (Data.Signals.Plan planData in data.Plans)
            {
                World.ErrorCollector.ReportRange(planData.Errors);

                List<SignalSchedule.Step> steps = planData.Steps.ConvertAll(stepData =>
                {
                    World.ErrorCollector.ReportRange(stepData.Errors);

                    Dictionary<string, SignalColor> signals = new(stepData.Groups.Count);
                    foreach (Data.Signals.Group groupData in stepData.Groups)
                    {
                        World.ErrorCollector.ReportRange(groupData.Errors);
                        signals[groupData.Key.Value] = groupData.Color.Value;
                    }

                    return new SignalSchedule.Step(stepData.Duration.Value, signals);
                });

                SignalSchedule.Plan plan = new(planData.Offset.Value, steps);
                allPlans[planData.Key.Value] = plan;
            }

            Dictionary<TimeSpan, SignalSchedule.Plan> plans = [];
            foreach (Data.Signals.PlanRef planRefData in data.Schedule)
            {
                World.ErrorCollector.ReportRange(planRefData.Errors);

                SignalSchedule.Plan plan = allPlans[planRefData.Key.Value];
                plans[planRefData.StartTime.Value] = plan;
            }

            SignalSchedule schedule = new(plans);
            SignalController controller = new(schedule);
            Source.Controllers.Add(controller);
            ControllersKey[key] = controller;
        }
    }
}
