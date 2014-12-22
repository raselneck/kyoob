#include "shared.fx"

float4x4 world;
float4x4 view;
float4x4 projection;
float3 ambientColor = float3( 0.15, 0.15, 0.15 );
float3 diffuseColor = float3( 1.00, 1.00, 1.00 );

// sprite texture information
texture2D spriteTexture;
sampler2D spriteSampler = sampler_state
{
    Texture = <spriteTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// light texture information
texture2D lightTexture;
sampler2D lightSampler = sampler_state
{
    Texture = <lightTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

// vertex shader input
struct VSInput
{
    float4 Position : POSITION0;
    float2 UV       : TEXCOORD0;
};

// vertex shader output
struct VSOutput
{
    float4 Position  : POSITION0;
    float2 UV        : TEXCOORD0;
    float4 TPosition : TEXCOORD1;
};

// vertex shader function
VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    float4 worldPosition = mul( input.Position, world );
    float4 viewPosition = mul( worldPosition, view );
    
    output.Position = mul( viewPosition, projection );
    output.TPosition = output.Position;
    output.UV = input.UV;

    return output;
}

// pixel shader function
float4 PSFunc( VSOutput input ) : COLOR0
{
    // extract lighting info
    float2 coords = ProjectToScreen( input.TPosition ) + GetHalfPixel();
    float3 light = tex2D( lightSampler, coords );
    light += ambientColor;

    // calculate final color
    float3 tex = tex2D( spriteSampler, input.UV );
    return float4( tex * diffuseColor * light, 1.0 );
}

technique MainTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 VSFunc();
        PixelShader  = compile ps_2_0 PSFunc();
    }
}