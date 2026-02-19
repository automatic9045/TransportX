Texture2D InputTexture : register(t0);

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

float4 main(PS_IN input) : SV_TARGET
{
    float3 color = InputTexture.Sample(LinearSampler, input.TexCoord).rgb;

    float brightness = max(color.r, max(color.g, color.b));

    float knee = BloomThreshold * BloomSoftKnee + 0.0001;
    float soft = brightness - BloomThreshold + knee;
    soft = clamp(soft, 0.0, 2.0 * knee);
    soft = (soft * soft) / (4.0 * knee);

    float contribution = max(soft, brightness - BloomThreshold);
    contribution /= max(brightness, 0.00001);

    float3 brightColor = color * contribution;
    return float4(brightColor, 1.0);
}
