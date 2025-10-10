using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scripting.Commands
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
