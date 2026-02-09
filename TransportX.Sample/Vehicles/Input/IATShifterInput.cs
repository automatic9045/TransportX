using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Input
{
    internal interface IATShifterInput
    {
        event EventHandler? RPressed;
        event EventHandler? NPressed;
        event EventHandler? DPressed;
        event EventHandler? ModePressed;
        event EventHandler? PlusPressed;
        event EventHandler? MinusPressed;
    }
}
