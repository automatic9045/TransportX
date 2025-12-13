Texture2D diffuseTexture;
SamplerState SampleType;

cbuffer ConstantBuffer : register(b0)
{
    float4 BaseColor;
    int HasTexture;
    float3 _Padding;
}

struct PS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD;
};

float4 main(PS_IN input) : SV_TARGET
{
    float4 textureColor = HasTexture != 0 ? diffuseTexture.Sample(SampleType, input.TexCoord) : float4(1, 1, 1, 1);
    return textureColor * BaseColor * input.Color;
}
