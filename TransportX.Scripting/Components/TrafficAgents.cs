using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Traffic;

using TransportX.Scripting.Data;

namespace TransportX.Scripting.Components
{
    public class TrafficAgents : IWorldInstantiable<TrafficAgents>, IComponentCommand
    {
        private readonly ScriptWorld World;

        private readonly ScriptDictionary<string, ITrafficSpawnerTemplate> Spawners;
        private readonly ScriptDictionary<string, ITrafficAgentTemplate> Agents;

        private readonly TrafficSpawnContext SpawnContext;

        public TrafficSpawnerComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        public TrafficAgents(ScriptWorld world)
        {
            World = world;

            Spawners = new ScriptDictionary<string, ITrafficSpawnerTemplate>(
                World.ErrorCollector, "エージェント発生ルール", key => ITrafficSpawnerTemplate.Default());
            Agents = new ScriptDictionary<string, ITrafficAgentTemplate>(
                World.ErrorCollector, "エージェント", key => ITrafficAgentTemplate.Default());

            SpawnContext = new TrafficSpawnContext()
            {
                PhysicsHost = World.PhysicsHost,
                Bodies = World.Bodies,
                Obstacles = new ObstacleCollection(World.Bodies),
            };

            Source = new TrafficSpawnerComponent(World);
        }

        public static TrafficAgents Create(ScriptWorld world) => new(world);

        public void AddSpawner<T>(string key) where T : IWorldInstantiable<T>, ITrafficSpawnerTemplate
        {
            ITrafficSpawnerTemplate template = T.Create(World);
            Spawners.Add(key, template);
        }

        public void AddAgent<T>(string key) where T : IWorldInstantiable<T>, ITrafficAgentTemplate
        {
            ITrafficAgentTemplate template = T.Create(World);
            Agents.Add(key, template);
        }

        public void Generate(string trafficTypeKey, string listPath)
        {
            if (!World.Commander.Network.LaneTraffic.Types.GetValue(trafficTypeKey, out LaneTrafficType trafficType)) return;

            string absolutePath = Path.GetFullPath(Path.Combine(World.BaseDirectory, listPath));
            if (!File.Exists(absolutePath))
            {
                ScriptError error = new(ErrorLevel.Error, $"交通構成リスト '{absolutePath}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
                return;
            }

            Data.Traffic data;
            try
            {
                data = XmlSerializer<Data.Traffic>.FromXml(absolutePath);
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error , ex, $"交通構成リスト '{absolutePath}' を読み込めませんでした。");
                World.ErrorCollector.Report(error);
                return;
            }

            foreach (Data.Spawner spawnerData in data.Spawners)
            {
                World.ErrorCollector.ReportRange(spawnerData.Errors);

                ITrafficSpawnerTemplate spawnerTemplate = Spawners[spawnerData.Key.Value];
                ITrafficSpawner spawner = spawnerTemplate.Build(trafficType, SpawnContext, spawnerData.FullElement!);

                foreach (Data.Agent agentData in spawnerData.Agents)
                {
                    World.ErrorCollector.ReportRange(agentData.Errors);

                    ITrafficAgentTemplate agentTemplate = Agents[agentData.Key.Value];
                    IParticipantFactory factory = agentTemplate.Build(agentData.FullElement!);
                    spawner.ParticipantFactories.Add(factory);
                }

                Source.Spawners.Register(spawner);
            }
        }
    }
}
