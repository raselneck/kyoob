float3 lightDirection; // will always be normalized
float3 lightColor;

// the pixel shader function for a directional light
float4 PSFunc( VSOutput input ) : COLOR0
{
    // extract information
    float2 coords = GetTexCoords( input.LPosition );
    float3 normal = ( tex2D( normalSampler, coords ) - 0.5 ) * 2.0;

    // calculate the lighting value
    float3 lighting = clamp( -dot( lightDirection, normal ), 0.0, 1.0 ) * lightColor;
    return float4( lighting, 1.0 );
}