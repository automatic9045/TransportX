using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

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
            inputManager.MouseScroll += (mouse, delta) => viewpoints.Current.Zoom(delta.Y);

            inputManager.MouseMove += (mouse, delta) =>
            {
                if (mouse.IsButtonPressed(MouseButton.Middle))
                {
                    viewpoints.Current.Move(delta, ClientSize);
                }

                if (mouse.IsButtonPressed(MouseButton.Right))
                {
                    viewpoints.Current.Rotate(delta, ClientSize);
                }
            };

            Driver = ObserveKey(Key.F1, ViewpointType.Driver);
            Passenger = ObserveKey(Key.F2, ViewpointType.Passenger);
            Bird = ObserveKey(Key.F3, ViewpointType.Bird);
            Free = ObserveKey(Key.F4, ViewpointType.Free);

            Reset = inputManager.ObserveKey(Key.Space);
            Reset.Pressed += keyboard => viewpoints.Current.Reset();


            KeyObserver ObserveKey(Key key, ViewpointType viewpointType)
            {
                KeyObserver keyObserver = inputManager.ObserveKey(key);
                keyObserver.Pressed += keyboard => viewpoints.Type = viewpointType;
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
