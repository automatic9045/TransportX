using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class RandomTrafficSpawner : ITrafficSpawner
    {
        private readonly ConcurrentDictionary<IParticipantFactory, List<ITrafficParticipant>> Participants = [];

        private readonly LaneTrafficType Type;
        private readonly TrafficSpawnContext Context;

        private readonly float Density;
        private readonly float AssumedSpeed;

        private IReadOnlyList<(ILanePath Path, ParticipantDirection Heading)> SourcePaths = [];
        private float[] CumulativeWeights = [];
        private float TotalWeight;
        private TimeSpan SinceLastSpawn = TimeSpan.Zero;

        public IList<IParticipantFactory> ParticipantFactories { get; } = [];

        public RandomTrafficSpawner(LaneTrafficType type, in TrafficSpawnContext context, float density, float assumedSpeed)
        {
            Type = type;
            Context = context;

            Density = density;
            AssumedSpeed = float.Max(1e-3f, assumedSpeed);
        }

        public void Initialize(IEnumerable<ILanePath> paths, IEnumerable<NetworkPort> sourcePorts)
        {
            if (Density <= 1e-6f) return;

            ILanePath[] targetPaths = paths
                .Where(path => path.AllowedTraffic.Contains(Type))
                .ToArray();

            foreach (ILanePath path in targetPaths)
            {
                float density = Density;
                if (path.Components.TryGet<TrafficDensityComponent>(out TrafficDensityComponent? component))
                {
                    density *= component.Factor;
                }

                float s = 0;
                while (s < path.Length)
                {
                    float u = 1 - Random.Shared.NextSingle();
                    float gap = -float.Log(u) / density;
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

            var rawSourcePaths = sourcePins.SelectMany(pin => Enumerable.Concat(
                pin.SourcePaths.Where(path => path.Directions.HasFlag(FlowDirections.Out)).Select(path => (Path: path, Heading: ParticipantDirection.Forward)),
                pin.DestPaths.Where(path => path.Directions.HasFlag(FlowDirections.In)).Select(path => (Path: path, Heading: ParticipantDirection.Backward))))
                .ToArray();

            float[] cumulativeWeights = new float[rawSourcePaths.Length];
            float totalWeight = 0;
            SourcePaths = rawSourcePaths.Select((item, i) =>
            {
                float weight = Density * AssumedSpeed;
                if (item.Path.Components.TryGet<TrafficDensityComponent>(out TrafficDensityComponent? component))
                {
                    weight *= component.Factor;
                }

                totalWeight += weight;
                cumulativeWeights[i] = totalWeight;

                return (item.Path, item.Heading);
            }).ToArray();
            CumulativeWeights = cumulativeWeights;
            TotalWeight = totalWeight;
        }

        public void Tick(TimeSpan elapsed)
        {
            if (TotalWeight < 1e-6f || SourcePaths.Count == 0) return;

            SinceLastSpawn += elapsed;

            float spawnInterval = 1 / TotalWeight;
            if (spawnInterval < SinceLastSpawn.TotalSeconds)
            {
                SinceLastSpawn -= TimeSpan.FromSeconds(spawnInterval);

                float r = Random.Shared.NextSingle() * TotalWeight;

                int index = Array.BinarySearch(CumulativeWeights, r);
                if (index < 0) index = ~index;
                if (SourcePaths.Count <= index) index = SourcePaths.Count - 1;

                (ILanePath path, ParticipantDirection heading) = SourcePaths[index];
                TrySpawnAt(path, heading, heading == ParticipantDirection.Backward ? path.Length : 0);
            }
        }

        private ITrafficParticipant? TrySpawnAt(ILanePath path, ParticipantDirection heading, float s)
        {
            IParticipantFactory factory = ParticipantFactories[Random.Shared.Next(ParticipantFactories.Count)];

            if (path.GetWidth(s).Total < factory.Spec.Width) return null;

            bool isOccupied = path.Participants.Any(p => float.Abs(p.S - s) < factory.Spec.Length);
            if (isOccupied) return null;

            ITrafficParticipant participant;

            List<ITrafficParticipant> participants = Participants.GetOrAdd(factory, _ => []);
            ITrafficParticipant? disabledParticipant = participants.FirstOrDefault(p => !p.IsEnabled);
            if (participants.Count == 0 || disabledParticipant is null)
            {
                participant = factory.Create(Context);
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
