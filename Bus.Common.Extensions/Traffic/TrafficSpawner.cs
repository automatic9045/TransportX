using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public class TrafficSpawner
    {
        private readonly ConcurrentDictionary<IParticipantFactory, List<ITrafficParticipant>> Participants = [];

        private readonly LaneTrafficType Type;
        private readonly float DefaultDensity;

        private IReadOnlyList<(ILanePath Path, ParticipantDirection Heading)> SourcePaths = [];
        private TimeSpan SinceLastSpawn = TimeSpan.Zero;

        public List<IParticipantFactory> ParticipantFactories { get; } = [];

        public TrafficSpawner(LaneTrafficType type, float defaultDensity)
        {
            Type = type;
            DefaultDensity = defaultDensity;
        }

        public void Initialize(IEnumerable<ILanePath> paths, IEnumerable<NetworkPort> sourcePorts)
        {
            IReadOnlyList<ILanePath> targetPaths = paths
                .Where(path => path.AllowedTraffic.Contains(Type))
                .ToArray();

            foreach (ILanePath path in targetPaths)
            {
                float s = 0;
                while (s < path.Length)
                {
                    float u = 1 - Random.Shared.NextSingle();
                    float gap = -float.Log(u) / DefaultDensity;
                    s += gap;

                    if (path.Length <= s) break;

                    ParticipantDirection heading = path.Directions switch
                    {
                        FlowDirections.In => ParticipantDirection.Backward,
                        FlowDirections.Out => ParticipantDirection.Forward,
                        FlowDirections.InOut => Random.Shared.GetItems([ParticipantDirection.Forward, ParticipantDirection.Backward], 1)[0],
                        _ => throw new NotSupportedException(),
                    };
                    TrySpawnAt(path, heading, s);
                }
            }


            IEnumerable<LanePin> sourcePins = sourcePorts
                .SelectMany(port => port.Pins)
                .Where(pin => pin.Definition.AllowedTraffic.Contains(Type));

            SourcePaths = sourcePins.SelectMany(pin => Enumerable.Concat(
                pin.SourcePaths.Where(path => path.Directions.HasFlag(FlowDirections.Out)).Select(path => (path, ParticipantDirection.Forward)),
                pin.DestPaths.Where(path => path.Directions.HasFlag(FlowDirections.In)).Select(path => (path, ParticipantDirection.Backward))
            )).ToArray();
        }

        public void Tick(TimeSpan elapsed)
        {
            SinceLastSpawn += elapsed;

            float spawnInterval = 1 / (DefaultDensity * 100);
            if (spawnInterval < SinceLastSpawn.TotalSeconds)
            {
                SinceLastSpawn = TimeSpan.Zero;
                if (SourcePaths.Count == 0) return;

                (ILanePath path, ParticipantDirection heading) = SourcePaths[Random.Shared.Next(SourcePaths.Count)];
                TrySpawnAt(path, heading, heading == ParticipantDirection.Backward ? path.Length : 0);
            }
        }

        private ITrafficParticipant? TrySpawnAt(ILanePath path, ParticipantDirection heading, float s)
        {
            IParticipantFactory factory = ParticipantFactories[Random.Shared.Next(ParticipantFactories.Count)];

            bool isOccupied = path.Participants.Any(p => float.Abs(p.S - s) < factory.Length);
            if (isOccupied) return null;

            ITrafficParticipant participant;

            List<ITrafficParticipant> participants = Participants.GetOrAdd(factory, _ => []);
            ITrafficParticipant? disabledParticipant = participants.FirstOrDefault(p => !p.IsEnabled);
            if (participants.Count == 0 || disabledParticipant is null)
            {
                participant = factory.Create();
                participants.Add(participant);
            }
            else
            {
                participant = disabledParticipant;
            }

            bool spawned = participant.Spawn(path, heading, s);
            return spawned ? participant : null;
        }
    }
}
