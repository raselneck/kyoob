float3 lightPosition;
float3 lightColor;
float lightFalloff;
float lightAtten;

// the pixel shader function for a point light
float4 PSFunc( VSOutput input ) : COLOR0
{
    // extract information
    float2 coords = GetTexCoords( input.LPosition );
    float3 position = UnprojectPosition( coords ).xyz;
    float3 normal = ( tex2D( normalSampler, coords ) - 0.5 ) * 2.0;

    // calculate lighting value
    float3 ldir = normalize( lightPosition - position );
    float lighting = clamp( dot( normal, ldir ), 0.0, 1.0 );

    // attenuate the light
    float d = distance( lightPosition, position );
    float att = 1.0 - pow( d / lightAtten, 6 );

    return float4( lightColor * lighting * att, 1.0 );
}