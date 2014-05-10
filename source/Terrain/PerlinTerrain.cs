using System;
using Microsoft.Xna.Framework;
using Kyoob.Blocks;

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
        /// Queries the Perlin noise module.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="height">The height modifier.</param>
        /// <param name="power">The power.</param>
        /// <returns></returns>
        private double QueryNoise( double x, double y, double z, double scale, double height, double power )
        {
            double noise = _noise.GetValue( x / scale, y / scale, z / scale );
            noise *= height;

            if ( power != 0 )
            {
                noise = Math.Pow( noise, power );
            }

            return noise;
        }

        /// <summary>
        /// Gets the block type for the given world coordinates.
        /// </summary>
        /// <param name="position">The position in the world.</param>
        /// <returns></returns>
        public override BlockType GetBlockType( Vector3 position )
        {
            position /= 33.0f;
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