cbuffer ConstantBuffer : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
    float4 Light;
}

struct VS_IN
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD;
};

struct VS_OUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD;
};

VS_OUT main(VS_IN input)
{
    VS_OUT output;

    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);

    if (!any(Light))
    {
        output.Color = input.Color;
    }
    else
    {
        float3 normal = mul(float4(input.Normal.xyz, 0), World).xyz;
        normal = normalize(normal);

        float light = saturate(dot(normal, -(float3)Light));
        light = light * 0.2f + 0.8f;
        output.Color = float4(input.Color.r * light, input.Color.g * light, input.Color.b * light, input.Color.a);
    }

    output.TexCoord = input.TexCoord;

    return output;
}
