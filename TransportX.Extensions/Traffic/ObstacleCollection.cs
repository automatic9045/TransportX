using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Bodies;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class ObstacleCollection : IEnumerable<ITrafficParticipant>
    {
        private readonly IReadOnlyList<RigidBody> Source;

        public ObstacleCollection(IReadOnlyList<RigidBody> source)
        {
            Source = source;
        }

        public Enumerator GetEnumerator() => new(Source);
        IEnumerator<ITrafficParticipant> IEnumerable<ITrafficParticipant>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public struct Enumerator : IEnumerator<ITrafficParticipant>
        {
            private readonly IReadOnlyList<RigidBody> Source;

            private int Index = 0;
            private ITrafficParticipant? CurrentKey = default;

            public readonly ITrafficParticipant Current => CurrentKey!;
            readonly object IEnumerator.Current => CurrentKey!;

            internal Enumerator(IReadOnlyList<RigidBody> source)
            {
                Source = source;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                while (Index < Source.Count)
                {
                    var item = Source[Index];
                    Index++;

                    if (item is ITrafficParticipant tp)
                    {
                        CurrentKey = tp;
                        return true;
                    }
                }

                CurrentKey = default;
                return false;
            }

            public void Reset()
            {
                Index = 0;
                CurrentKey = default;
            }
        }
    }
}
