using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal class KeyboardATShifterInput : IATShifterInput
    {
        private readonly KeyObserver RKey;
        private readonly KeyObserver NKey;
        private readonly KeyObserver DKey;
        private readonly KeyObserver ModeKey;
        private readonly KeyObserver PlusKey;
        private readonly KeyObserver MinusKey;

        public event EventHandler? RPressed;
        public event EventHandler? NPressed;
        public event EventHandler? DPressed;
        public event EventHandler? ModePressed;
        public event EventHandler? PlusPressed;
        public event EventHandler? MinusPressed;

        public KeyboardATShifterInput(KeyObserver rKey, KeyObserver nKey, KeyObserver dKey, KeyObserver modeKey, KeyObserver plusKey, KeyObserver minusKey)
        {
            RKey = rKey;
            RKey.Pressed += (sender, e) => RPressed?.Invoke(sender, e);

            NKey = nKey;
            NKey.Pressed += (sender, e) => NPressed?.Invoke(sender, e);

            DKey = dKey;
            DKey.Pressed += (sender, e) => DPressed?.Invoke(sender, e);

            ModeKey = modeKey;
            ModeKey.Pressed += (sender, e) => ModePressed?.Invoke(sender, e);

            PlusKey = plusKey;
            PlusKey.Pressed += (sender, e) => PlusPressed?.Invoke(sender, e);

            MinusKey = minusKey;
            MinusKey.Pressed += (sender, e) => MinusPressed?.Invoke(sender, e);
        }
    }
}
