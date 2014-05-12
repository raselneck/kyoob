using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#pragma warning disable 1587 // disable "invalid XML comment placement"

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
        private VoxelBuffer _terrainBuff;
        private VoxelBuffer _waterBuff;

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
                new Vector3( _position.X - Size / 2 - 0.5f, _position.Y - Size / 2 - 0.5f, _position.Z - Size / 2 - 0.5f ),
                new Vector3( _position.X + Size / 2 - 0.5f, _position.Y + Size / 2 - 0.5f, _position.Z + Size / 2 - 0.5f )
            );
            _octree = new Octree<Block>( _bounds );
            _terrainBuff = new VoxelBuffer();
            _waterBuff = new VoxelBuffer();

            // tell the terrain generator to generate data for this chunk
            _world.TerrainGenerator.CurrentChunk = this;

            // create blocks
            for ( int x = 0; x < Size; ++x )
            {
                for ( int y = 0; y < Size; ++y )
                {
                    for ( int z = 0; z < Size; ++z )
                    {
                        // get block data
                        Vector3 coords = _world.LocalToWorld( _position, x, y, z );
                        BlockType type = _world.TerrainGenerator.GetBlockType( coords );
                        _blocks[ x, y, z ] = new Block( coords, type );
                    }
                }
            }

            // build the voxel buffer and octree
            BuildVoxelBuffers();
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
            _terrainBuff = new VoxelBuffer();
            _waterBuff = new VoxelBuffer();

            // read position
            _position = new Vector3(
                bin.ReadSingle(),
                bin.ReadSingle(),
                bin.ReadSingle()
            );

            // create bounds and octree
            _bounds = new BoundingBox(
                new Vector3( _position.X - Size / 2 - 0.5f, _position.Y - Size / 2 - 0.5f, _position.Z - Size / 2 - 0.5f ),
                new Vector3( _position.X + Size / 2 - 0.5f, _position.Y + Size / 2 - 0.5f, _position.Z + Size / 2 - 0.5f )
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
            BuildVoxelBuffers();
        }

        /// <summary>
        /// Builds the voxel buffers.
        /// </summary>
        private void BuildVoxelBuffers()
        {
            /**
             * What we need to do is go through all of our blocks and determine which are exposed at all
             * and which are completely hidden. If they have any exposed sides, then we need to use that
             * face's data for constructing our vertex buffer.
             */

            // clear out everything that needs to be cleared
            _terrainBuff.Clear();
            _terrainBuff.Dispose();
            _waterBuff.Clear();
            _waterBuff.Dispose();
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

                        // add the block to the octree because it's active
                        _octree.Add( block );

                        // if the type is dirt and there's nothing on top, then the block should be grass.
                        if ( block.Type == BlockType.Dirt && GetBlockType( x, y + 1, z ) == BlockType.Air )
                        {
                            block.Type = BlockType.Grass;
                        }

                        /**
                         * Now we need to check which directions are empty for the block.
                         */

                        // check above
                        BlockType type = GetBlockType( x, y + 1, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Top, _world.SpriteSheet, block.Type ) );
                        }
                        if ( block.Type == BlockType.Water && type == BlockType.Air )
                        {
                            _waterBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Top, _world.SpriteSheet, block.Type ) );
                        }

                        // check below
                        type = GetBlockType( x, y - 1, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Bottom, _world.SpriteSheet, block.Type ) );
                        }

                        // check to the left
                        type = GetBlockType( x - 1, y, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Left, _world.SpriteSheet, block.Type ) );
                        }

                        // check to the right
                        type = GetBlockType( x + 1, y, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Right, _world.SpriteSheet, block.Type ) );
                        }

                        // check in front
                        type = GetBlockType( x, y, z - 1 );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Front, _world.SpriteSheet, block.Type ) );
                        }

                        // check in back
                        type = GetBlockType( x, y, z + 1 );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Back, _world.SpriteSheet, block.Type ) );
                        }
                    }
                }
            }

            // set our vertex count and create the buffer
            _terrainBuff.Compile( _world.GraphicsDevice );
            _waterBuff.Compile( _world.GraphicsDevice );
        }

        /// <summary>
        /// Checks to see if a block type is an empty block type.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <returns></returns>
        private bool IsEmptyBlockType( BlockType type )
        {
            return type == BlockType.Air
                || type == BlockType.Water;
        }

        /// <summary>
        /// Gets the block type in the world relative to the given local coordinates.
        /// </summary>
        /// <param name="x">The X index of the block to check.</param>
        /// <param name="y">The Y index of the block to check.</param>
        /// <param name="z">The Z index of the block to check.</param>
        private BlockType GetBlockType( int x, int y, int z )
        {
            // get the world block type
            Vector3 coords = _world.LocalToWorld( _position, x, y, z );
            return _world.TerrainGenerator.GetBlockType( coords );
        }

        /// <summary>
        /// Disposes of this chunk.
        /// </summary>
        public void Dispose()
        {
            _terrainBuff.Dispose();
            _waterBuff.Dispose();
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
            _octree.Clear();
            _terrainBuff.Dispose();
            _waterBuff.Dispose();

            Terminal.WriteLine( Color.Cyan, 1.5, "Unloaded chunk @ [{0},{1},{2}]", _position.X, _position.Y, _position.Z );
        }

        /// <summary>
        /// Reloads the chunk's data into the graphics card.
        /// </summary>
        public void Reload()
        {
            //_terrainBuff.Compile( _world.GraphicsDevice );
            //_waterBuff.Compile( _world.GraphicsDevice );

            BuildVoxelBuffers();

            Terminal.WriteLine( Color.Cyan, 1.5, "Reloaded chunk @ [{0},{1},{2}]", _position.X, _position.Y, _position.Z );
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="renderer">The renderer to draw with.</param>
        public void Draw( EffectRenderer renderer )
        {
            renderer.QueueSolid( _terrainBuff );
            renderer.QueueAlpha( _waterBuff );
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