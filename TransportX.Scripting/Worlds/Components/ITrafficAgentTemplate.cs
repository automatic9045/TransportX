using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TransportX.Network;
using TransportX.Spatial;
using TransportX.Traffic;

using TransportX.Extensions.Traffic;

namespace TransportX.Scripting.Worlds.Components
{
    public interface ITrafficAgentTemplate
    {
        public static ITrafficAgentTemplate Default() => new DefaultTemplate();

        IEntityFactory Build(XElement data);


        private class DefaultTemplate : ITrafficAgentTemplate
        {
            public IEntityFactory Build(XElement data)
            {
                return new EntityFactory();
            }


            private class EntityFactory : IEntityFactory
            {
                private static readonly EntitySpec Spec = new()
                {
                    Width = 1,
                    Height = 1,
                    Length = 1,
                };

                EntitySpec IEntityFactory.Spec => Spec;

                public ITrafficEntity Create(in TrafficSpawnContext context)
                {
                    return new Entity();
                }
            }

            private class Entity : WorldObject, ITrafficEntity
            {
                public Vector3 Velocity => Vector3.Zero;
                public Vector3 AngularVelocity => Vector3.Zero;

                public float Width => 1;
                public float Height => 1;
                public float Length => 1;
                public bool IsEnabled => false;
                public ILanePath? Path => null;
                public EntityDirection Heading => EntityDirection.Forward;
                public float S => 0;
                public float SVelocity => 0;

                public bool Spawn(ILanePath path, EntityDirection heading, float s)
                {
                    return false;
                }
            }
        }
    }
}
