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
    float2 d = TexelSize * BloomScatter;

    float3 color = InputTexture.Sample(LinearSampler, uv).rgb * 0.25;

    color += InputTexture.Sample(LinearSampler, uv + float2(-d.x, -d.y)).rgb * 0.0625;
    color += InputTexture.Sample(LinearSampler, uv + float2(d.x, -d.y)).rgb * 0.0625;
    color += InputTexture.Sample(LinearSampler, uv + float2(-d.x, d.y)).rgb * 0.0625;
    color += InputTexture.Sample(LinearSampler, uv + float2(d.x, d.y)).rgb * 0.0625;

    color += InputTexture.Sample(LinearSampler, uv + float2(0.0, -d.y)).rgb * 0.125;
    color += InputTexture.Sample(LinearSampler, uv + float2(0.0, d.y)).rgb * 0.125;
    color += InputTexture.Sample(LinearSampler, uv + float2(-d.x, 0.0)).rgb * 0.125;
    color += InputTexture.Sample(LinearSampler, uv + float2(d.x, 0.0)).rgb * 0.125;

    return float4(color, 1.0);
}