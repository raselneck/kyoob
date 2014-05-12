float4x4  _world;
float4x4  _view;
float4x4  _projection;

float3    _ambientColor     = float3( 0.1, 0.1, 0.1 );
float3    _diffuseColor     = float3( 0.85, 0.85, 0.85 );
float3    _lightColor       = float3( 1.0, 1.0, 1.0 );
float3    _lightPosition    = float3( 0.0, 0.0, 0.0 );
float     _lightAttenuation = 10.0;
float     _lightFalloff     = 4.0;

// point clamp the textures for the good ol' blocky style
texture2D _texture;
sampler2D _textureSampler : register(s0) = sampler_state
{
    texture   = <_texture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

// vertex shader input data
struct VSInput
{
    float4 Position : POSITION0;
    float2 UV       : TEXCOORD0;
    float3 Normal   : NORMAL0;
};

// vertex shader output data
struct VSOutput
{
    float4 Position : POSITION0;
    float2 UV       : TEXCOORD0;
    float3 Normal   : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;
};

// vertex shader
VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    // transform input position
    float4 worldPosition = mul( input.Position, _world );
    float4 viewPosition  = mul( worldPosition, _view );
    output.Position      = mul( viewPosition, _projection );

    // send input data to pixel shader
    output.UV            = input.UV;
    output.Normal        = mul( input.Normal, _world );
    output.WorldPos      = worldPosition;

    return output;
}

// pixel shader
float4 PSFunc( VSOutput input ) : COLOR0
{
    float3 diffuseColor = _diffuseColor;
    float4 texColor = tex2D( _textureSampler, input.UV );
    diffuseColor *= texColor.rgb;

    float3 lightDir = normalize( _lightPosition - input.WorldPos );
    float  diffuse  = saturate( dot( normalize( input.Normal ), lightDir ) );
    float  dist     = distance( _lightPosition, input.WorldPos );
    float  att      = 1 - pow( clamp( dist / _lightAttenuation, 0.0, 1.0 ), _lightFalloff );

    float3 light    = _ambientColor + diffuse * att * _lightColor;
    return float4( diffuseColor * light, texColor.a );
}


technique MainTechnique
{
    pass Pass0
    {
        AlphaBlendEnable = FALSE;
        VertexShader     = compile vs_2_0 VSFunc();
        PixelShader      = compile ps_2_0 PSFunc();
    }
}

technique AlphaTechnique
{
    pass Pass0
    {
        AlphaBlendEnable = TRUE;
        DestBlend        = INVSRCALPHA;
        SrcBlend         = SRCALPHA;
        VertexShader     = compile vs_2_0 VSFunc();
        PixelShader      = compile ps_2_0 PSFunc();
    }
}