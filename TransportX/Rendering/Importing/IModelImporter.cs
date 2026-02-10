using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal interface IModelImporter : IDisposable
    {
        Model Import(string path, bool isForVisual, bool makeLH);
    }
}
