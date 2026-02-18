using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        public static Stream? GetShaderStream(string name)
        {
            return Assembly.GetManifestResourceStream(typeof(ShaderFactory), $"Shaders.{name}");
        }

        public static Blob CompileFromResource(ID3D11Device device, string fileName, string entryPoint, string sourceName, string profile)
        {
            using Stream s = GetShaderStream(fileName)!;
            using StreamReader sr = new(s);

            string source = sr.ReadToEnd();

            Result result = Compiler.Compile(source, entryPoint, sourceName, profile, out Blob? blob, out Blob? errorBlob);
            if (result.Failure)
            {
                if (errorBlob is null)
                {
                    throw new Exception(result.Description);
                }
                else
                {
                    string errorMessage = Marshal.PtrToStringAnsi(errorBlob.BufferPointer)!;
                    throw new Exception(errorMessage);
                }
            }

            return blob;
        }
    }
}
