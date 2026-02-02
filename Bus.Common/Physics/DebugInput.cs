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
                camera.DebugMode = camera.DebugMode switch
                {
                    DebugRenderingMode.None => DebugRenderingMode.Colliders,
                    DebugRenderingMode.Colliders => DebugRenderingMode.Network,
                    DebugRenderingMode.Network => DebugRenderingMode.None,
                    _ => throw new NotSupportedException(),
                };
            };
        }

        public void Dispose()
        {
            DrawColliderModel.Dispose();
        }
    }
}
