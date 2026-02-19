Texture2D HDRTexture : register(t0);
SamplerState PointSampler : register(s0);

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
    float3 hdrColor = HDRTexture.Sample(PointSampler, input.TexCoord).rgb;
    float3 ldrColor = ACESFilm(hdrColor);
    return float4(ldrColor, 1.0);
}
