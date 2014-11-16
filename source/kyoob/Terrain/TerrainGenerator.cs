using System;
using Kyoob.Blocks;
using Microsoft.Xna.Framework;

namespace Kyoob.Terrain
{
    /// <summary>
    /// A base class for terrain generators.
    /// </summary>
    public abstract class TerrainGenerator
    {
        private int _seed;
        private TerrainLevels _levels;
        private BlockType[,,] _blocks;

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
        /// Gets the highest point this terrain generator can produce.
        /// </summary>
        public abstract float HighestPoint
        {
            get;
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
        /// Creates a new terrain generator.
        /// </summary>
        /// <param name="seed">The generator's seed.</param>
        public TerrainGenerator( int seed )
        {
            _seed = seed;
            _levels = new TerrainLevels();
            _blocks = new BlockType[Chunk.Size + 2, Chunk.Size + 2, Chunk.Size + 2];
        }

        /// <summary>
        /// Generates chunk block data that is Chunk.Size + 2 in each direction.
        /// </summary>
        /// <param name="chunk">The chunk to generate data for.</param>
        /// <returns></returns>
        public virtual BlockType[,,] GenerateChunkData( Chunk chunk )
        {
            for ( int x = 0; x < Chunk.Size + 2; ++x )
            {
                for ( int y = 0; y < Chunk.Size + 2; ++y )
                {
                    for ( int z = 0; z < Chunk.Size + 2; ++z )
                    {
                        Vector3 world = World.LocalToWorld( chunk.Center, x - 1, y - 1, z - 1 );
                        _blocks[ x, y, z ] = GetBlockType( world );
                    }
                }
            }

            return _blocks;
        }

        /// <summary>
        /// Gets the block type for the given position.
        /// </summary>
        /// <param name="world">The world coordinates of the block type.</param>
        public abstract BlockType GetBlockType( Vector3 world );
    }
}