using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Debug;
using Kyoob.Effects;
using Kyoob.Graphics;

#pragma warning disable 1587 // disable "invalid XML comment placement"

namespace Kyoob.Blocks
{
    /// <summary>
    /// A data structure containing a chunk of blocks.
    /// </summary>
    public sealed class Chunk : IDisposable
    {
        /// <summary>
        /// The size of each chunk. (Where each chunk is Size*Size*Size.)
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
        /// Checks to see if the chunk is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                lock ( _terrainBuff )
                {
                    lock ( _waterBuff )
                    {
                        return _terrainBuff.IsOnGPU && _waterBuff.IsOnGPU;
                    }
                }
                
            }
        }

        /// <summary>
        /// Gets this chunk's blocks.
        /// </summary>
        public Block[,,] Blocks
        {
            get
            {
                return _blocks;
            }
        }

        /// <summary>
        /// Gets this chunk's octree.
        /// </summary>
        public Octree<Block> Octree
        {
            get
            {
                return _octree;
            }
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="world">The world this chunk is in.</param>
        /// <param name="position">The chunk's position (the center of the chunk).</param>
        public Chunk( World world, Vector3 position )
        {
            CommonInitialization( world, position );

            // tell the terrain generator to generate data for this chunk
            BlockType[,,] types = _world.TerrainGenerator.GenerateChunkData( this );

            // create blocks
            for ( int x = 0; x < Size; ++x )
            {
                for ( int y = 0; y < Size; ++y )
                {
                    for ( int z = 0; z < Size; ++z )
                    {
                        // get block data
                        Vector3 coords = _world.LocalToWorld( _position, x, y, z );
                        BlockType type = types[ x, y, z ];
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
            // read position
            Vector3 position = new Vector3(
                bin.ReadSingle(),
                bin.ReadSingle(),
                bin.ReadSingle()
            );

            CommonInitialization( world, position );

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
        /// Performs common chunk initialization.
        /// </summary>
        /// <param name="world">The chunk's world.</param>
        /// <param name="position">The chunk's position.</param>
        private void CommonInitialization( World world, Vector3 position )
        {
            _world = world;
            _blocks = new Block[ Size, Size, Size ];
            _position = position;
            _bounds = new BoundingBox(
                new Vector3(
                    _position.X - Size / 2 - Cube.Size / 2,
                    _position.Y - Size / 2 - Cube.Size / 2,
                    _position.Z - Size / 2 - Cube.Size / 2
                ),
                new Vector3(
                    _position.X + Size / 2 - Cube.Size / 2,
                    _position.Y + Size / 2 - Cube.Size / 2,
                    _position.Z + Size / 2 - Cube.Size / 2
                )
            );
            _octree = new Octree<Block>( _bounds );
            _terrainBuff = new VoxelBuffer();
            _waterBuff = new VoxelBuffer();
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
            _waterBuff.Clear();
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

                        // if the type is dirt and there's nothing on top, then the block should be grass.
                        if ( block.Type == BlockType.Dirt && GetBlockType( x, y + 1, z ) == BlockType.Air )
                        {
                            block.Type = BlockType.Grass;
                        }

                        /**
                         * Now we need to check which directions are empty for the block.
                         */

                        bool exposed = false;

                        // check above
                        BlockType type = GetBlockType( x, y + 1, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Top, _world.SpriteSheet, block.Type ) );
                            exposed = true;
                        }
                        // only do the tops of water
                        if ( block.Type == BlockType.Water && type == BlockType.Air )
                        {
                            _waterBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Top, _world.SpriteSheet, block.Type ) );
                        }

                        // check below
                        type = GetBlockType( x, y - 1, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Bottom, _world.SpriteSheet, block.Type ) );
                            exposed = true;
                        }

                        // check to the left
                        type = GetBlockType( x - 1, y, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Left, _world.SpriteSheet, block.Type ) );
                            exposed = true;
                        }

                        // check to the right
                        type = GetBlockType( x + 1, y, z );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Right, _world.SpriteSheet, block.Type ) );
                            exposed = true;
                        }

                        // check in front
                        type = GetBlockType( x, y, z - 1 );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Front, _world.SpriteSheet, block.Type ) );
                            exposed = true;
                        }

                        // check in back
                        type = GetBlockType( x, y, z + 1 );
                        if ( IsEmptyBlockType( type ) && !IsEmptyBlockType( block.Type ) )
                        {
                            _terrainBuff.AddFaceData( Cube.CreateFaceData( block.Position, CubeFace.Back, _world.SpriteSheet, block.Type ) );
                            exposed = true;
                        }

                        // add the block to the octree if it's not an empty type and it's exposed
                        if ( !IsEmptyBlockType( block.Type ) && exposed )
                        {
                            _octree.Add( block );
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
            Vector3 pos = _world.LocalToWorld( _position, x, y, z );
            return _world.GetBlockType( pos );
        }

        /// <summary>
        /// Disposes of this chunk.
        /// </summary>
        public void Dispose()
        {
            lock ( _terrainBuff )
            {
                _terrainBuff.Dispose();
            }
            lock ( _waterBuff )
            {
                _waterBuff.Dispose();
            }
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this chunk.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box )
        {
            return _octree.Collides( box );
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this chunk.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <param name="collisions">The list of bounding boxes that the given box collides with.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box, out List<BoundingBox> collisions )
        {
            return _octree.Collides( box, out collisions );
        }

        /// <summary>
        /// Gets the list of blocks that the given bounding box collides with.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public List<Block> GetCollisions( BoundingBox box )
        {
            return _octree.GetCollisions( box );
        }

        /// <summary>
        /// Gets the list of blocks that a ray intersects.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns></returns>
        public Dictionary<Block, float?> GetIntersections( Ray ray )
        {
            return _octree.GetIntersections( ray );
        }

        /// <summary>
        /// Unloads the chunk's data from the graphics card.
        /// </summary>
        public void Unload()
        {
            lock ( _octree )
            {
                _octree.Clear();
            }
            lock ( _terrainBuff )
            {
                _terrainBuff.Dispose();
            }
            lock ( _waterBuff )
            {
                _waterBuff.Dispose();
            }
        }

        /// <summary>
        /// Reloads the chunk's data into the graphics card.
        /// </summary>
        public void Reload()
        {
            //_terrainBuff.Compile( _world.GraphicsDevice );
            //_waterBuff.Compile( _world.GraphicsDevice );

            lock ( _terrainBuff )
            {
                lock ( _waterBuff )
                {
                    lock ( _octree )
                    {
                        if ( !_terrainBuff.IsOnGPU && !_waterBuff.IsOnGPU )
                        {
                            BuildVoxelBuffers();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="renderer">The renderer to draw with.</param>
        public void Draw( EffectRenderer renderer )
        {
            if ( !_terrainBuff.IsEmpty )
            {
                renderer.QueueSolid( _terrainBuff );
            }
            if ( !_waterBuff.IsEmpty )
            {
                renderer.QueueAlpha( _waterBuff );
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
                Terminal.WriteError( "Encountered invalid chunk in stream." );
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
                Terminal.WriteError( "Failed to load chunk." );
                Terminal.WriteError( "-- {0}", ex.Message );
                Terminal.WriteError( ex.StackTrace );

                return null;
            }
        }
    }
}