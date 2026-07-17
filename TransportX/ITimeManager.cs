using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public interface ITimeManager
    {
        double Frequency { get; }
        TimeSpan DeltaTime { get; }
        DateTime Now { get; set; }
    }
}
