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
        /// <summary>
        /// The delegate callback for when the current chunk is changed.
        /// </summary>
        /// <param name="chunk">The new chunk.</param>
        protected delegate void ChunkChangedDelegate( Chunk chunk );

        private int _seed;
        private Chunk _chunk;
        private TerrainLevels _levels;

        /// <summary>
        /// The event that gets called when the chunk is changed.
        /// </summary>
        protected event ChunkChangedDelegate ChunkChanged;

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
        /// Gets or sets the current chunk.
        /// </summary>
        public Chunk CurrentChunk
        {
            get
            {
                return _chunk;
            }
            set
            {
                if ( _chunk != value )
                {
                    _chunk = value;
                    if ( ChunkChanged != null )
                    {
                        ChunkChanged( _chunk );
                    }
                }
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
        /// Creates a new terrain generator.
        /// </summary>
        /// <param name="seed">The generator's seed.</param>
        public TerrainGenerator( int seed )
        {
            _seed = seed;
            _levels = new TerrainLevels();
        }

        /// <summary>
        /// Gets the block type for the given position.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns></returns>
        public abstract BlockType GetBlockType( Vector3 position );
    }
}