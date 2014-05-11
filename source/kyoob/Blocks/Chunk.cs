using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#pragma warning disable 1587 // disable "invalid XML comment placement"

#warning TODO : Change bounds calculations to use Chunk.Size

namespace Kyoob.Blocks
{
    /// <summary>
    /// A data structure containing a chunk of blocks.
    /// </summary>
    public sealed class Chunk : IDisposable
    {
        /// <summary>
        /// The size of each chunk. (Where each chunk is Size x Size x Size.)
        /// </summary>
        public const int Size = 16;

        /// <summary>
        /// The magic number for chunks. (FourCC = 'CHNK')
        /// </summary>
        private const int MagicNumber = 0x4B4E4843;

        private Vector3 _position;
        private BoundingBox _bounds;
        private World _world;
        private Block[ , , ] _blocks;
        private Octree<Block> _octree;
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
        /// Gets the world this chunk is in.
        /// </summary>
        public World World
        {
            get
            {
                return _world;
            }
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
            _blocks = new Block[ Size, Size, Size ];
            _position = position;
            _bounds = new BoundingBox(
                new Vector3( _position.X - 8.5f, _position.Y - 8.5f, _position.Z - 8.5f ),
                new Vector3( _position.X + 7.5f, _position.Y + 7.5f, _position.Z + 7.5f )
            );
            _octree = new Octree<Block>( _bounds );

            // tell the terrain generator to generate data for us
            _world.TerrainGenerator.CurrentChunk = this;

            // create blocks
            for ( int x = 0; x < Size; ++x )
            {
                for ( int y = 0; y < Size; ++y )
                {
                    for ( int z = 0; z < Size; ++z )
                    {
                        // create the block
                        Vector3 coords = _world.LocalToWorld( _position, x, y, z );
                        BlockType type = BlockType.Bedrock;
                        if ( coords.Y > 0 )
                        {
                            type = _world.TerrainGenerator.GetBlockType( coords );
                        }
                        _blocks[ x, y, z ] = new Block( coords, type );
                    }
                }
            }

            // build the voxel buffer and octree
            BuildVoxelBuffer();
        }

        /// <summary>
        /// Creates a chunk by loading data from a binary reader.
        /// </summary>
        /// <param name="bin">The reader.</param>
        /// <param name="world">The world this chunk belongs in.</param>
        private Chunk( BinaryReader bin, World world )
        {
            // set world and create blocks
            _world = world;
            _blocks = new Block[ Size, Size, Size ];

            // read position
            _position = new Vector3(
                bin.ReadSingle(),
                bin.ReadSingle(),
                bin.ReadSingle()
            );

            // create bounds and octree
            _bounds = new BoundingBox(
                new Vector3( _position.X - 8.5f, _position.Y - 8.5f, _position.Z - 8.5f ),
                new Vector3( _position.X + 7.5f, _position.Y + 7.5f, _position.Z + 7.5f )
            );
            _octree = new Octree<Block>( _bounds );

            // load each block
            for ( int x = 0; x < Size; ++x )
            {
                for ( int y = 0; y < Size; ++y )
                {
                    for ( int z = 0; z < Size; ++z )
                    {
                        // load block data
                        Vector3 coords = _world.LocalToWorld( _position, x, y, z );
                        BlockType type = (BlockType)bin.ReadByte();
                        bool active = bin.ReadBoolean();

                        // create the block
                        _blocks[ x, y, z ] = new Block( coords, type );
                        _blocks[ x, y, z ].IsActive = active;
                    }
                }
            }

            // finally, build our buffer and octree
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

            // create our buffer data list, reset our counts, and clear our octree
            List<VertexPositionNormalTexture> bufferData = new List<VertexPositionNormalTexture>();
            _triangleCount = 0;
            _vertexCount = 0;
            _octree.Clear();

            // begin block iteration
            for ( int x = 0; x < Size; ++x )
            {
                for ( int y = 0; y < Size; ++y )
                {
                    for ( int z = 0; z < Size; ++z )
                    {
                        Block block = _blocks[ x, y, z ];

                        // make sure the block is active
                        if ( !block.IsActive )
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

                        // if the type is dirt and there's nothing on top, then it's grass
                        if ( block.Type == BlockType.Dirt && !above )
                        {
                            block.Type = BlockType.Grass;
                        }

                        // add the block to the octree because it's active
                        _octree.Add( block );

                        // make sure the block is actually not exposed
                        if ( above && below && left && right && front && back )
                        {
                            continue;
                        }

                        // get the block and check which faces are exposed
                        if ( !above )
                        {
                            bufferData.AddRange( Cube.CreateFaceData( block.Position, CubeFace.Top, _world.SpriteSheet, block.Type ) );
                            _triangleCount += Cube.TrianglesPerFace;
                            _vertexCount += Cube.VerticesPerFace;
                        }
                        if ( !below )
                        {
                            bufferData.AddRange( Cube.CreateFaceData( block.Position, CubeFace.Bottom, _world.SpriteSheet, block.Type ) );
                            _triangleCount += Cube.TrianglesPerFace;
                            _vertexCount += Cube.VerticesPerFace;
                        }
                        if ( !left )
                        {
                            bufferData.AddRange( Cube.CreateFaceData( block.Position, CubeFace.Left, _world.SpriteSheet, block.Type ) );
                            _triangleCount += Cube.TrianglesPerFace;
                            _vertexCount += Cube.VerticesPerFace;
                        }
                        if ( !right )
                        {
                            bufferData.AddRange( Cube.CreateFaceData( block.Position, CubeFace.Right, _world.SpriteSheet, block.Type ) );
                            _triangleCount += Cube.TrianglesPerFace;
                            _vertexCount += Cube.VerticesPerFace;
                        }
                        if ( !front )
                        {
                            bufferData.AddRange( Cube.CreateFaceData( block.Position, CubeFace.Front, _world.SpriteSheet, block.Type ) );
                            _triangleCount += Cube.TrianglesPerFace;
                            _vertexCount += Cube.VerticesPerFace;
                        }
                        if ( !back )
                        {
                            bufferData.AddRange( Cube.CreateFaceData( block.Position, CubeFace.Back, _world.SpriteSheet, block.Type ) );
                            _triangleCount += Cube.TrianglesPerFace;
                            _vertexCount += Cube.VerticesPerFace;
                        }
                    }
                }
            }

            // set our vertex count and create the buffer
            if ( _buffer != null )
            {
                _buffer.Dispose();
            }
            if ( _vertexCount > 0 )
            {
                _buffer = new VertexBuffer( _world.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, _vertexCount, BufferUsage.None );
                _buffer.SetData<VertexPositionNormalTexture>( bufferData.ToArray() );
            }
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
            Vector3 coords = _world.LocalToWorld( _position, x, y, z );
            return _world.TerrainGenerator.GetBlockType( coords ) == BlockType.Air;
        }

        /// <summary>
        /// Disposes of this chunk.
        /// </summary>
        public void Dispose()
        {
            if ( _buffer != null )
            {
                _buffer.Dispose();
                _buffer = null;
            }
        }

        /// <summary>
        /// Checks to see if a bounding box collides with this chunk.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        public bool Collides( BoundingBox box )
        {
            return _octree.Collides( box );
        }

        /// <summary>
        /// Unloads the chunk's data from the graphics card.
        /// </summary>
        public void Unload()
        {
            _triangleCount = 0;
            _vertexCount = 0;
            _octree.Clear();
            if ( _buffer != null )
            {
                _buffer.Dispose();
                _buffer = null;

                Terminal.WriteLine( Color.Cyan, 1.5, "Unloaded chunk @ [{0},{1},{2}]", _position.X, _position.Y, _position.Z );
            }
        }

        /// <summary>
        /// Reloads the chunk's data into the graphics card.
        /// </summary>
        public void Reload()
        {
            if ( _buffer == null )
            {
                BuildVoxelBuffer();

                Terminal.WriteLine( Color.Cyan, 1.5, "Reloaded chunk @ [{0},{1},{2}]", _position.X, _position.Y, _position.Z );
            }
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="effect">The effect to use to draw.</param>
        public void Draw( BaseEffect effect )
        {
            // if we have a buffer, then attach it to the graphics device and draw
            if ( _buffer != null && _triangleCount > 0 )
            {
                // draw our vertex buffer
                _world.GraphicsDevice.SetVertexBuffer( _buffer );
                foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
                {
                    pass.Apply();
                    _world.GraphicsDevice.DrawPrimitives( PrimitiveType.TriangleList, 0, _triangleCount );
                }
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
            // create our helper reader and make sure we find the chunk's magic number
            BinaryReader bin = new BinaryReader( stream );
            if ( bin.ReadInt32() != MagicNumber )
            {
                Terminal.WriteLine( Color.Red, 3.0, "Encountered invalid chunk in stream." );
                return null;
            }

            // now try to read the chunk
            try
            {
                Chunk chunk = new Chunk( bin, world );
                return chunk;
            }
            catch ( Exception ex )
            {
                Terminal.WriteLine( Color.Red, 3.0, "Failed to load chunk." );
                Terminal.WriteLine( Color.Red, 3.0, "-- {0}", ex.Message );
                // Terminal.WriteLine( ex.StackTrace );

                return null;
            }
        }
    }
}