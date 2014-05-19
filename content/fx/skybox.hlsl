float4x4    _world;
float4x4    _view;
float4x4    _projection;

textureCUBE _cubeMap;
samplerCUBE _cubeMapSampler = sampler_state
{
    texture = <_cubeMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

// vertex shader input
struct VSInput
{
    float3 Position    : POSITION0;
};

// vertex shader output
struct VSOutput
{
    float4 SkyPosition : POSITION0;
    float3 SkyCoords   : TEXCOORD0;
};

// vertex shader
VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    float3 rotated = mul( input.Position, _view );
    output.SkyPosition = mul( float4( rotated, 1.0 ), _projection ).xyww;
    output.SkyCoords = input.Position;

    return output;
}

// pixel shader
float4 PSFunc( VSOutput input ) : COLOR0
{
    return texCUBE( _cubeMapSampler, input.SkyCoords );
}

technique MainTechnique
{
    pass Pass0
    {
        CullMode = None;
        ZWriteEnable = false;

        VertexShader = compile vs_2_0 VSFunc();
        PixelShader  = compile ps_2_0 PSFunc();
    }
}