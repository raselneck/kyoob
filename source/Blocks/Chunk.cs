using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#pragma warning disable 1587 // I don't care about "invalid XML comment placement"

#warning TODO : Add chunk interoperability for Chunk.IsEmpty(int,int,int)
#warning TODO : Make bounds calculation fixed
#warning TODO : After bounds calculation is fixed, incorporate octree creation into same loop as block creation

namespace Kyoob.Blocks
{
    /// <summary>
    /// A data structure containing a chunk of blocks.
    /// </summary>
    public sealed class Chunk
    {
        private GraphicsDevice _device;
        private SpriteSheet _spriteSheet;
        private LibNoise.Perlin _noise;
        private Block[ , , ] _blocks;
        private Vector3 _position;
        private BoundingBox _bounds;
        private VertexBuffer _buffer;
        private int _triangleCount;
        private int _vertexCount;

        /// <summary>
        /// Gets this chunk's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Gets the coordinates of the center of the chunk.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="device">The graphics device to be created on.</param>
        /// <param name="position">The chunk's position (the center of the chunk).</param>
        /// <param name="spriteSheet">The sprite sheet to use.</param>
        public Chunk( GraphicsDevice device, Vector3 position, SpriteSheet spriteSheet )
        {
            // initialize our data
            _device = device;
            _spriteSheet = spriteSheet;
            _noise = new LibNoise.Perlin();
            _blocks = new Block[ 16, 16, 16 ];
            _position = position;
            _bounds = new BoundingBox( position, position );

            // create blocks
            for ( int x = 0, xx = (int)position.X - 8; x < 16; ++x, ++xx )
            {
                for ( int y = 0, yy = (int)position.Y - 8; y < 16; ++y, ++yy )
                {
                    for ( int z = 0, zz = (int)position.Z - 8; z < 16; ++z, ++zz )
                    {
                        // create the block
                        _blocks[ x, y, z ] = new Block( ToWorldCoordinates( x, y, z ), GetBlockType( xx, yy, zz ) );
                        Block current = _blocks[ x, y, z ];
                        if ( current.Type != BlockType.Air )
                        {
                            // current.CreateMesh( device );
                        }

                        // re-adjust our bounding box
                        _bounds.Min.X = Math.Min( _bounds.Min.X, current.Position.X - 0.5f );
                        _bounds.Min.Y = Math.Min( _bounds.Min.Y, current.Position.Y - 0.5f );
                        _bounds.Min.Z = Math.Min( _bounds.Min.Z, current.Position.Z - 0.5f );
                        _bounds.Max.X = Math.Max( _bounds.Max.X, current.Position.X + 0.5f );
                        _bounds.Max.Y = Math.Max( _bounds.Max.Y, current.Position.Y + 0.5f );
                        _bounds.Max.Z = Math.Max( _bounds.Max.Z, current.Position.Z + 0.5f );
                    }
                }
            }

            // build the voxel buffer
            BuildVoxelBuffer();
        }

        /// <summary>
        /// Converts local, array-based coodinates into world coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The X coordinate.</param>
        /// <param name="z">The X coordinate.</param>
        /// <returns></returns>
        private Vector3 ToWorldCoordinates( int x, int y, int z )
        {
            return new Vector3(
                _position.X + ( x - 8 ),
                _position.Y + ( y - 8 ),
                _position.Z + ( z - 8 )
            );
        }

        /// <summary>
        /// Builds the voxel buffer.
        /// </summary>
        private void BuildVoxelBuffer()
        {
            /**
             * What we need to do is go through all of our blocks and determine which are exposed at all
             * and which are completely hidden. If they have any exposed sides, then we need to store that
             * face's data so that we can use it in constructing the chunk buffer.
             */

            // create our buffer data list and reset our counts
            List<VertexPositionNormalTexture> bufferData = new List<VertexPositionNormalTexture>();
            _triangleCount = 0;
            _vertexCount = 0;

            // begin block iteration
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        // make sure the block is not empty
                        if ( _blocks[ x, y, z ].IsEmpty )
                        {
                            continue;
                        }

                        // check blocks in all directions (relative to the array)
                        bool above = IsEmpty( x, y - 1, z );
                        bool below = IsEmpty( x, y + 1, z );
                        bool left  = IsEmpty( x + 1, y, z );
                        bool right = IsEmpty( x - 1, y, z );
                        bool front = IsEmpty( x, y, z + 1 );
                        bool back  = IsEmpty( x, y, z - 1 );

                        // make sure the block is actually exposed
                        if ( above && below && left && right && front && back )
                        {
                            continue;
                        }

                        /**
                         * For some reason, the IsEmpty checking is only *kind of* working.
                         * For each of the directions, it works *sometimes*.
                         */

                        // get the block and set its exposed face data
                        Block block = _blocks[ x, y, z ];
                        //if ( !above )
                        //{
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Top, _spriteSheet, block.Type ) );
                            _triangleCount += 6;
                        //}
                        //if ( !below )
                        //{
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Bottom, _spriteSheet, block.Type ) );
                            _triangleCount += 6;
                        //}
                        //if ( !left )
                        //{
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Left, _spriteSheet, block.Type ) );
                            _triangleCount += 6;
                        //}
                        //if ( !right )
                        //{
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Right, _spriteSheet, block.Type ) );
                            _triangleCount += 6;
                        //}
                        //if ( !front )
                        //{
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Front, _spriteSheet, block.Type ) );
                            _triangleCount += 6;
                        //}
                        //if ( !back )
                        //{
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Back, _spriteSheet, block.Type ) );
                            _triangleCount += 6;
                        //}
                    }
                }
            }

            // set our vertex count and create the buffer
            _vertexCount = _triangleCount * 3;
            if ( _buffer != null )
            {
                _buffer.Dispose();
            }
            _buffer = new VertexBuffer( _device, VertexPositionNormalTexture.VertexDeclaration, _vertexCount, BufferUsage.None );
            _buffer.SetData<VertexPositionNormalTexture>( bufferData.ToArray() );
        }

        /// <summary>
        /// Checks to see if a block is empty.
        /// </summary>
        /// <param name="x">The X index of the block to check.</param>
        /// <param name="y">The Y index of the block to check.</param>
        /// <param name="z">The Z index of the block to check.</param>
        private bool IsEmpty( int x, int y, int z )
        {
            // make sure the bounds are legit for this chunk
            if ( ( x < 0 || x > 15 ) ||
                 ( y < 0 || y > 15 ) ||
                 ( z < 0 || z > 15 ) )
            {
                Vector3 world = ToWorldCoordinates( x, y, z );
                return GetBlockType( (int)world.X, (int)world.Y, (int)world.Z ) == BlockType.Air;
            }

            return _blocks[ x, y, z ].IsEmpty;
        }

        /// <summary>
        /// Gets a block type.
        /// </summary>
        /// <param name="x">The global X coordinate.</param>
        /// <param name="y">The global Y coordinate.</param>
        /// <param name="z">The global Z coordinate.</param>
        /// <returns></returns>
        private BlockType GetBlockType( int x, int y, int z )
        {
            double value = _noise.GetValue( ( x - 8 ) / 17.0, ( y - 8 ) / 17.0, ( z - 8 ) / 17.0 );
            
            // just some arbitrary values
            BlockType type = BlockType.Air;
            if ( Math.Abs( value ) >= 0.60 )
            {
                type = BlockType.Dirt;
            }
            else if ( Math.Abs( value ) >= 0.40 && Math.Abs( value ) < 0.50 )
            {
                type = BlockType.Stone;
            }
            else if ( Math.Abs( value ) >= 0.50 && Math.Abs( value ) < 0.60 )
            {
                type = BlockType.Sand;
            }

            return type;
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="effect">The effect to use to draw.</param>
        public void Draw( BaseEffect effect )
        {
            // draw our vertex buffer
            _device.SetVertexBuffer( _buffer );
            foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                _device.DrawPrimitives( PrimitiveType.TriangleList, 0, _triangleCount );
            }
        }
    }
}