using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.LV290.Vehicles.Input;

namespace TransportX.Sample.LV290.Vehicles.Interfaces
{
    internal class AMTShifter
    {
        private bool LeverMoved = false;

        public IMTShifterInput Source { get; set; }

        public AMTShifterPosition Position { get; private set; } = AMTShifterPosition.N;

        public AMTShifter(IMTShifterInput defaultSource)
        {
            Source = defaultSource;
        }

        public void Tick(TimeSpan elapsed)
        {
            if (LeverMoved)
            {
                if (Source.Direction.Y == 0) LeverMoved = false;
            }
            else
            {
                switch (Position)
                {
                    case AMTShifterPosition.R:
                        if (Source.Direction.Y < 0) ChangeTo(AMTShifterPosition.N);
                        break;

                    case AMTShifterPosition.N:
                        if (Source.Direction.Y < 0) ChangeTo(AMTShifterPosition.D);
                        else if (0 < Source.Direction.Y) ChangeTo(AMTShifterPosition.R);
                        break;

                    case AMTShifterPosition.D:
                        if (0 < Source.Direction.Y) ChangeTo(AMTShifterPosition.N);
                        else if (0 < Source.Direction.X) ChangeTo(AMTShifterPosition.M);
                        break;

                    case AMTShifterPosition.M:
                        if (Source.Direction.Y < 0) ChangeTo(AMTShifterPosition.Minus);
                        else if (0 < Source.Direction.Y) ChangeTo(AMTShifterPosition.Plus);
                        else if (Source.Direction.X < 0) ChangeTo(AMTShifterPosition.D);
                        break;

                    case AMTShifterPosition.Plus:
                        if (Source.Direction.Y <= 0) ChangeTo(AMTShifterPosition.M);
                        break;

                    case AMTShifterPosition.Minus:
                        if (0 <= Source.Direction.Y) ChangeTo(AMTShifterPosition.M);
                        break;

                    default:
                        throw new NotSupportedException();
                }


                void ChangeTo(AMTShifterPosition position)
                {
                    Position = position;
                    LeverMoved = true;
                }
            }
        }
    }
}
