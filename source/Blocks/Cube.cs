using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

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
        public const int CubeTriangleCount = 12;

        /// <summary>
        /// The number of vertices in a cube.
        /// </summary>
        public const int CubeVertexCount = 36;

        /// <summary>
        /// The "universal" vertex buffer for cubes.
        /// </summary>
        public static VertexBuffer CubeVertices;

        private Matrix _world;

        /// <summary>
        /// Gets or sets the cube's position. Will also be the center of the bounding box.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _world.Translation;
            }
            set
            {
                _world.Translation = value;
            }
        }

        /// <summary>
        /// Gets the cube's world matrix.
        /// </summary>
        public Matrix World
        {
            get
            {
                return _world;
            }
        }

        /// <summary>
        /// Static constructor to initialize the tables.
        /// </summary>
        static Cube()
        {
            CubeVertices = null;
        }

        /// <summary>
        /// Creates a new cube.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        /// <param name="spriteSheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        public Cube( GraphicsDevice device, SpriteSheet spriteSheet, BlockType type )
            : this( device, Vector3.Zero, spriteSheet, type )
        {
        }

        /// <summary>
        /// Creates a new cube.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <param name="spriteSheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        public Cube( GraphicsDevice device, float x, float y, float z, SpriteSheet spriteSheet, BlockType type )
            : this( device, new Vector3( x, y, z ), spriteSheet, type )
        {
        }

        /// <summary>
        /// Creates a new cube.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        /// <param name="position">The position.</param>
        /// <param name="spriteSheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        public Cube( GraphicsDevice device, Vector3 position, SpriteSheet spriteSheet, BlockType type )
        {
            _world = Matrix.CreateTranslation( position );
            CheckVertexBuffer( device, spriteSheet, type );
        }

        /// <summary>
        /// Generates the cube mesh data if necessary and populates a vertex buffer.
        /// </summary>
        /// <param name="device">The graphics device to create the cube on.</param>
        /// <param name="spriteSheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        private static void CheckVertexBuffer( GraphicsDevice device, SpriteSheet spriteSheet, BlockType type )
        {
            // data from http://tech.pro/tutorial/750/creating-a-textured-box-in-xna

            // make sure we have our data set
            if ( CubeVertices == null )
            {
                CubeVertices = CreateBuffer( device, Vector3.Zero, spriteSheet, type );
            }
            device.SetVertexBuffer( CubeVertices );
        }

        /// <summary>
        /// Creates a vertex buffer of a textured cube.
        /// </summary>
        /// <param name="device">The graphics device to create on.</param>
        /// <param name="center">The center of the cube.</param>
        /// <param name="spritesheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        /// <returns></returns>
        public static VertexBuffer CreateBuffer( GraphicsDevice device, Vector3 center, SpriteSheet spritesheet, BlockType type )
        {
            // get data for each face
            List<VertexPositionNormalTexture> data = new List<VertexPositionNormalTexture>( 36 );
            data.AddRange( GetFaceData( center, CubeFace.Front, spritesheet, type ) );
            data.AddRange( GetFaceData( center, CubeFace.Back, spritesheet, type ) );
            data.AddRange( GetFaceData( center, CubeFace.Top, spritesheet, type ) );
            data.AddRange( GetFaceData( center, CubeFace.Bottom, spritesheet, type ) );
            data.AddRange( GetFaceData( center, CubeFace.Left, spritesheet, type ) );
            data.AddRange( GetFaceData( center, CubeFace.Right, spritesheet, type ) );

            // copy that data into a vertex buffer
            VertexBuffer buffer = new VertexBuffer( device, VertexPositionNormalTexture.VertexDeclaration, data.Count, BufferUsage.None );
            buffer.SetData<VertexPositionNormalTexture>( data.ToArray() );
            return buffer;
        }

        /// <summary>
        /// Gets cube face data.
        /// </summary>
        /// <param name="center">The center of the cube.</param>
        /// <param name="face">The cube face to create data for.</param>
        /// <param name="spritesheet">The sprite sheet to use for texture coordinates.</param>
        /// <param name="type">The destined block type.</param>
        /// <returns></returns>
        public static VertexPositionNormalTexture[] GetFaceData( Vector3 center, CubeFace face, SpriteSheet spritesheet, BlockType type )
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
            Vector2 tl = spritesheet.GetTexCoords( type );
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

        /// <summary>
        /// Draws this cube to a graphics device.
        /// </summary>
        /// <param name="device">The device to draw to.</param>
        /// <param name="effect">The effect to use to draw this cube.</param>
        public void Draw( GraphicsDevice device, BaseEffect effect )
        {
            effect.World = World;

            // vertex buffer binding and unbinding is expensive for lots of cubes,
            // so let's *assume* that the vertex buffer is bound

            //device.SetVertexBuffer( CubeVertices ); // stupid fucking SpriteBatch
            foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, CubeTriangleCount );
            }
        }
    }
}