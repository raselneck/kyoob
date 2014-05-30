using System;
using Microsoft.Xna.Framework;
using Kyoob.Blocks;

namespace Kyoob.Terrain
{
    /// <summary>
    /// A base class for terrain generators.
    /// </summary>
    public abstract class TerrainGenerator
    {
        private int _seed;
        private TerrainLevels _levels;
        private World _world;

        /// <summary>
        /// Gets or sets the seed for the seed for the terrain generator.
        /// </summary>
        public virtual int Seed
        {
            get
            {
                return _seed;
            }
            set
            {
                _seed = value;
            }
        }

        /// <summary>
        /// Gets the terrain level data used by this terrain generator.
        /// </summary>
        public TerrainLevels Levels
        {
            get
            {
                return _levels;
            }
        }

        /// <summary>
        /// Gets or sets the world this terrain generator is for.
        /// </summary>
        public World World
        {
            get
            {
                return _world;
            }
            set
            {
                _world = value;
            }
        }

        /// <summary>
        /// Creates a new terrain generator.
        /// </summary>
        /// <param name="seed">The generator's seed.</param>
        public TerrainGenerator( int seed )
        {
            _seed = seed;
            _levels = new TerrainLevels();
        }

        /// <summary>
        /// Generates chunk block data that is Chunk.Size + 2 in each direction.
        /// </summary>
        /// <param name="chunk">The chunk to generate data for.</param>
        /// <returns></returns>
        public virtual BlockType[,,] GenerateChunkData( Chunk chunk )
        {
            // make sure we have a world
            if ( _world == null )
            {
                throw new NullReferenceException( "The world must be set." );
            }

            BlockType[,,] blocks = new BlockType[ Chunk.Size, Chunk.Size, Chunk.Size ];

            for ( int x = 0; x < Chunk.Size; ++x )
            {
                for ( int y = 0; y < Chunk.Size; ++y )
                {
                    for ( int z = 0; z < Chunk.Size; ++z )
                    {
                        Vector3 world = _world.LocalToWorld( chunk.Center, x, y, z );
                        blocks[ x, y, z ] = GetBlockType( world );
                    }
                }
            }

            return blocks;
        }

        /// <summary>
        /// Gets the block type for the given position.
        /// </summary>
        /// <param name="world">The world coordinates of the block type.</param>
        public abstract BlockType GetBlockType( Vector3 world );
    }
}