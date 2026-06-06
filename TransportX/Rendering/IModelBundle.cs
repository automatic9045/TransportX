using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public interface IModelBundle : IDisposable
    {
        string Key { get; }
        IReadOnlyDictionary<string, IModel> Models { get; }
    }
}
