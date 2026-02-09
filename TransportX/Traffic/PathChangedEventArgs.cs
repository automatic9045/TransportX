using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Traffic
{
    public class PathChangedEventArgs : EventArgs
    {
        public ILanePath? OldPath { get; }
        public ILanePath? NewPath { get; }

        public PathChangedEventArgs(ILanePath? oldPath, ILanePath? newPath)
        {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }
}
