cbuffer TransformBuffer : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
}

struct VS_IN
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
    float2 TexCoord : TEXCOORD;
};

struct VS_OUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;

    float3 WorldPosition : WORLDPOS;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float2 TexCoord : TEXCOORD;
};

VS_OUT main(VS_IN input)
{
    VS_OUT output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, View);
    output.Position = mul(output.Position, Projection);

    output.WorldPosition = worldPosition.xyz;

    output.Normal = normalize(mul(input.Normal.xyz, (float3x3) World));
    output.Tangent = normalize(mul(input.Tangent.xyz, (float3x3) World));

    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    return output;
}
