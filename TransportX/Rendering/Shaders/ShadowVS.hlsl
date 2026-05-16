cbuffer ShadowConstantBuffer : register(b1)
{
    float4x4 LightViewProjection;
};

struct VSInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
    float2 TexCoord : TEXCOORD;

    float4x4 World : WORLD;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
};

VSOutput main(VSInput input)
{
    VSOutput output;
    float4 worldPosition = mul(input.Position, input.World);
    output.Position = mul(worldPosition, LightViewProjection);
    return output;
}
