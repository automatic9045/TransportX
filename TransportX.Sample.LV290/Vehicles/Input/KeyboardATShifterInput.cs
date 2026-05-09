using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal class KeyboardATShifterInput : IATShifterInput
    {
        private readonly KeyObserver RKey;
        private readonly KeyObserver NKey;
        private readonly KeyObserver DKey;
        private readonly KeyObserver ModeKey;
        private readonly KeyObserver PlusKey;
        private readonly KeyObserver MinusKey;

        public event Action? RPressed;
        public event Action? NPressed;
        public event Action? DPressed;
        public event Action? ModePressed;
        public event Action? PlusPressed;
        public event Action? MinusPressed;

        public KeyboardATShifterInput(KeyObserver rKey, KeyObserver nKey, KeyObserver dKey, KeyObserver modeKey, KeyObserver plusKey, KeyObserver minusKey)
        {
            RKey = rKey;
            RKey.Pressed += keyboard => RPressed?.Invoke();

            NKey = nKey;
            NKey.Pressed += keyboard => NPressed?.Invoke();

            DKey = dKey;
            DKey.Pressed += keyboard => DPressed?.Invoke();

            ModeKey = modeKey;
            ModeKey.Pressed += keyboard => ModePressed?.Invoke();

            PlusKey = plusKey;
            PlusKey.Pressed += keyboard => PlusPressed?.Invoke();

            MinusKey = minusKey;
            MinusKey.Pressed += keyboard => MinusPressed?.Invoke();
        }
    }
}
