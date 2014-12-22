float viewportWidth;
float viewportHeight;

// gets the size of half of a pixel
float2 GetHalfPixel()
{
    return 0.5 / float2( viewportWidth, viewportHeight );
}

// calculates the 2D screen position of a 3D world position
float2 ProjectToScreen( float4 pos )
{
    float2 screen = pos.xy / pos.w;
    return 0.5 * ( 1 + float2( screen.x, -screen.y ) );
}