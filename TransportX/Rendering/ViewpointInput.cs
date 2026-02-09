using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Key = System.Windows.Input.Key;
using MouseButtonState = System.Windows.Input.MouseButtonState;

using TransportX.Input;

namespace TransportX.Rendering
{
    public class ViewpointInput : IDisposable
    {
        private readonly KeyObserver Driver;
        private readonly KeyObserver Passenger;
        private readonly KeyObserver Bird;
        private readonly KeyObserver Free;

        private readonly KeyObserver Reset;

        public Vector2 ClientSize { get; set; } = Vector2.Zero;

        public ViewpointInput(InputManager inputManager, ViewpointSet viewpoints)
        {
            inputManager.MouseDragMoved += (sender, e) =>
            {
                Vector2 offset = new Vector2((float)e.Offset.X, (float)e.Offset.Y);
                if (e.MiddleButton == MouseButtonState.Pressed) viewpoints.Current.Move(offset, ClientSize);
                if (e.RightButton == MouseButtonState.Pressed) viewpoints.Current.Rotate(offset, ClientSize);
            };

            inputManager.MouseWheel += (sender, e) => viewpoints.Current.Zoom(e.Delta);

            Driver = ObserveKey(Key.F1, ViewpointType.Driver);
            Passenger = ObserveKey(Key.F2, ViewpointType.Passenger);
            Bird = ObserveKey(Key.F3, ViewpointType.Bird);
            Free = ObserveKey(Key.F4, ViewpointType.Free);

            Reset = inputManager.ObserveKey(Key.Space);
            Reset.Pressed += (sender, e) => viewpoints.Current.Reset();


            KeyObserver ObserveKey(Key key, ViewpointType viewpointType)
            {
                KeyObserver keyObserver = inputManager.ObserveKey(key);
                keyObserver.Pressed += (sender, e) => viewpoints.Type = viewpointType;
                return keyObserver;
            }
        }

        public void Dispose()
        {
            Driver.Dispose();
            Passenger.Dispose();
            Bird.Dispose();
            Free.Dispose();

            Reset.Dispose();
        }
    }
}
