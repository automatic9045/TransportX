using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Sample
{
    internal class ModelFactory
    {
        private static readonly string BaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Models");


        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext Context;

        public ModelFactory(ID3D11Device device, ID3D11DeviceContext context)
        {
            Device = device;
            Context = context;
        }

        public Model FromFile(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            Model model = Model.FromFile(Device, Context, path);
            return model;
        }
    }
}
