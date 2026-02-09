using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    internal class ShaderFactory
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        public static Blob CompileFromResource(ID3D11Device device, string fileName, string entryPoint, string sourceName, string profile)
        {
            using (Stream s = Assembly.GetManifestResourceStream(typeof(ShaderFactory), $"Shaders.{fileName}")!)
            using (StreamReader sr = new StreamReader(s))
            {
                string source = sr.ReadToEnd();

                Result result = Compiler.Compile(source, entryPoint, sourceName, profile, out Blob blob, out _);
                if (result.Failure)
                {
                    throw new Exception(result.Description);
                }

                return blob;
            }
        }
    }
}
