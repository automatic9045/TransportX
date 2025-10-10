using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace Bus.Common.Rendering
{
    public interface IModel : IDisposable
    {
        void Draw(ID3D11DeviceContext context);
    }
}
