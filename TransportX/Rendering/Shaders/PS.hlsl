Texture2D BaseColorTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D ORMTexture : register(t2);
Texture2D EmissiveTexture : register(t3);

TextureCube DiffuseIBLTexture : register(t10);
TextureCube SpecularIBLTexture : register(t11);

Texture2D BrdfLutTexture : register(t100);

SamplerState TextureSampler : register(s0);
SamplerState BrdfSampler : register(s1);

cbuffer MaterialBuffer : register(b0)
{
    float4 BaseColor;
    float3 Emissive;
    float Roughness;
    float Metallic;
    int HasBaseTexture;
    int HasNormalTexture;
    int HasORMTexture;
    int HasEmissiveTexture;
    float3 _Padding1;
}

cbuffer EnvironmentBuffer : register(b1)
{
    float IBLIntensity;
    float IBLSaturation;
    float2 _Padding2;
}

cbuffer SceneBuffer : register(b2)
{
    float3 CameraPosition;
    float _Padding3;
    float3 LightColor;
    float _Padding4;
    float3 LightDirection;
    float LightIntensity;
}

struct PS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;

    float3 WorldPosition : WORLDPOS;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float2 TexCoord : TEXCOORD;
};

static const float PI = 3.14159265359;

float3 FresnelSchlick(float vDotH, float3 baseReflectivity)
{
    return baseReflectivity + (1.0 - baseReflectivity) * pow(max(1.0 - vDotH, 0.0), 5.0);
}

float3 FresnelSchlickRoughness(float nDotV, float3 baseReflectivity, float roughness)
{
    return baseReflectivity
        + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), baseReflectivity) - baseReflectivity)
        * pow(max(1.0 - nDotV, 0.0), 5.0);
}

float DistributionGGX(float3 normal, float3 h, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float nDotH = max(dot(normal, h), 0.0);
    float nDotH2 = nDotH * nDotH;

    float denominator = (nDotH2 * (a2 - 1.0) + 1.0);
    denominator = PI * denominator * denominator;

    return a2 / max(denominator, 0.0000001);
}

float GeometrySchlickGGX(float nDotX, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    return nDotX / max(nDotX * (1.0 - k) + k, 0.0000001);
}

float GeometrySmith(float nDotV, float nDotL, float roughness)
{
    float geometryView = GeometrySchlickGGX(nDotV, roughness);
    float geometryLight = GeometrySchlickGGX(nDotL, roughness);

    return geometryLight * geometryView;
}

float3 ACESFilm(float3 x)
{
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
}

float4 main(PS_IN input) : SV_TARGET
{
    float4 baseColor = BaseColor * input.Color;
    if (HasBaseTexture)
    {
        baseColor *= BaseColorTexture.Sample(TextureSampler, input.TexCoord);
    }

    float3 normal = normalize(input.Normal);
    if (HasNormalTexture)
    {
        float3 tangentNormal = NormalTexture.Sample(TextureSampler, input.TexCoord).xyz * 2.0 - 1.0;

        float3 tangent = normalize(input.Tangent);
        tangent = normalize(tangent - dot(tangent, normal) * normal);

        float3 binormal = cross(normal, tangent);

        float3x3 tbn = float3x3(tangent, binormal, normal);
        normal = normalize(mul(tangentNormal, tbn));
    }

    float occlusion = 1.0;
    float roughness = Roughness;
    float metallic = Metallic;

    if (HasORMTexture)
    {
        float4 orm = ORMTexture.Sample(TextureSampler, input.TexCoord);
        occlusion = orm.r;
        roughness *= orm.g;
        metallic *= orm.b;
    }

    float3 emissive = Emissive;
    if (HasEmissiveTexture)
    {
        emissive *= EmissiveTexture.Sample(TextureSampler, input.TexCoord).rgb;
    }

    float3 v = normalize(CameraPosition - input.WorldPosition); // View
    float3 l = normalize(-LightDirection); // Light
    float3 h = normalize(v + l); // Half
    float3 r = reflect(-v, normal); // Reflection (for IBL)

    float nDotV = max(dot(normal, v), 0.0);
    float nDotL = max(dot(normal, l), 0.0);
    float vDotH = max(dot(v, h), 0.0);

    float3 baseReflectivity = float3(0.04, 0.04, 0.04);
    baseReflectivity = lerp(baseReflectivity, baseColor.rgb, metallic);

    float d = DistributionGGX(normal, h, roughness);
    float g = GeometrySmith(nDotV, nDotL, roughness);
    float3 f = FresnelSchlick(vDotH, baseReflectivity);

    float3 specular = d * g * f / (4.0 * nDotV * nDotL + 0.0001);

    float3 specularRatio = f;
    float3 diffuseRatio = (float3(1.0, 1.0, 1.0) - specularRatio) * (1.0 - metallic);

    float3 radianceOut = (diffuseRatio * baseColor.rgb / PI + specular) * LightColor * LightIntensity * nDotL;

    float3 diffuseIBLRatio = (1.0 - FresnelSchlickRoughness(nDotV, baseReflectivity, roughness)) * (1.0 - metallic);

    float3 irradiance = DiffuseIBLTexture.Sample(TextureSampler, normal).rgb;

    float3 grayIBL = dot(irradiance, float3(0.299, 0.587, 0.114));
    irradiance = lerp(grayIBL, irradiance, IBLSaturation);

    float3 diffuseIBL = irradiance * baseColor.rgb;

    float MAX_REFLECTION_LOD = 5.0;
    float3 prefilteredColor = SpecularIBLTexture.SampleLevel(TextureSampler, r, roughness * MAX_REFLECTION_LOD).rgb;
    float2 envBRDF = BrdfLutTexture.Sample(BrdfSampler, float2(nDotV, roughness)).rg;
    float3 specularIBL = prefilteredColor * (baseReflectivity * envBRDF.x + envBRDF.y);
    specularIBL *= pow(saturate(1.0 - roughness * 0.625), 2.0);

    float3 ambient = (diffuseIBLRatio * diffuseIBL + specularIBL) * occlusion * IBLIntensity;

    float3 color = ambient + radianceOut + emissive;
    color = ACESFilm(color);

    return float4(color, baseColor.a);
}
