Texture2D AmbientTexture : register(t0);
Texture2D DirectionalTexture : register(t1);
Texture2D RawShadowTexture : register(t2);
SamplerState Sampler : register(s0);

struct PS_IN
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

float4 main(PS_IN input) : SV_Target0
{
    float2 uv = input.TexCoord;
    float width, height;
    RawShadowTexture.GetDimensions(width, height);
    float2 texelSize = float2(1.0 / float(width), 1.0 / float(height));

    float spatialBlur = 0.0;
    float totalWeight = 0.0;

    const float blurRadius = 1.0;

    /*const float weights[25] =
    {
        1, 4, 6, 4, 1,
        4, 16, 24, 16, 4,
        6, 24, 36, 24, 6,
        4, 16, 24, 16, 4,
        1, 4, 6, 4, 1
    };

    [unroll]
    for (int x = -2; x <= 2; ++x)
    {
        [unroll]
        for (int y = -2; y <= 2; ++y)
        {
            float2 offsetUV = uv + float2(x, y) * texelSize * blurRadius;
            float neighborShadow = RawShadowTexture.SampleLevel(Sampler, offsetUV, 0).r;

            float w = weights[(x + 2) + (y + 2) * 5];

            spatialBlur += neighborShadow * w;
            totalWeight += w;
        }
    }*/

    const float weights[9] = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };

    [unroll]
    for (int x = -1; x <= 1; ++x)
    {
        [unroll]
        for (int y = -1; y <= 1; ++y)
        {
            float2 offsetUV = uv + float2(x, y) * texelSize * blurRadius;
            float neighborShadow = RawShadowTexture.SampleLevel(Sampler, offsetUV, 0).r;
            float w = weights[(x + 1) + (y + 1) * 3];
            spatialBlur += neighborShadow * w;
            totalWeight += w;
        }
    }

    float currentShadow = spatialBlur / totalWeight;

    float3 ambient = AmbientTexture.Sample(Sampler, uv).rgb;
    float3 directional = DirectionalTexture.Sample(Sampler, uv).rgb;
    float3 finalColor = ambient + directional * currentShadow;

    return float4(finalColor, 1.0);
}
