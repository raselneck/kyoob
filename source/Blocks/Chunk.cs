using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#pragma warning disable 1587 // I don't care about "invalid XML comment placement"

#warning TODO : Make bounds calculation fixed
#warning TODO : After bounds calculation is fixed, incorporate octree creation into same loop as block creation

namespace Kyoob.Blocks
{
    /// <summary>
    /// A data structure containing a chunk of blocks.
    /// </summary>
    public sealed class Chunk
    {
        /// <summary>
        /// The magic number for chunks. (FourCC = 'CHNK')
        /// </summary>
        private const int MagicNumber = 0x43484E4B;

        private World _world;
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
        /// Creates a chunk that can be loaded from a stream.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="position"></param>
        /// <param name="blockData"></param>
        /// <param name="activeData"></param>
        private Chunk( World world, Vector3 position, BlockType[ , , ] blockData, bool[,,] activeData )
        {
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="world">The world this chunk is in.</param>
        /// <param name="position">The chunk's position (the center of the chunk).</param>
        public Chunk( World world, Vector3 position )
        {
            // initialize our data
            _world = world;
            _blocks = new Block[ 16, 16, 16 ];
            _position = position;
            _bounds = new BoundingBox( position, position );

            // create blocks
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        // create the block
                        Vector3 coords = _world.ChunkToWorld( _position, x, y, z );
                        _blocks[ x, y, z ] = new Block( coords, _world.GetBlockType( coords ) );

                        // re-adjust our bounding box
                        _bounds.Min.X = Math.Min( _bounds.Min.X, coords.X - 0.5f );
                        _bounds.Min.Y = Math.Min( _bounds.Min.Y, coords.Y - 0.5f );
                        _bounds.Min.Z = Math.Min( _bounds.Min.Z, coords.Z - 0.5f );
                        _bounds.Max.X = Math.Max( _bounds.Max.X, coords.X + 0.5f );
                        _bounds.Max.Y = Math.Max( _bounds.Max.Y, coords.Y + 0.5f );
                        _bounds.Max.Z = Math.Max( _bounds.Max.Z, coords.Z + 0.5f );
                    }
                }
            }

            // build the voxel buffer
            BuildVoxelBuffer();
        }

        /// <summary>
        /// Builds the voxel buffer.
        /// </summary>
        private void BuildVoxelBuffer()
        {
            /**
             * What we need to do is go through all of our blocks and determine which are exposed at all
             * and which are completely hidden. If they have any exposed sides, then we need to use that
             * face's data for constructing our vertex buffer.
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
                        // make sure the block is active
                        if ( !_blocks[ x, y, z ].IsActive )
                        {
                            continue;
                        }

                        // check blocks in all directions
                        bool above = !IsEmpty( x, y + 1, z );
                        bool below = !IsEmpty( x, y - 1, z );
                        bool left  = !IsEmpty( x - 1, y, z );
                        bool right = !IsEmpty( x + 1, y, z );
                        bool front = !IsEmpty( x, y, z - 1 );
                        bool back  = !IsEmpty( x, y, z + 1 );

                        // make sure the block is actually not exposed
                        if ( above && below && left && right && front && back )
                        {
                            continue;
                        }

                        // get the block and check which faces are exposed
                        Block block = _blocks[ x, y, z ];
                        if ( !above )
                        {
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Top, _world.SpriteSheet, block.Type ) );
                            _triangleCount += 6;
                        }
                        if ( !below )
                        {
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Bottom, _world.SpriteSheet, block.Type ) );
                            _triangleCount += 6;
                        }
                        if ( !left )
                        {
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Left, _world.SpriteSheet, block.Type ) );
                            _triangleCount += 6;
                        }
                        if ( !right )
                        {
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Right, _world.SpriteSheet, block.Type ) );
                            _triangleCount += 6;
                        }
                        if ( !front )
                        {
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Front, _world.SpriteSheet, block.Type ) );
                            _triangleCount += 6;
                        }
                        if ( !back )
                        {
                            bufferData.AddRange( Cube.GetFaceData( block.Position, CubeFace.Back, _world.SpriteSheet, block.Type ) );
                            _triangleCount += 6;
                        }
                    }
                }
            }

            // set our vertex count and create the buffer
            _vertexCount = _triangleCount * 3;
            if ( _buffer != null )
            {
                _buffer.Dispose();
            }
            _buffer = new VertexBuffer( _world.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, _vertexCount, BufferUsage.None );
            _buffer.SetData<VertexPositionNormalTexture>( bufferData.ToArray() );
        }

        /// <summary>
        /// Checks to see if the given coordinates are given in the chunk.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns></returns>
        private bool IsValidInChunk( int x, int y, int z )
        {
            return ( x >= 0 && x < 16 )
                && ( y >= 0 && y < 16 )
                && ( z >= 0 && z < 16 );
        }

        /// <summary>
        /// Checks to see if a block is empty.
        /// </summary>
        /// <param name="x">The X index of the block to check.</param>
        /// <param name="y">The Y index of the block to check.</param>
        /// <param name="z">The Z index of the block to check.</param>
        private bool IsEmpty( int x, int y, int z )
        {
            // get the world block type
            Vector3 coords = _world.ChunkToWorld( _position, x, y, z );
            return _world.GetBlockType( coords ) == BlockType.Air;
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="effect">The effect to use to draw.</param>
        public void Draw( BaseEffect effect )
        {
            // draw our vertex buffer
            _world.GraphicsDevice.SetVertexBuffer( _buffer );
            foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                _world.GraphicsDevice.DrawPrimitives( PrimitiveType.TriangleList, 0, _triangleCount );
            }
        }

        /// <summary>
        /// Saves this chunk to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void SaveTo( Stream stream )
        {
            // create helper writer and write the magic number
            BinaryWriter bin = new BinaryWriter( stream );
            bin.Write( MagicNumber );

            // save chunk center
            bin.Write( _position.X );
            bin.Write( _position.Y );
            bin.Write( _position.Z );

            // save block data
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        bin.Write( (byte)_blocks[ x, y, z ].Type );
                        bin.Write( _blocks[ x, y, z ].IsActive );
                    }
                }
            }
        }

        /// <summary>
        /// Reads a chunk's data from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="world">The world the chunk is in.</param>
        /// <returns></returns>
        public static Chunk ReadFrom( Stream stream, World world )
        {

        }
    }
}