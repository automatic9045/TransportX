cbuffer MaterialBuffer : register(b0)
{
    float4 BaseColor;
}

struct PS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

float4 main(PS_IN input) : SV_TARGET
{
    return BaseColor * input.Color;
}
