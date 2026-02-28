using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TransportX.Network;

using TransportX.Extensions.Traffic;

namespace TransportX.Scripting.Components
{
    public class RandomTrafficSpawnerTemplate : ITrafficSpawnerTemplate, IWorldInstantiable<RandomTrafficSpawnerTemplate>
    {
        private readonly ScriptWorld World;

        public RandomTrafficSpawnerTemplate(ScriptWorld world)
        {
            World = world;
        }

        public static RandomTrafficSpawnerTemplate Create(ScriptWorld world) => new(world);

        public ITrafficSpawner Build(LaneTrafficType type, in TrafficSpawnContext context, XElement data)
        {
            float density = (float?)data.Attribute("Density") ?? 0;
            float assumedSpeed = (float?)data.Attribute("AssumedSpeed") ?? 60;
            RandomTrafficSpawner spawner = new(type, context, density, assumedSpeed / 3.6f);
            return spawner;
        }
    }
}
