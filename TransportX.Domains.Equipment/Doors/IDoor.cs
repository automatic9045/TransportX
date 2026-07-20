using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.Equipment.Doors
{
    public interface IDoor
    {
        public static readonly IDoor Empty = new EmptyDoor();


        void Tick(TimeSpan elapsed);


        private class EmptyDoor : IDoor
        {
            public void Tick(TimeSpan elapsed)
            {
            }
        }
    }
}
