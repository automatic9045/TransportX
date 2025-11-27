cbuffer ConstantBuffer : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
}

struct VS_IN
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD;
    float4 Color : COLOR0;
};

struct VS_OUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
    float4 Color : COLOR0;
};

VS_OUT main(VS_IN input)
{
    VS_OUT output;

    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;

    return output;
}
