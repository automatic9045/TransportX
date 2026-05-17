Texture2D BaseColorTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D ORMTexture : register(t2);
Texture2D EmissiveTexture : register(t3);

TextureCube DiffuseIBLTexture : register(t10);
TextureCube SpecularIBLTexture : register(t11);

Texture2DArray ShadowMapTexture : register(t12);

Texture2D BrdfLutTexture : register(t100);

SamplerState TextureSampler : register(s0);
SamplerState BrdfSampler : register(s1);
SamplerComparisonState ShadowSampler : register(s2);

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
    float4x4 View;
    float4x4 Projection;
    float3 CameraPosition;
    float _Padding3;
    float3 LightColor;
    float _Padding4;
    float3 LightDirection;
    float LightIntensity;
}

cbuffer CSMSamplingBuffer : register(b3)
{
    float4x4 LightViewProjection0;
    float4x4 LightViewProjection1;
    float4x4 LightViewProjection2;
    float4x4 LightViewProjection3;
    float4 SplitDepths;
    float Resolution;
    float3 _Padding5;

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

float InterleavedGradientNoise(float2 positionScreen)
{
    float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
    return frac(magic.z * frac(dot(positionScreen, magic.xy)));
}

static const float2 PoissonDisk[16] =
{
    float2(-0.94201624, -0.39906216), float2(0.94558609, -0.76890725),
    float2(-0.094184101, -0.92938870), float2(0.34495938, 0.29387760),
    float2(-0.91588581, 0.45771432), float2(-0.81544232, -0.87912464),
    float2(-0.38277543, 0.27676845), float2(0.97484398, 0.75648379),
    float2(0.44323325, -0.97511554), float2(0.53742981, -0.47373420),
    float2(-0.26496911, -0.41893023), float2(0.79197514, 0.19090188),
    float2(-0.24188840, 0.99706507), float2(-0.81409955, 0.91437590),
    float2(0.19984126, 0.78641367), float2(0.14383161, -0.14100467)
};

float CalculateShadow(float3 worldPos, float3 geometricNormal, float3 lightDir, float viewDistance, float2 screenPos)
{
    float3 l = normalize(-lightDir);
    float nDotL = dot(geometricNormal, l);

    if (nDotL <= 0.0)
    {
        return 0.0;
    }

    int cascadeIndex = 0;
    if (SplitDepths.x < viewDistance)
        cascadeIndex = 1;
    if (SplitDepths.y < viewDistance)
        cascadeIndex = 2;

    float radius = 100.0;
    if (cascadeIndex == 1)
        radius = 250.0;
    if (cascadeIndex == 2)
        radius = 1000.0;
    float texelSizeWorld = (radius * 2.0) / Resolution;

    float pcfSpread = 1.75;
    if (cascadeIndex == 1)
        pcfSpread *= 1.5;
    if (cascadeIndex == 2)
        pcfSpread *= 2.5;

    float clampedNDotL = max(nDotL, 0.0001);
    float sinTheta = sqrt(1.0 - clampedNDotL * clampedNDotL);
    float tanTheta = sinTheta / clampedNDotL;

    float maxPcfDistance = texelSizeWorld * pcfSpread;
    float normalOffset = maxPcfDistance * sinTheta;
    float3 biasedWorldPos = worldPos + (geometricNormal * normalOffset);

    float4x4 lightViewProj;
    if (cascadeIndex == 0)
        lightViewProj = LightViewProjection0;
    else if (cascadeIndex == 1)
        lightViewProj = LightViewProjection1;
    else
        lightViewProj = LightViewProjection2;

    float4 lightSpacePos = mul(float4(biasedWorldPos, 1.0), lightViewProj);
    float3 projCoords = lightSpacePos.xyz / lightSpacePos.w;

    projCoords.x = projCoords.x * 0.5 + 0.5;
    projCoords.y = projCoords.y * -0.5 + 0.5;

    if (projCoords.x < 0.0 || 1.0 < projCoords.x || projCoords.y < 0.0 || 1.0 < projCoords.y || projCoords.z < 0.0 || 1.0 < projCoords.z)
    {
        return 1.0;
    }

    float currentDepth = projCoords.z;
    float shadow = 0.0;
    float texelSize = 1.0 / Resolution;

    float zBiasWorld = texelSizeWorld * (0.5 + pcfSpread * tanTheta);
    zBiasWorld = min(zBiasWorld, 5.0);

    float zFar = 1500.0 + radius;
    float depthBias = zBiasWorld / zFar;

    float noise = InterleavedGradientNoise(screenPos);
    float angle = noise * PI * 2.0;
    float s, c;
    sincos(angle, s, c);
    float2x2 rotation = float2x2(c, -s, s, c);

    for (int i = 0; i < 16; ++i)
    {
        float2 offset = mul(PoissonDisk[i], rotation) * texelSize * pcfSpread;
        float3 uv = float3(projCoords.xy + offset, cascadeIndex);

        shadow += ShadowMapTexture.SampleCmpLevelZero(ShadowSampler, uv, currentDepth - depthBias);
    }
    shadow /= 16.0;

    return shadow;
}

float4 main(PS_IN input) : SV_TARGET
{
    float4 baseColor = BaseColor * input.Color;
    if (HasBaseTexture)
    {
        baseColor *= BaseColorTexture.Sample(TextureSampler, input.TexCoord);
    }

    float3 geometricNormal = normalize(input.Normal);
    float3 normal = geometricNormal;
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

    float viewDistance = distance(input.WorldPosition, CameraPosition);
    float shadowFactor = CalculateShadow(input.WorldPosition, geometricNormal, LightDirection, viewDistance, input.Position.xy);

    float3 radianceOut = (diffuseRatio * baseColor.rgb / PI + specular) * LightColor * LightIntensity * nDotL * shadowFactor;

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
    return float4(color, baseColor.a);
}
