using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

/**
 * The cube assumes one texture per cube. I know in another commit I said
 * that switching textures would be costly, but that was only because I
 * read it somewhere. I'm calling "bull shit" because I just ran a test
 * on my laptop and it only takes an average of 2.1 nanoseconds to set
 * an effect's texture.
 */

namespace Kyoob.Blocks
{
    /// <summary>
    /// A simple cube.
    /// </summary>
    public sealed class Cube
    {
        /// <summary>
        /// The number of triangles in a cube.
        /// </summary>
        private const int CubeTriangleCount = 12;

        /// <summary>
        /// The "universal" vertex data for cubes.
        /// </summary>
        private static VertexPositionNormalTexture[] CubeVertexData;

        /// <summary>
        /// The "universal" vertex buffer for cubes.
        /// </summary>
        private static VertexBuffer CubeVertices;

        private Vector3 _position;
        private BoundingBox _bounds;

        /// <summary>
        /// Gets or sets the cube's position. Will also be the center of the bounding box.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;

                Vector3 tlf = new Vector3( _position.X - 0.5f, _position.Y - 0.5f, _position.Z + 0.5f );
                Vector3 brb = new Vector3( _position.X + 0.5f, _position.Y + 0.5f, _position.Z - 0.5f );
                _bounds = new BoundingBox( tlf, brb );
            }
        }

        /// <summary>
        /// Gets the cube's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Gets the cube's world matrix.
        /// </summary>
        public Matrix World
        {
            get
            {
                return Matrix.CreateTranslation( _position );
            }
        }

        /// <summary>
        /// Static constructor to initialize the tables.
        /// </summary>
        static Cube()
        {
            CubeVertexData = null;
            CubeVertices = null;
        }

        /// <summary>
        /// Creates a new cube.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        public Cube( GraphicsDevice device )
            : this( device, Vector3.Zero )
        {
        }

        /// <summary>
        /// Creates a new cube.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Cube( GraphicsDevice device, float x, float y, float z )
            : this( device, new Vector3( x, y, z ) )
        {
        }

        /// <summary>
        /// Creates a new cube.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        /// <param name="position">The position.</param>
        public Cube( GraphicsDevice device, Vector3 position )
        {
            Position = position; // use property to also set bounding box
            CheckVertexBuffer( device );
        }

        /// <summary>
        /// Generates the cube mesh data if necessary and populates a vertex buffer.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        private static void CheckVertexBuffer( GraphicsDevice device )
        {
            // data from http://tech.pro/tutorial/750/creating-a-textured-box-in-xna

            // make sure we have our data set
            if ( CubeVertexData == null || CubeVertices == null )
            {
                CubeVertexData = new VertexPositionNormalTexture[ 36 ];

                // vertex data
                Vector3 tlf     = new Vector3( -0.5f,  0.5f, -0.5f );
                Vector3 blf     = new Vector3( -0.5f, -0.5f, -0.5f );
                Vector3 trf     = new Vector3(  0.5f,  0.5f, -0.5f );
                Vector3 brf     = new Vector3(  0.5f, -0.5f, -0.5f );
                Vector3 tlb     = new Vector3( -0.5f,  0.5f,  0.5f );
                Vector3 trb     = new Vector3(  0.5f,  0.5f,  0.5f );
                Vector3 blb     = new Vector3( -0.5f, -0.5f,  0.5f );
                Vector3 brb     = new Vector3(  0.5f, -0.5f,  0.5f );

                // normal data
                Vector3 nFront  = Vector3.Forward;
                Vector3 nBack   = Vector3.Backward;
                Vector3 nUp     = Vector3.Up;
                Vector3 nDown   = Vector3.Down;
                Vector3 nLeft   = Vector3.Left;
                Vector3 nRight  = Vector3.Right;

                // texture coordinate data
                Vector2 tl      = new Vector2( 0.0f, 0.0f );
                Vector2 tr      = new Vector2( 1.0f, 0.0f );
                Vector2 bl      = new Vector2( 0.0f, 1.0f );
                Vector2 br      = new Vector2( 1.0f, 1.0f );

                // front face
                CubeVertexData[ 0  ] = new VertexPositionNormalTexture( tlf, nFront, tl );
                CubeVertexData[ 1  ] = new VertexPositionNormalTexture( blf, nFront, bl );
                CubeVertexData[ 2  ] = new VertexPositionNormalTexture( trf, nFront, tr );
                CubeVertexData[ 3  ] = new VertexPositionNormalTexture( blf, nFront, bl );
                CubeVertexData[ 4  ] = new VertexPositionNormalTexture( brf, nFront, br );
                CubeVertexData[ 5  ] = new VertexPositionNormalTexture( trf, nFront, tr );

                // back face
                CubeVertexData[ 6  ] = new VertexPositionNormalTexture( tlb, nBack, tr );
                CubeVertexData[ 7  ] = new VertexPositionNormalTexture( trb, nBack, tl );
                CubeVertexData[ 8  ] = new VertexPositionNormalTexture( blb, nBack, br );
                CubeVertexData[ 9  ] = new VertexPositionNormalTexture( blb, nBack, br );
                CubeVertexData[ 10 ] = new VertexPositionNormalTexture( trb, nBack, tl );
                CubeVertexData[ 11 ] = new VertexPositionNormalTexture( brb, nBack, bl );

                // top face
                CubeVertexData[ 12 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                CubeVertexData[ 13 ] = new VertexPositionNormalTexture( trb, nUp, tr );
                CubeVertexData[ 14 ] = new VertexPositionNormalTexture( tlb, nUp, tl );
                CubeVertexData[ 15 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                CubeVertexData[ 16 ] = new VertexPositionNormalTexture( trf, nUp, br );
                CubeVertexData[ 17 ] = new VertexPositionNormalTexture( trb, nUp, tr );

                // bottom face
                CubeVertexData[ 18 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                CubeVertexData[ 19 ] = new VertexPositionNormalTexture( blb, nDown, bl );
                CubeVertexData[ 20 ] = new VertexPositionNormalTexture( brb, nDown, br );
                CubeVertexData[ 21 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                CubeVertexData[ 22 ] = new VertexPositionNormalTexture( brb, nDown, br );
                CubeVertexData[ 23 ] = new VertexPositionNormalTexture( brf, nDown, tr );

                // left face
                CubeVertexData[ 24 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );
                CubeVertexData[ 25 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                CubeVertexData[ 26 ] = new VertexPositionNormalTexture( blf, nLeft, br );
                CubeVertexData[ 27 ] = new VertexPositionNormalTexture( tlb, nLeft, tl );
                CubeVertexData[ 28 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                CubeVertexData[ 29 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );

                // right face
                CubeVertexData[ 30 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                CubeVertexData[ 31 ] = new VertexPositionNormalTexture( brf, nRight, bl );
                CubeVertexData[ 32 ] = new VertexPositionNormalTexture( brb, nRight, br );
                CubeVertexData[ 33 ] = new VertexPositionNormalTexture( trb, nRight, tr );
                CubeVertexData[ 34 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                CubeVertexData[ 35 ] = new VertexPositionNormalTexture( brb, nRight, br );

                // create the vertex buffer
                CubeVertices = new VertexBuffer( device, VertexPositionNormalTexture.VertexDeclaration, CubeVertexData.Length, BufferUsage.None );
                CubeVertices.SetData<VertexPositionNormalTexture>( CubeVertexData );
            }
        }

        /// <summary>
        /// Draws this cube to a graphics device.
        /// </summary>
        /// <param name="device">The device to draw to.</param>
        /// <param name="effect">The effect to use to draw this cube.</param>
        public void Draw( GraphicsDevice device, BaseEffect effect )
        {
            effect.World = World;

            device.SetVertexBuffer( CubeVertices );
            foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, CubeTriangleCount );
            }
            device.SetVertexBuffer( null );
        }
    }
}