using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Data;
using TransportX.Environment;
using TransportX.Rendering;

using TransportX.Extensions.Utilities;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Environment
    {
        protected readonly ScriptWorld World;

        private protected Environment(ScriptWorld world)
        {
            World = world;
        }

        protected EnvironmentProfile? CreateEnvironment(string path)
        {
            string fullPath = Path.Combine(World.BaseDirectory, path);
            Data.Environment.EnvironmentProfile? data = XmlSerializer<Data.Environment.EnvironmentProfile>.FromXml(fullPath, World.ErrorCollector);
            if (data is null) return null;

            World.ErrorCollector.ReportRange(data.Errors);

            EnvironmentProfile environment = new()
            {
                IBL = new IBLProfile()
                {
                    Intensity = data.IBL.Intensity.Value,
                    Saturation = data.IBL.Saturation.Value,
                },
                Bloom = new BloomProfile()
                {
                    Threshold = data.Bloom.Threshold.Value,
                    Intensity = data.Bloom.Intensity.Value,
                    Scatter = data.Bloom.Scatter.Value,
                    SoftKnee = data.Bloom.SoftKnee.Value,
                    Tint = data.Bloom.Tint.Value.ToVector3(),
                },
                ToneMap = new ToneMapProfile()
                {
                    Contrast = data.ToneMap.Contrast.Value,
                    Shoulder = data.ToneMap.Shoulder.Value,
                    MaxLuminance = data.ToneMap.MaxLuminance.Value,
                    MidtoneScale = data.ToneMap.MidtoneScale.Value,
                },
                Exposure = new ExposureProfile()
                {
                    Key = data.Exposure.Key.Value,
                    Min = data.Exposure.Min.Value,
                    Max = data.Exposure.Max.Value,
                    DarkAdaptationSpeed = data.Exposure.DarkAdaptationSpeed.Value,
                    LightAdaptationSpeed = data.Exposure.LightAdaptationSpeed.Value,
                },
            };

            return environment;
        }
    }


    public class WorldEnvironment : Environment
    {
        internal WorldEnvironment(ScriptWorld world) : base(world)
        {
        }

        public void SetDefault(string path)
        {
            EnvironmentProfile? environment = CreateEnvironment(path);
            if (environment is null) return;

            World.SetDefaultEnvironment(environment);
        }
    }
}
