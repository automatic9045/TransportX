using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public interface IAppParameters
    {
        public static readonly EmptyAppParameters Empty = new();
    }

    public readonly struct EmptyAppParameters : IAppParameters
    {
    }
}
