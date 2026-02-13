using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Assimp;

namespace TransportX.Rendering.Importing
{
    internal static class AssimpExtensions
    {
        private static bool TryGetValue<T>(this Assimp.Material material, string baseName, out T value) where T : unmanaged
        {
            if (material.GetNonTextureProperty(baseName) is MaterialProperty property)
            {
                if (typeof(T) == typeof(int))
                {
                    int baseValue = property.GetIntegerValue();
                    value = Unsafe.As<int, T>(ref baseValue);
                }
                else if (typeof(T) == typeof(float))
                {
                    float baseValue = property.GetFloatValue();
                    value = Unsafe.As<float, T>(ref baseValue);
                }
                else if (typeof(T) == typeof(Vector4))
                {
                    Vector4 baseValue = property.GetVector4Value();
                    value = Unsafe.As<Vector4, T>(ref baseValue);
                }
                else
                {
                    throw new NotSupportedException();
                }

                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static bool TryGetBaseColor(this Assimp.Material material, out Vector4 value)
        {
            return material.TryGetValue("$clr.base", out value);
        }

        public static bool TryGetMetallicFactor(this Assimp.Material material, out float value)
        {
            return material.TryGetValue("$mat.metallicFactor", out value);
        }

        public static bool TryGetRoughnessFactor(this Assimp.Material material, out float value)
        {
            return material.TryGetValue("$mat.roughnessFactor", out value);
        }
    }
}
