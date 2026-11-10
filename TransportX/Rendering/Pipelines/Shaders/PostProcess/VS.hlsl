struct VS_OUT
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD;
};

VS_OUT main(uint id : SV_VertexID)
{
    VS_OUT output;
    output.UV = float2((id << 1) & 2, id & 2);
    output.Position = float4(output.UV * float2(2, -2) + float2(-1, 1), 0, 1);
    return output;
}
