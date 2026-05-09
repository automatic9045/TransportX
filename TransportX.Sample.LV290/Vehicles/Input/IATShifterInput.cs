using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal interface IATShifterInput
    {
        event Action? RPressed;
        event Action? NPressed;
        event Action? DPressed;
        event Action? ModePressed;
        event Action? PlusPressed;
        event Action? MinusPressed;
    }
}
