using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#warning See comment in Cube.cs
/**
 * The cube assumes one texture per cube. As this would be very costly,
 * create a SpriteSheet class to use with getting the texture coordinates
 * associated with a given block type.
 */

namespace Kyoob
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
        /// The "universal" mesh data for cubes.
        /// </summary>
        private static VertexPositionNormalTexture[] CubeMeshData;

        private Vector3 _position;
        private BoundingBox _bounds;
        private VertexBuffer _vertices;

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
            CubeMeshData = null;
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
            CreateVertexBuffer( device, out _vertices );
        }

        /// <summary>
        /// Generates the cube mesh data if necessary and populates a vertex buffer.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        private static void CreateVertexBuffer( GraphicsDevice device, out VertexBuffer buffer )
        {
            // data from http://tech.pro/tutorial/750/creating-a-textured-box-in-xna

            // make sure we have our table
            if ( CubeMeshData == null )
            {
                CubeMeshData = new VertexPositionNormalTexture[ 36 ];

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
                Vector3 nFront  = Vector3.Backward;
                Vector3 nBack   = Vector3.Forward;
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
                CubeMeshData[ 0  ] = new VertexPositionNormalTexture( tlf, nFront, tl );
                CubeMeshData[ 1  ] = new VertexPositionNormalTexture( blf, nFront, bl );
                CubeMeshData[ 2  ] = new VertexPositionNormalTexture( trf, nFront, tr );
                CubeMeshData[ 3  ] = new VertexPositionNormalTexture( blf, nFront, bl );
                CubeMeshData[ 4  ] = new VertexPositionNormalTexture( brf, nFront, br );
                CubeMeshData[ 5  ] = new VertexPositionNormalTexture( trf, nFront, tr );

                // back face
                CubeMeshData[ 6  ] = new VertexPositionNormalTexture( tlb, nBack, tr );
                CubeMeshData[ 7  ] = new VertexPositionNormalTexture( trb, nBack, tl );
                CubeMeshData[ 8  ] = new VertexPositionNormalTexture( blb, nBack, br );
                CubeMeshData[ 9  ] = new VertexPositionNormalTexture( blb, nBack, br );
                CubeMeshData[ 10 ] = new VertexPositionNormalTexture( trb, nBack, tl );
                CubeMeshData[ 11 ] = new VertexPositionNormalTexture( brb, nBack, bl );

                // top face
                CubeMeshData[ 12 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                CubeMeshData[ 13 ] = new VertexPositionNormalTexture( trb, nUp, tr );
                CubeMeshData[ 14 ] = new VertexPositionNormalTexture( tlb, nUp, tl );
                CubeMeshData[ 15 ] = new VertexPositionNormalTexture( tlf, nUp, bl );
                CubeMeshData[ 16 ] = new VertexPositionNormalTexture( trf, nUp, br );
                CubeMeshData[ 17 ] = new VertexPositionNormalTexture( trb, nUp, tr );

                // bottom face
                CubeMeshData[ 18 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                CubeMeshData[ 19 ] = new VertexPositionNormalTexture( blb, nDown, bl );
                CubeMeshData[ 20 ] = new VertexPositionNormalTexture( brb, nDown, br );
                CubeMeshData[ 21 ] = new VertexPositionNormalTexture( blf, nDown, tl );
                CubeMeshData[ 22 ] = new VertexPositionNormalTexture( brb, nDown, br );
                CubeMeshData[ 23 ] = new VertexPositionNormalTexture( brf, nDown, tr );

                // left face
                CubeMeshData[ 24 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );
                CubeMeshData[ 25 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                CubeMeshData[ 26 ] = new VertexPositionNormalTexture( blf, nLeft, br );
                CubeMeshData[ 27 ] = new VertexPositionNormalTexture( tlb, nLeft, tl );
                CubeMeshData[ 28 ] = new VertexPositionNormalTexture( blb, nLeft, bl );
                CubeMeshData[ 29 ] = new VertexPositionNormalTexture( tlf, nLeft, tr );

                // right face
                CubeMeshData[ 30 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                CubeMeshData[ 31 ] = new VertexPositionNormalTexture( brf, nRight, bl );
                CubeMeshData[ 32 ] = new VertexPositionNormalTexture( brb, nRight, br );
                CubeMeshData[ 33 ] = new VertexPositionNormalTexture( trb, nRight, tr );
                CubeMeshData[ 34 ] = new VertexPositionNormalTexture( trf, nRight, tl );
                CubeMeshData[ 35 ] = new VertexPositionNormalTexture( brb, nRight, br );
            }

            buffer = new VertexBuffer( device, VertexPositionNormalTexture.VertexDeclaration, CubeMeshData.Length, BufferUsage.None );
            buffer.SetData<VertexPositionNormalTexture>( CubeMeshData );
        }

        /// <summary>
        /// Draws this cube to a graphics device.
        /// </summary>
        /// <param name="device">The device to draw to.</param>
        /// <param name="effect">The effect to use to draw this cube.</param>
        public void Draw( GraphicsDevice device, Effect effect )
        {
            // operate under assumption effect is a basic effect
            ( (BasicEffect)effect ).World = World;

            device.SetVertexBuffer( _vertices );
            foreach ( EffectPass pass in effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, CubeTriangleCount );
            }
        }
    }
}