using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialConstants
    {
        internal static readonly int Size = Marshal.SizeOf<MaterialConstants>();


        public Vector4 BaseColor = Vector4.One;
        public Vector3 Emissive = Vector3.Zero;
        public float Roughness = 1;
        public float Metallic = 0;
        public int HasBaseTexture = 0;
        public int HasNormalTexture = 0;
        public int HasORMTexture = 0;
        public int HasEmissiveTexture = 0;
        public Vector3 Padding;

        public MaterialConstants()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct EnvironmentConstants
    {
        internal static readonly int Size = Marshal.SizeOf<EnvironmentConstants>();


        public float IBLIntensity = 1;
        public float IBLSaturation = 1;
        public Vector2 Padding;

        public EnvironmentConstants()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct SceneConstants
    {
        internal static readonly int Size = Marshal.SizeOf<SceneConstants>();


        public Matrix4x4 ViewProjection;
        public Vector3 CameraPosition = Vector3.Zero;
        public float Padding1;
        public Vector3 LightColor = Vector3.Zero;
        public float Padding2;
        public Vector3 LightDirection = Vector3.Zero;
        public float LightIntensity = 0;

        public SceneConstants()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct ShadowConstants
    {
        internal static readonly int Size = Marshal.SizeOf<ShadowConstants>();


        public Matrix4x4 LightViewProjection;

        public ShadowConstants()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CSMSamplingConstants
    {
        internal static readonly int Size = Marshal.SizeOf<CSMSamplingConstants>();


        public Matrix4x4 LightViewProjection0;
        public Matrix4x4 LightViewProjection1;
        public Matrix4x4 LightViewProjection2;
        public Matrix4x4 LightViewProjection3;
        public Vector4 SplitDepths;
        public float Resolution;
        public float ZPullback;
        public Vector2 Padding;

        public CSMSamplingConstants()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PostProcessConstants
    {
        internal static readonly int Size = Marshal.SizeOf<PostProcessConstants>();


        public float BloomThreshold;
        public float BloomIntensity;
        public float BloomScatter;
        public float BloomSoftKnee;
        public Vector3 BloomTint;
        public float Exposure;
        public float ToneMapA;
        public float ToneMapD;
        public float ToneMapB;
        public float ToneMapC;

        public PostProcessConstants()
        {
        }
    }
}
