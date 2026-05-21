Texture2D AmbientTex : register(t0);
Texture2D DirectionalTex : register(t1);
Texture2D RawShadowTex : register(t2);
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
    RawShadowTex.GetDimensions(width, height);
    float2 texelSize = float2(1.0 / float(width), 1.0 / float(height));

    float2 rawData = RawShadowTex.SampleLevel(Sampler, uv, 0).rg;
    float centerShadow = rawData.r;
    float cascadeIndex = rawData.g;

    float spatialBlur = 0.0;
    float totalWeight = 0.0;

    const float weights[25] =
    {
        1, 4, 6, 4, 1,
        4, 16, 24, 16, 4,
        6, 24, 36, 24, 6,
        4, 16, 24, 16, 4,
        1, 4, 6, 4, 1
    };

    float blurRadius = 1.0;

    [unroll]
    for (int x = -2; x <= 2; ++x)
    {
        [unroll]
        for (int y = -2; y <= 2; ++y)
        {
            float2 offsetUV = uv + float2(x, y) * texelSize * blurRadius;
            float neighborShadow = RawShadowTex.SampleLevel(Sampler, offsetUV, 0).r;

            float w = weights[(x + 2) + (y + 2) * 5];

            spatialBlur += neighborShadow * w;
            totalWeight += w;
        }
    }

    float currentShadow = spatialBlur / totalWeight;

    float3 ambient = AmbientTex.Sample(Sampler, uv).rgb;
    float3 directional = DirectionalTex.Sample(Sampler, uv).rgb;
    float3 finalColor = ambient + directional * currentShadow;

    return float4(finalColor, 1.0);
}
