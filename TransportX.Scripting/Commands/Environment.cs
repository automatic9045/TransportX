using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Diagnostics;
using TransportX.Environment;
using TransportX.Rendering;

using TransportX.Extensions.Utilities;

namespace TransportX.Scripting.Commands
{
    public class Environment
    {
        protected readonly ScriptWorld World;
        private readonly DDSTextureFactory DDSFactory;

        private protected Environment(ScriptWorld world)
        {
            World = world;
            DDSFactory = new DDSTextureFactory(World.DXHost.Device);
        }

        protected EnvironmentProfile? CreateEnvironment(string path)
        {
            string fullPath = Path.Combine(World.BaseDirectory, path);
            Data.Environment.EnvironmentProfile? data = Data.XmlSerializer<Data.Environment.EnvironmentProfile>.FromXml(fullPath, World.ErrorCollector);
            if (data is null) return null;

            World.ErrorCollector.ReportRange(data.Errors);

            EnvironmentProfile environment = new()
            {
                IBL = new IBL()
                {
                    DiffuseTexture = LoadTexture(data.IBL.DiffuseTexturePath.Value, "拡散光マップファイル"),
                    SpecularTexture = LoadTexture(data.IBL.SpecularTexturePath.Value, "反射光マップファイル"),
                    Intensity = data.IBL.Intensity.Value,
                    Saturation = data.IBL.Saturation.Value,
                },
                Bloom = new Bloom()
                {
                    Threshold = data.Bloom.Threshold.Value,
                    Intensity = data.Bloom.Intensity.Value,
                    Scatter = data.Bloom.Scatter.Value,
                    SoftKnee = data.Bloom.SoftKnee.Value,
                    Tint = data.Bloom.Tint.Value.ToVector3(),
                },
            };

            return environment;


            ID3D11ShaderResourceView? LoadTexture(string? texturePath, string nameForMessage)
            {
                if (texturePath is null) return null;

                string textureFullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fullPath)!, texturePath));
                if (!File.Exists(textureFullPath))
                {
                    ScriptError error = new(ErrorLevel.Error, $"{nameForMessage} '{textureFullPath}' が見つかりませんでした。");
                    World.ErrorCollector.Report(error);
                    return null;
                }

                try
                {
                    return DDSFactory.CreateFromFile(textureFullPath);
                }
                catch (Exception ex)
                {
                    ScriptError error = new(ErrorLevel.Error, ex, $"{nameForMessage} '{textureFullPath}' を読み込めませんでした。");
                    World.ErrorCollector.Report(error);
                    return null;
                }
            }
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

            World.DefaultEnvironment?.Dispose();
            World.SetDefaultEnvironment(environment);
        }
    }
}
