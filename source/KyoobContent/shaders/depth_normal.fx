float4x4 world;
float4x4 view;
float4x4 projection;

// vertex shader input
struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
};

// vertex shader output
struct VSOutput
{
    float4 Position : POSITION0;
    float2 Depth    : TEXCOORD0;
    float3 Normal   : TEXCOORD1;
};

// pixel shader output
struct PSOutput
{
    float4 Normal : COLOR0;
    float4 Depth  : COLOR1;
};

// vertex shader function
VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    float4 worldPosition = mul( input.Position, world );
    float4 viewPosition = mul( worldPosition, view );
    
    output.Position = mul( viewPosition, projection );
    output.Normal = mul( input.Normal, world );
    output.Depth.xy = output.Position.zw; // .xy needed?

    return output;
}

// pixel shader function
PSOutput PSFunc( VSOutput input )
{
    PSOutput output;

    // calculate depth as distance from camera / far plane distance
    output.Depth = input.Depth.x / input.Depth.y;

    // shift normal values from [-1, 1] to [0, 1]
    output.Normal.xyz = ( normalize( input.Normal ).xyz * 0.5 ) + 0.5;

    output.Depth.a = 1;
    output.Normal.a = 1;
    return output;
}

technique MainTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 VSFunc();
        PixelShader  = compile ps_2_0 PSFunc();
    }
}