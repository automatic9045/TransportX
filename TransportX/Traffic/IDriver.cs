using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Traffic
{
    public interface IDriver
    {
        float Acceleration { get; }

        void Tick(TimeSpan elapsed);
    }
}
