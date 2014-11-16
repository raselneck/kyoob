using Kyoob.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Blocks
{
    /// <summary>
    /// A simple cube.
    /// </summary>
    public static class Cube
    {
        /// <summary>
        /// The number of triangles per cube face.
        /// </summary>
        public const int TrianglesPerFace = 2;

        /// <summary>
        /// The number of vertices per cube face.
        /// </summary>
        public const int VerticesPerFace = 6;

        /// <summary>
        /// The size of a cube.
        /// </summary>
        public const float Size = 1.0f;

        private static VertexPositionNormalTexture[] _data = new VertexPositionNormalTexture[ 6 ];

        /// <summary>
        /// Creates data for a single cube face.
        /// </summary>
        /// <param name="center">The center of the cube.</param>
        /// <param name="face">The cube face to create data for.</param>
        /// <param name="spritesheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        /// <returns></returns>
        public static VertexPositionNormalTexture[] CreateFaceData( Vector3 center, CubeFace face, SpriteSheet spritesheet, BlockType type )
        {
            // vertex data
            Vector3 tlf = new Vector3( center.X - 0.5f, center.Y + 0.5f, center.Z - 0.5f );
            Vector3 blf = new Vector3( center.X - 0.5f, center.Y - 0.5f, center.Z - 0.5f );
            Vector3 trf = new Vector3( center.X + 0.5f, center.Y + 0.5f, center.Z - 0.5f );
            Vector3 brf = new Vector3( center.X + 0.5f, center.Y - 0.5f, center.Z - 0.5f );
            Vector3 tlb = new Vector3( center.X - 0.5f, center.Y + 0.5f, center.Z + 0.5f );
            Vector3 trb = new Vector3( center.X + 0.5f, center.Y + 0.5f, center.Z + 0.5f );
            Vector3 blb = new Vector3( center.X - 0.5f, center.Y - 0.5f, center.Z + 0.5f );
            Vector3 brb = new Vector3( center.X + 0.5f, center.Y - 0.5f, center.Z + 0.5f );

            // normal data
            Vector3 nFront = Vector3.Forward;
            Vector3 nBack = Vector3.Backward;
            Vector3 nUp = Vector3.Up;
            Vector3 nDown = Vector3.Down;
            Vector3 nLeft = Vector3.Left;
            Vector3 nRight = Vector3.Right;

            // texture coordinate data
            Vector2 tl = spritesheet.GetTexCoords( type, face );
            Vector2 tr = new Vector2( tl.X + spritesheet.TexCoordWidth, tl.Y );
            Vector2 bl = new Vector2( tl.X, tl.Y + spritesheet.TexCoordHeight );
            Vector2 br = new Vector2( tl.X + spritesheet.TexCoordWidth, tl.Y + spritesheet.TexCoordHeight );

            switch ( face )
            {
                case CubeFace.Front:
                    // front face
                    _data[ 0 ] = new VertexPositionNormalTexture( tlf, nFront, tl );
                    _data[ 1 ] = new VertexPositionNormalTexture( blf, nFront, bl );
                    _data[ 2 ] = new VertexPositionNormalTexture( trf, nFront, tr );
                    _data[ 3 ] = new VertexPositionNormalTexture( blf, nFront, bl );
                    _data[ 4 ] = new VertexPositionNormalTexture( brf, nFront, br );
                    _data[ 5 ] = new VertexPositionNormalTexture( trf, nFront, tr );
                    break;
                case CubeFace.Back:
                    // back face
                    _data[ 0 ] = new VertexPositionNormalTexture( tlb, nBack, tr );
                    _data[ 1 ] = new VertexPositionNormalTexture( trb, nBack, tl );
                    _data[ 2 ] = new VertexPositionNormalTexture( blb, nBack, br );
                    _data[ 3 ] = new VertexPositionNormalTexture( blb, nBack, br );
                    _data[ 4 ] = new VertexPositionNormalTexture( trb, nBack, tl );
                    _data[ 5 ] = new VertexPositionNormalTexture( brb, nBack, bl );
                    break;
                case CubeFace.Top:
                    // top face
                    _data[ 0 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                    _data[ 1 ] = new VertexPositionNormalTexture( trb, nUp, tr );
                    _data[ 2 ] = new VertexPositionNormalTexture( tlb, nUp, tl );
                    _data[ 3 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                    _data[ 4 ] = new VertexPositionNormalTexture( trf, nUp, br );
                    _data[ 5 ] = new VertexPositionNormalTexture( trb, nUp, tr );
                    break;
                case CubeFace.Bottom:
                    // bottom face
                    _data[ 0 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                    _data[ 1 ] = new VertexPositionNormalTexture( blb, nDown, bl );
                    _data[ 2 ] = new VertexPositionNormalTexture( brb, nDown, br );
                    _data[ 3 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                    _data[ 4 ] = new VertexPositionNormalTexture( brb, nDown, br );
                    _data[ 5 ] = new VertexPositionNormalTexture( brf, nDown, tr );
                    break;
                case CubeFace.Left:
                    // left face
                    _data[ 0 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );
                    _data[ 1 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                    _data[ 2 ] = new VertexPositionNormalTexture( blf, nLeft, br );
                    _data[ 3 ] = new VertexPositionNormalTexture( tlb, nLeft, tl );
                    _data[ 4 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                    _data[ 5 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );
                    break;
                case CubeFace.Right:
                    // right face
                    _data[ 0 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                    _data[ 1 ] = new VertexPositionNormalTexture( brf, nRight, bl );
                    _data[ 2 ] = new VertexPositionNormalTexture( brb, nRight, br );
                    _data[ 3 ] = new VertexPositionNormalTexture( trb, nRight, tr );
                    _data[ 4 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                    _data[ 5 ] = new VertexPositionNormalTexture( brb, nRight, br );
                    break;
            }

            return _data;
        }
    }
}