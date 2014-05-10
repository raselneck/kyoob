using System;
using Microsoft.Xna.Framework;
using Kyoob.Blocks;

#warning TODO : Improve this so it actually looks like terrain.

namespace Kyoob.Terrain
{
    /// <summary>
    /// A simple perlin noise terrain generator.
    /// </summary>
    public class PerlinTerrain : TerrainGenerator
    {
        private LibNoise.Perlin _noise;

        /// <summary>
        /// Gets or sets the seed for this Perlin terrain generator.
        /// </summary>
        public override int Seed
        {
            get
            {
                return _noise.Seed;
            }
            set
            {
                base.Seed = value;
                _noise.Seed = value;
            }
        }

        /// <summary>
        /// Creates a new Perlin terrain generator.
        /// </summary>
        /// <param name="seed"></param>
        public PerlinTerrain( int seed )
            : base( seed )
        {
            _noise = new LibNoise.Perlin();
            _noise.Seed = seed;
        }

        /// <summary>
        /// Gets the block type for the given world coordinates.
        /// </summary>
        /// <param name="position">The position in the world.</param>
        /// <returns></returns>
        public override BlockType GetBlockType( Vector3 position )
        {
            position /= 23.0f;
            double value = _noise.GetValue( position.X, position.Y, position.Z );
            value = Math.Abs( value );

            // just some arbitrary values
            BlockType type = BlockType.Air;
            if ( value >= 0.60 )
            {
                type = BlockType.Dirt;
            }
            else if ( value >= 0.40 && value < 0.50 )
            {
                type = BlockType.Stone;
            }
            else if ( value >= 0.50 && value < 0.60 )
            {
                type = BlockType.Sand;
            }

            return type;
        }
    }
}