Texture2D HDRTexture : register(t0);
Texture2D RawShadowDepthTexture : register(t1);
SamplerState LinearSampler : register(s0);

struct PS_IN
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

float4 main(PS_IN input) : SV_TARGET
{
    float totalLuminanceLog = 0.0;
    int sampleCount = 0;
    
    for (float x = 0.0625; x < 1.0; x += 0.125)
    {
        for (float y = 0.0625; y < 1.0; y += 0.125)
        {
            float2 uv = float2(x, y);
            float depth = RawShadowDepthTexture.SampleLevel(LinearSampler, uv, 0).g;
            
            if (0.9999 <= depth)
                continue;
                
            float3 hdr = HDRTexture.SampleLevel(LinearSampler, uv, 0).rgb;
            float luminance = dot(hdr, float3(0.2126, 0.7152, 0.0722));
            totalLuminanceLog += log(max(luminance, 0.0001));
            sampleCount++;
        }
    }
    
    if (sampleCount == 0)
        return float4(0.0, 0.0, 0.0, 1.0);
    
    float averageLuminanceLog = totalLuminanceLog / float(sampleCount);
    return float4(averageLuminanceLog, 0.0, 0.0, 1.0);
}
