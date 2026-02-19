Texture2D HDRTexture : register(t0);
Texture2D BloomTexture : register(t1);

SamplerState LinearSampler : register(s0);

cbuffer PostProcessBuffer : register(b0)
{
    float BloomThreshold;
    float BloomIntensity;
    float BloomScatter;
    float BloomSoftKnee;
    float3 BloomTint;
    float _Padding;
}

struct PS_IN
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

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
    float3 hdrColor = HDRTexture.Sample(LinearSampler, input.TexCoord).rgb;
    float3 bloomColor = BloomTexture.Sample(LinearSampler, input.TexCoord).rgb;

    hdrColor += bloomColor * BloomTint * BloomIntensity;

    float3 ldrColor = ACESFilm(hdrColor);

    return float4(ldrColor, 1.0);
}
