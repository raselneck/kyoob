float3 lightDirection; // will always be normalized
float3 lightPosition;
float3 lightColor;
float lightConeAngle;
float lightFalloff;

// the pixel shader function for a spot light
float4 PSFunc( VSOutput input ) : COLOR0
{
    // extract information
    float2 coords = GetTexCoords( input.LPosition );
    float3 position = UnprojectPosition( coords ).xyz;
    float3 normal = ( tex2D( normalSampler, coords ) - 0.5 ) * 2.0;

    // calculate direction and diffuse
    float3 ldir = normalize( lightPosition - position );
    float diffuse = saturate( dot( normalize( normal.xyz ), lightDirection ) );

    // calculate actual light values using: light = (dot(p - lp, ld) / cos(a))^f
    float d = dot( -ldir, lightDirection );
    float a = cos( lightConeAngle );
    float att = 0.0;
    if ( a < d )
    {
        att = 1 - pow( saturate( a / d ), lightFalloff );
    }

    return float4( diffuse * att * lightColor, 1.0 );
}