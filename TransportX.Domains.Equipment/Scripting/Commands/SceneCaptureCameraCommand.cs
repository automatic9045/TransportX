using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

using TransportX.Domains.Equipment.Cameras;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class SceneCaptureCameraCommand
    {
        public static SceneCaptureCameraCommand Empty(IErrorCollector errorCollector, string key) => new(errorCollector, key, null);


        protected readonly IErrorCollector ErrorCollector;

        public string Key { get; }
        public ISceneCaptureCamera? Source { get; }

        public SceneCaptureCameraCommand(IErrorCollector errorCollector, string key, ISceneCaptureCamera? source)
        {
            ErrorCollector = errorCollector;
            Key = key;
            Source = source;
        }
    }
}
