using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Worlds
{
    public interface IWorldInfo
    {
        string InfoPath { get; }

        string Title { get; }
        string Description { get; }
        string Author { get; }
        string Path { get; }
        string? Identifier { get; }
        string GamePath { get; }
        IReadOnlyList<string> Args { get; }
    }
}
