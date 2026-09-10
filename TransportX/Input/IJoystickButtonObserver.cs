using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public interface IJoystickButtonObserver : IDisposable
    {
        bool IsPressed { get; }
    }
}
