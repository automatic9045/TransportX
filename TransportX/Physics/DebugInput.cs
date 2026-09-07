using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Cameras;
using TransportX.Input;

namespace TransportX.Physics
{
    public class DebugInput : IDisposable
    {
        private static readonly ICamera.VisualLayers[] Modes = [
            ICamera.VisualLayers.Normal,
            ICamera.VisualLayers.Normal | ICamera.VisualLayers.Colliders,
            ICamera.VisualLayers.Normal | ICamera.VisualLayers.Network,
            ICamera.VisualLayers.Normal | ICamera.VisualLayers.Traffic,
        ];


        private readonly KeyObserver DrawColliderModel;
        private int ModeIndex = 0;

        public DebugInput(IInputClient inputClient, ICamera camera)
        {
            DrawColliderModel = inputClient.ObserveKey(Key.F6);
            DrawColliderModel.Pressed += keyboard =>
            {
                ModeIndex++;
                if (ModeIndex == Modes.Length) ModeIndex = 0;

                camera.VisibleLayers = Modes[ModeIndex];
            };
        }

        public void Dispose()
        {
            DrawColliderModel.Dispose();
        }
    }
}
