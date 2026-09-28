using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Traffic
{
    public delegate void PathChangedEventHandler(ILanePath? oldPath, ILanePath? newPath);
}
