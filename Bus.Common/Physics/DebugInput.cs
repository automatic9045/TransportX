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
        private readonly KeyObserver DrawColliderModel;

        public DebugInput(InputManager inputManager, Camera camera)
        {
            DrawColliderModel = inputManager.ObserveKey(Key.F6);
            DrawColliderModel.Pressed += (sender, e) =>
            {
                camera.DrawColliderDebugModel = !camera.DrawColliderDebugModel;
            };
        }

        public void Dispose()
        {
            DrawColliderModel.Dispose();
        }
    }
}
