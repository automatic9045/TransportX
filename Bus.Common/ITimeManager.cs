using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common
{
    public interface ITimeManager
    {
        double Fps { get; }
        TimeSpan DeltaTime { get; }
    }
}
