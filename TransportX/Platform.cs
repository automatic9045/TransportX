using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Windowing;

namespace TransportX
{
    public class Platform
    {
        public required IWindow Window { get; init; }
        public required IInputContext Input { get; init; }
    }
}
