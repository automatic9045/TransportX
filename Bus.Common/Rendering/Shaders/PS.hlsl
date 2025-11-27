Texture2D diffuseTexture;
SamplerState SampleType;

cbuffer ConstantBuffer : register(b0)
{
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
    if (HasTexture != 0)
    {
        float4 textureColor = diffuseTexture.Sample(SampleType, input.TexCoord);
        return textureColor * input.Color;
    }
    else
    {
        return input.Color;
    }
}
