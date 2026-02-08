using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Key = System.Windows.Input.Key;

using Bus.Common.Input;
using Bus.Common.Rendering;

namespace Bus.Common.Physics
{
    public class DebugInput : IDisposable
    {
        private static readonly Camera.VisualLayers[] Modes = [
            Camera.VisualLayers.Normal,
            Camera.VisualLayers.Normal | Camera.VisualLayers.Colliders,
            Camera.VisualLayers.Normal | Camera.VisualLayers.Network,
            Camera.VisualLayers.Normal | Camera.VisualLayers.Traffic,
        ];


        private readonly KeyObserver DrawColliderModel;
        private int ModeIndex = 0;

        public DebugInput(InputManager inputManager, Camera camera)
        {
            DrawColliderModel = inputManager.ObserveKey(Key.F6);
            DrawColliderModel.Pressed += (sender, e) =>
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
