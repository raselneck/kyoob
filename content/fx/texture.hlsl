float4x4  _world;
float4x4  _view;
float4x4  _projection;

texture2D _texture;
sampler2D _textureSampler : register(s0) = sampler_state
{
    texture   = <_texture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

// vertex shader input data
struct VSInput
{
    float4 Position : POSITION0;
    float2 UV       : TEXCOORD0;
};

// vertex shader output data
struct VSOutput
{
    float4 Position : POSITION0;
    float2 UV       : TEXCOORD0;
};

// vertex shader
VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    float4 worldPosition = mul( input.Position, _world );
    float4 viewPosition  = mul( worldPosition, _view );
    output.Position      = mul( viewPosition, _projection );
    output.UV            = input.UV;

    return output;
}

// pixel shader
float4 PSFunc( VSOutput input ) : COLOR0
{
    float3 color = tex2D( _textureSampler, input.UV );
    return float4( color, 1.0 );
}


technique MainTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VSFunc();
        PixelShader  = compile ps_2_0 PSFunc();
    }
}