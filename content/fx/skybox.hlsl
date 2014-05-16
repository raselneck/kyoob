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

// vertex shader
void VSFunc( float3 pos : POSITION0,
         out float4 skyPos : POSITION0,
         out float3 skyCoord : TEXCOORD0 )
{
    float3 rotated = mul( pos, _view );
    skyPos = mul( float4( rotated, 1.0 ), _projection ).xyww;
    skyCoord = pos;
}

// pixel shader
float4 PSFunc( float3 skyCoord : TEXCOORD0 ) : COLOR
{
    return texCUBE( _cubeMapSampler, skyCoord );
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