using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Vortice.Mathematics;

using TransportX.Input;

namespace TransportX.Cameras
{
    public class ViewpointInput : IDisposable
    {
        private readonly KeyObserver Driver;
        private readonly KeyObserver Passenger;
        private readonly KeyObserver Bird;
        private readonly KeyObserver Free;

        private readonly KeyObserver Forward;
        private readonly KeyObserver Backward;
        private readonly KeyObserver Reset;

        public SizeI ClientSize { get; set; } = SizeI.Empty;

        public ViewpointInput(IInputClient inputClient, ViewpointSet viewpoints)
        {
            inputClient.MouseScroll += (mouse, delta) => viewpoints.Current.Zoom(delta.Y);

            inputClient.MouseMove += (mouse, delta) =>
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

            Forward = inputClient.ObserveKey(Key.R);
            Forward.Pressed += keyboard => viewpoints.Current.NextPreset();

            Backward = inputClient.ObserveKey(Key.E);
            Backward.Pressed += keyboard => viewpoints.Current.PreviousPreset();

            Reset = inputClient.ObserveKey(Key.Space);
            Reset.Pressed += keyboard => viewpoints.Current.Reset();


            KeyObserver ObserveKey(Key key, ViewpointType viewpointType)
            {
                KeyObserver keyObserver = inputClient.ObserveKey(key);
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

            Forward.Dispose();
            Backward.Dispose();
            Reset.Dispose();
        }
    }
}
