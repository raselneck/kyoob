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
        /// Creates a new terrain generator.
        /// </summary>
        /// <param name="seed">The generator's seed.</param>
        public TerrainGenerator( int seed )
        {
            _seed = seed;
        }

        /// <summary>
        /// Gets the block type for the given position.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns></returns>
        public abstract BlockType GetBlockType( Vector3 position );
    }
}