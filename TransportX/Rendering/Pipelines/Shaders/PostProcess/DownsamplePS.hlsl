Texture2D InputTexture : register(t0);

SamplerState LinearSampler : register(s0);

cbuffer BlurBuffer : register(b1)
{
    float2 TexelSize;
    float BloomScatter;
    float Padding;
};

struct PS_IN
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

float4 main(PS_IN input) : SV_TARGET
{
    float2 uv = input.TexCoord;
    float2 d = TexelSize;

    float3 color = 0.0;
    color += InputTexture.Sample(LinearSampler, uv + float2(-d.x, -d.y)).rgb;
    color += InputTexture.Sample(LinearSampler, uv + float2(d.x, -d.y)).rgb;
    color += InputTexture.Sample(LinearSampler, uv + float2(-d.x, d.y)).rgb;
    color += InputTexture.Sample(LinearSampler, uv + float2(d.x, d.y)).rgb;

    return float4(color * 0.25, 1.0);
}
