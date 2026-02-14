Texture2D BaseColorTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D ORMTexture : register(t2);
Texture2D EmissiveTexture : register(t3);

SamplerState SampleType : register(s0);

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
    float3 _Padding;
}

cbuffer SceneBuffer : register(b1)
{
    float3 ToLight;
    float _Padding1;
    float3 CameraPosition;
    float _Padding2;
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
        baseColor *= BaseColorTexture.Sample(SampleType, input.TexCoord);
    }

    float3 normal = normalize(input.Normal);
    if (HasNormalTexture)
    {
        float3 tangentNormal = NormalTexture.Sample(SampleType, input.TexCoord).xyz * 2.0 - 1.0;

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
        float4 orm = ORMTexture.Sample(SampleType, input.TexCoord);
        occlusion = orm.r;
        roughness *= orm.g;
        metallic *= orm.b;
    }

    float3 emissive = Emissive;
    if (HasEmissiveTexture)
    {
        emissive *= EmissiveTexture.Sample(SampleType, input.TexCoord).rgb;
    }

    float3 v = normalize(CameraPosition - input.WorldPosition); // View
    float3 l = normalize(ToLight); // Light
    float3 h = normalize(v + l); // Half

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

    float3 lightColor = float3(1.0, 1.0, 1.0) * 3;
    float3 radianceOut = (diffuseRatio * baseColor.rgb / PI + specular) * lightColor * nDotL;

    float3 skyColor = float3(0.6, 0.7, 0.8) * 0.3;
    float3 groundColor = float3(0.2, 0.2, 0.2) * 0.3;
    float3 envLight = lerp(groundColor, skyColor, normal.y * 0.5 + 0.5);
    float3 ambientDiffuse = envLight * baseColor.rgb * (1.0 - metallic);
    float3 ambientSpecular = envLight * baseColor.rgb * metallic;

    float3 ambient = (ambientDiffuse + ambientSpecular) * occlusion;

    float3 color = ambient + radianceOut + emissive;
    color = ACESFilm(color);

    return float4(color, baseColor.a);
}
