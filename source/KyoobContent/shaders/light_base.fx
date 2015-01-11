#include "shared.fx"

float4x4 worldViewProjection;
float4x4 invViewProjection;

// depth texture and sampler
texture2D depthTexture;
sampler2D depthSampler = sampler_state
{
    Texture = <depthTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

// normal texture and sampler
texture2D normalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = <normalTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

// vertex shader input
struct VSInput
{
    float4 Position : POSITION0;
};

// vertex shader output
struct VSOutput
{
    float4 Position  : POSITION0;
    float4 LPosition : TEXCOORD0;
};

// vertex shader function
VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    output.Position = mul( input.Position, worldViewProjection );
    output.LPosition = output.Position;

    return output;
}

// gets the current texture coordinates
float2 GetTexCoords( float4 pos )
{
    return ProjectToScreen( pos ) + GetHalfPixel();
}

// gets the unprojected position in the depth texture
float4 UnprojectPosition( float2 coords )
{
    // extract depth value
    float4 depth = tex2D( depthSampler, coords );

    // re-create position
    float4 position;
    position.x = coords.x * 2.0 - 1.0;
    position.y = ( 1.0 - coords.y ) * 2.0 - 1.0;
    position.z = depth.r;
    position.w = 1.0;

    // transform the position
    position = mul( position, invViewProjection );
    position.xyz /= position.w;
    position.w = 1.0;

    return position;
}

// pixel shader function is reserved for other files
#if defined( DIR_LIGHT )
#  include "dir_light_ps.fx"
#elif defined( POINT_LIGHT )
#  include "point_light_ps.fx"
#elif defined( SPOT_LIGHT )
#  include "spot_light_ps.fx"
#endif

technique MainTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 VSFunc();
        PixelShader  = compile ps_2_0 PSFunc();
    }
}