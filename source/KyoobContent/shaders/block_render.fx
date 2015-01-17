float4x4 world;
float4x4 view;
float4x4 projection;
float3 playerPosition;

float3 fogColor;
float fogStart;
float fogEnd;

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

struct VertexInput
{
    float4 Position     : POSITION0;
    float3 Normal       : NORMAL0;
    float2 UV           : TEXCOORD0;
    float  LightLevel   : TEXCOORD1;
};

struct VertexOutput
{
    float4 Position     : POSITION0;
    float2 UV           : TEXCOORD0;
    float  LightLevel   : TEXCOORD1;
    float3 ViewDir      : TEXCOORD2;
};

// vertex shader function
VertexOutput VertexFunc( VertexInput input )
{
    VertexOutput output;

    // calculate world position
    float4 worldPosition = mul( input.Position, world );
    float4 viewPosition = mul( worldPosition, view );
    output.Position = mul( viewPosition, projection );

    // save texture coordinates
    output.UV = input.UV;

    // save light level
    output.LightLevel = input.LightLevel;

    // get the view direction
    output.ViewDir = worldPosition - playerPosition;

    return output;
}

// pixel shader function
float4 PixelFunc( VertexOutput input ) : COLOR0
{
    // get texture color
    float3 color = tex2D( spriteSampler, input.UV ).rgb;
    color *= input.LightLevel; // dim the color 

    // add in fog
    float dist = length( input.ViewDir );
    float fogAmount = clamp( ( dist - fogStart ) / ( fogEnd - fogStart ), 0.0, 1.0 );
    color = lerp( color, fogColor, fogAmount );

    return float4( color, 1.0 );
}

technique MainTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 VertexFunc();
        PixelShader  = compile ps_2_0 PixelFunc();
    }
}
