using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Rendering.Backend;

using TransportX.Scripting;
using TransportX.Scripting.Collections;

using TransportX.Domains.Equipment.Cameras;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class CamerasBase : IComponentCommand
    {
        internal IGraphicsHost GraphicsHost { get; }
        internal IErrorCollector ErrorCollector { get; }

        public CameraCollectionComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        private readonly ScriptKeyedList<string, SceneCaptureCameraCommand> SceneCaptureKey;
        public IReadOnlyScriptKeyedList<string, SceneCaptureCameraCommand> SceneCapture => SceneCaptureKey;

        protected CamerasBase(IGraphicsHost graphicsHost, IErrorCollector errorCollector)
        {
            GraphicsHost = graphicsHost;
            ErrorCollector = errorCollector;

            Source = new CameraCollectionComponent();

            SceneCaptureKey = new ScriptKeyedList<string, SceneCaptureCameraCommand>(
                camera => camera.Key, ErrorCollector, "シーンキャプチャカメラ", key => SceneCaptureCameraCommand.Empty(ErrorCollector, key));
        }

        public void AddSceneCapture(SceneCaptureCameraCommand camera)
        {
            SceneCaptureKey.Add(camera);
            if (camera.Source is not null) Source.SceneCapture.Add(camera.Source);
        }
    }
}
