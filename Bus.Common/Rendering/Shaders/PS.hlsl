Texture2D diffuseTexture;
SamplerState SampleType;

struct PS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD;
};

float4 main(PS_IN input) : SV_TARGET
{
    float4 textureColor = diffuseTexture.Sample(SampleType, input.TexCoord);
    return textureColor * input.Color;
}
