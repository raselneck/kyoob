using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Blocks
{
    /// <summary>
    /// A simple cube.
    /// </summary>
    public sealed class Cube
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
        /// Creates data for a single cube face.
        /// </summary>
        /// <param name="center">The center of the cube.</param>
        /// <param name="face">The cube face to create data for.</param>
        /// <param name="spritesheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        /// <returns></returns>
        public static VertexPositionNormalTexture[] CreateFaceData( Vector3 center, CubeFace face, SpriteSheet spritesheet, BlockType type )
        {
            VertexPositionNormalTexture[] data = new VertexPositionNormalTexture[ 6 ];

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

            switch (face)
            {
                case CubeFace.Front:
                    // front face
                    data[ 0 ] = new VertexPositionNormalTexture( tlf, nFront, tl );
                    data[ 1 ] = new VertexPositionNormalTexture( blf, nFront, bl );
                    data[ 2 ] = new VertexPositionNormalTexture( trf, nFront, tr );
                    data[ 3 ] = new VertexPositionNormalTexture( blf, nFront, bl );
                    data[ 4 ] = new VertexPositionNormalTexture( brf, nFront, br );
                    data[ 5 ] = new VertexPositionNormalTexture( trf, nFront, tr );
                    break;
                case CubeFace.Back:
                    // back face
                    data[ 0 ] = new VertexPositionNormalTexture( tlb, nBack, tr );
                    data[ 1 ] = new VertexPositionNormalTexture( trb, nBack, tl );
                    data[ 2 ] = new VertexPositionNormalTexture( blb, nBack, br );
                    data[ 3 ] = new VertexPositionNormalTexture( blb, nBack, br );
                    data[ 4 ] = new VertexPositionNormalTexture( trb, nBack, tl );
                    data[ 5 ] = new VertexPositionNormalTexture( brb, nBack, bl );
                    break;
                case CubeFace.Top:
                    // top face
                    data[ 0 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                    data[ 1 ] = new VertexPositionNormalTexture( trb, nUp, tr );
                    data[ 2 ] = new VertexPositionNormalTexture( tlb, nUp, tl );
                    data[ 3 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                    data[ 4 ] = new VertexPositionNormalTexture( trf, nUp, br );
                    data[ 5 ] = new VertexPositionNormalTexture( trb, nUp, tr );
                    break;
                case CubeFace.Bottom:
                    // bottom face
                    data[ 0 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                    data[ 1 ] = new VertexPositionNormalTexture( blb, nDown, bl );
                    data[ 2 ] = new VertexPositionNormalTexture( brb, nDown, br );
                    data[ 3 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                    data[ 4 ] = new VertexPositionNormalTexture( brb, nDown, br );
                    data[ 5 ] = new VertexPositionNormalTexture( brf, nDown, tr );
                    break;
                case CubeFace.Left:
                    // left face
                    data[ 0 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );
                    data[ 1 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                    data[ 2 ] = new VertexPositionNormalTexture( blf, nLeft, br );
                    data[ 3 ] = new VertexPositionNormalTexture( tlb, nLeft, tl );
                    data[ 4 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                    data[ 5 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );
                    break;
                case CubeFace.Right:
                    // right face
                    data[ 0 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                    data[ 1 ] = new VertexPositionNormalTexture( brf, nRight, bl );
                    data[ 2 ] = new VertexPositionNormalTexture( brb, nRight, br );
                    data[ 3 ] = new VertexPositionNormalTexture( trb, nRight, tr );
                    data[ 4 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                    data[ 5 ] = new VertexPositionNormalTexture( brb, nRight, br );
                    break;
            }
            
            return data;
        }
    }
}