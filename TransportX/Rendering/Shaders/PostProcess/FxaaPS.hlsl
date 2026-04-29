Texture2D SourceTexture : register(t0);
SamplerState LinearSampler : register(s0);

struct PSInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

static const float FXAA_EDGE_THRESHOLD_MIN = 0.0312;
static const float FXAA_EDGE_THRESHOLD_MAX = 0.125;
static const int FXAA_ITERATIONS = 12;
static const float FXAA_SUBPIXEL_QUALITY = 0.75;

float RgbToLuma(float3 rgb)
{
    return dot(rgb, float3(0.299, 0.587, 0.114));
}

float4 main(PSInput input) : SV_Target
{
    uint width, height;
    SourceTexture.GetDimensions(width, height);
    float2 inverseScreenSize = float2(1.0 / (float) width, 1.0 / (float) height);

    float3 colorCenter = SourceTexture.Sample(LinearSampler, input.TexCoord).rgb;
    float lumaCenter = RgbToLuma(colorCenter);

    float lumaDown = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(0.0, 1.0) * inverseScreenSize, 0).rgb);
    float lumaUp = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(0.0, -1.0) * inverseScreenSize, 0).rgb);
    float lumaLeft = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(-1.0, 0.0) * inverseScreenSize, 0).rgb);
    float lumaRight = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(1.0, 0.0) * inverseScreenSize, 0).rgb);

    float lumaMin = min(lumaCenter, min(min(lumaDown, lumaUp), min(lumaLeft, lumaRight)));
    float lumaMax = max(lumaCenter, max(max(lumaDown, lumaUp), max(lumaLeft, lumaRight)));
    float lumaRange = lumaMax - lumaMin;

    if (lumaRange < max(FXAA_EDGE_THRESHOLD_MIN, lumaMax * FXAA_EDGE_THRESHOLD_MAX))
    {
        return float4(colorCenter, 1.0);
    }

    float lumaDownLeft = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(-1.0, 1.0) * inverseScreenSize, 0).rgb);
    float lumaUpRight = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(1.0, -1.0) * inverseScreenSize, 0).rgb);
    float lumaUpLeft = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(-1.0, -1.0) * inverseScreenSize, 0).rgb);
    float lumaDownRight = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, input.TexCoord + float2(1.0, 1.0) * inverseScreenSize, 0).rgb);

    float edgeHorizontal = abs(-2.0 * lumaLeft + lumaUpLeft + lumaDownLeft) +
                            abs(-2.0 * lumaCenter + lumaUp + lumaDown) * 2.0 +
                            abs(-2.0 * lumaRight + lumaUpRight + lumaDownRight);

    float edgeVertical = abs(-2.0 * lumaUp + lumaUpLeft + lumaUpRight) +
                            abs(-2.0 * lumaCenter + lumaLeft + lumaRight) * 2.0 +
                            abs(-2.0 * lumaDown + lumaDownLeft + lumaDownRight);

    bool isHorizontal = (edgeHorizontal >= edgeVertical);

    float stepLength = isHorizontal ? inverseScreenSize.y : inverseScreenSize.x;

    float luma1 = isHorizontal ? lumaDown : lumaLeft;
    float luma2 = isHorizontal ? lumaUp : lumaRight;
    float gradient1 = abs(luma1 - lumaCenter);
    float gradient2 = abs(luma2 - lumaCenter);

    bool is1Steepest = gradient1 >= gradient2;
    float gradientScaled = 0.25 * max(gradient1, gradient2);

    float stepSign = is1Steepest ? -1.0 : 1.0;
    float2 currentUv = input.TexCoord;
    if (isHorizontal)
        currentUv.y += stepSign * stepLength * 0.5;
    else
        currentUv.x += stepSign * stepLength * 0.5;

    float2 offset = isHorizontal ? float2(inverseScreenSize.x, 0.0) : float2(0.0, inverseScreenSize.y);
    float2 uv1 = currentUv - offset;
    float2 uv2 = currentUv + offset;

    float lumaLocalAverage = (lumaCenter + (is1Steepest ? luma1 : luma2)) * 0.5;

    bool reached1 = false;
    bool reached2 = false;
    float lumaEnd1 = 0.0;
    float lumaEnd2 = 0.0;

    for (int i = 0; i < FXAA_ITERATIONS; i++)
    {
        if (!reached1)
        {
            lumaEnd1 = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, uv1, 0).rgb);
            lumaEnd1 = lumaEnd1 - lumaLocalAverage;
        }
        if (!reached2)
        {
            lumaEnd2 = RgbToLuma(SourceTexture.SampleLevel(LinearSampler, uv2, 0).rgb);
            lumaEnd2 = lumaEnd2 - lumaLocalAverage;
        }

        reached1 = abs(lumaEnd1) >= gradientScaled;
        reached2 = abs(lumaEnd2) >= gradientScaled;

        if (reached1 && reached2)
            break;

        if (!reached1)
            uv1 -= offset;
        if (!reached2)
            uv2 += offset;
    }

    float distance1 = isHorizontal ? (input.TexCoord.x - uv1.x) : (input.TexCoord.y - uv1.y);
    float distance2 = isHorizontal ? (uv2.x - input.TexCoord.x) : (uv2.y - input.TexCoord.y);

    bool isDirection1 = distance1 < distance2;
    float distanceFinal = min(distance1, distance2);
    float edgeThickness = (distance1 + distance2);

    float pixelOffset = -distanceFinal / edgeThickness + 0.5;

    bool isLumaCenterSmaller = lumaCenter < lumaLocalAverage;
    bool correctVariation = ((isDirection1 ? lumaEnd1 : lumaEnd2) < 0.0) != isLumaCenterSmaller;
    float finalOffset = correctVariation ? pixelOffset : 0.0;

    float lumaAverage = (1.0 / 12.0) * (2.0 * (lumaDown + lumaUp + lumaLeft + lumaRight) + lumaDownLeft + lumaUpRight + lumaUpLeft + lumaDownRight);
    float subPixelOffset1 = clamp(abs(lumaAverage - lumaCenter) / lumaRange, 0.0, 1.0);
    float subPixelOffset2 = (-2.0 * subPixelOffset1 + 3.0) * subPixelOffset1 * subPixelOffset1;
    float subPixelOffsetFinal = subPixelOffset2 * subPixelOffset2 * FXAA_SUBPIXEL_QUALITY;

    finalOffset = max(finalOffset, subPixelOffsetFinal);
    float2 finalUv = input.TexCoord;
    if (isHorizontal)
        finalUv.y += finalOffset * stepLength * stepSign;
    else
        finalUv.x += finalOffset * stepLength * stepSign;

    float3 finalColor = SourceTexture.SampleLevel(LinearSampler, finalUv, 0).rgb;

    return float4(finalColor, 1.0);
}
