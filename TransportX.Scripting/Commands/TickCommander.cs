using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Commands
{
    public class TickCommander : Commander
    {
        public TimeSpan Elapsed { get; }

        internal TickCommander(Commander parent, TimeSpan elapsed) : base(parent)
        {
            Elapsed = elapsed;
        }
    }
}
