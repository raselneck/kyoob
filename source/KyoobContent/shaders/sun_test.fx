float4x4 world;
float4x4 view;
float4x4 projection;

float3 lightDirection;
float3 lightColor;
float3 diffuseColor;
float3 ambientColor;

texture2D spriteSheet;
sampler2D spriteSampler = sampler_state
{
    Texture = <spriteSheet>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

VSOutput VSFunc( VSInput input )
{
    VSOutput output;

    // calculate output position
    float4 worldPosition = mul( input.Position, world );
    float4 viewPosition = mul( worldPosition, view );
    output.Position = mul( viewPosition, projection );

    // calculate normal
    float3 normal = mul( input.Normal, world );
    output.Normal = normalize( normal );

    // pass-through UV coordinates
    output.UV = input.UV;

    return output;
}

float4 PSFunc( VSOutput input ) : COLOR0
{
    float4 color = float4( diffuseColor, 1.0 );

    // add in the texture color
    color *= tex2D( spriteSampler, input.UV );

    // calculate Lambertian directional light
    float3 lighting = ambientColor;
    float3 lDir = normalize( lightDirection );
    float3 normal = input.Normal;
    lighting += clamp( -dot( lDir, normal ), ambientColor.r, 1.0 ) * lightColor; // assume ambient color is a shade of gray

    // calculate and return final color
    color *= float4( saturate( lighting ), 1.0 );
    return color;
}

technique MainTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 VSFunc();
        PixelShader  = compile ps_2_0 PSFunc();
    }
}