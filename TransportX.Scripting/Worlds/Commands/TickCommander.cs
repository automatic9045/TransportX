using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Worlds.Commands
{
    public class TickCommander : Commander
    {
        public TimeSpan Elapsed { get; internal set; } = TimeSpan.Zero;

        internal TickCommander(Commander parent) : base(parent)
        {
        }


        public class Factory
        {
            public TickCommander Commander { get; }

            public Factory(Commander parent)
            {
                Commander = new TickCommander(parent);
            }

            public TickCommander Tick(TimeSpan elapsed)
            {
                Commander.Elapsed = elapsed;
                return Commander;
            }
        }
    }
}
