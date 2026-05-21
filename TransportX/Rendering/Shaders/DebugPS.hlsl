cbuffer MaterialBuffer : register(b0)
{
    float4 BaseColor;
}

struct PS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

struct PS_OUT
{
    float4 Ambient : SV_Target0;
    float4 Velocity : SV_Target1;
    float4 Directional : SV_Target2;
    float4 RawShadow : SV_Target3;
};

PS_OUT main(PS_IN input)
{
    PS_OUT output;
    output.Ambient = BaseColor * input.Color;
    output.Velocity = float4(0.0, 0.0, 0.0, 0.0);
    output.Directional = float4(0.0, 0.0, 0.0, 0.0);
    output.RawShadow = float4(1.0, 0.0, 0.0, 0.0);
    return output;
}
