using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TransportX.Traffic;

using TransportX.Extensions.Traffic;
using TransportX.Network;
using System.Numerics;

namespace TransportX.Scripting.Components
{
    public interface ITrafficAgentTemplate
    {
        public static ITrafficAgentTemplate Default() => new DefaultTemplate();

        IParticipantFactory Build(XElement data);


        private class DefaultTemplate : ITrafficAgentTemplate
        {
            public IParticipantFactory Build(XElement data)
            {
                return new ParticipantFactory();
            }


            private class ParticipantFactory : IParticipantFactory
            {
                private static readonly ParticipantSpec Spec = new()
                {
                    Width = 1,
                    Height = 1,
                    Length = 1,
                };

                ParticipantSpec IParticipantFactory.Spec => Spec;

                public ITrafficParticipant Create(in TrafficSpawnContext context)
                {
                    return new Participant();
                }
            }

            private class Participant : LocatableObject, ITrafficParticipant
            {
                public float Width => 1;
                public float Height => 1;
                public float Length => 1;
                public bool IsEnabled => false;
                public ILanePath? Path => null;
                public ParticipantDirection Heading => ParticipantDirection.Forward;
                public float S => 0;
                public float SVelocity => 0;

                public bool Spawn(ILanePath path, ParticipantDirection heading, float s)
                {
                    return false;
                }
            }
        }
    }
}
