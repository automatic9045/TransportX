using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Diagnostics;
using TransportX.Rendering;

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

            EnvironmentProfile environment = new()
            {
                DiffuseTexture = LoadTexture(data.DiffuseTexturePath.Value, "拡散光マップファイル"),
                SpecularTexture = LoadTexture(data.SpecularTexturePath.Value, "反射光マップファイル"),
                Intensity = data.Intensity.Value,
                Saturation = data.Saturation.Value,
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

            World.DefaultEnvironmentKey = environment;
        }
    }
}
