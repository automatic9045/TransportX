using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;
using Bus.Common.Worlds;

namespace Bus.Models
{
    internal class RendererLoader
    {
        private readonly IDXHost DXHost;

        public RendererLoader(IDXHost dxHost)
        {
            DXHost = dxHost;
        }

        public IRenderer Load(IWorldInfo worldInfo)
        {
            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(worldInfo.RendererPath);
            Type[] types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IRenderer).IsAssignableFrom(t))
                .ToArray();

            switch (types.Length)
            {
                case 0:
                {
                    string fileName = Path.GetFileName(worldInfo.RendererPath);
                    throw new ArgumentException($"{fileName} にはレンダラーが定義されていません。", nameof(worldInfo));
                }

                case 1:
                    break;

                default:
                {
                    string fileName = Path.GetFileName(worldInfo.RendererPath);
                    throw new ArgumentException($"{fileName} には 2 つ以上のレンダラーが定義されています。", nameof(worldInfo));
                }
            }

            Type type = types[0];
            ConstructorInfo constructor = type.GetConstructor([typeof(IDXHost), typeof(IWorldInfo)])
                ?? throw new ArgumentException($"{type.Name} にはパラメータが {nameof(IDXHost)}、{nameof(IWorldInfo)} のコンストラクタが定義されていません。", nameof(worldInfo));

            IRenderer renderer = (IRenderer)constructor.Invoke([DXHost, worldInfo]);
            return renderer;
        }
    }
}
