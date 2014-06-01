using Kyoob.Blocks;
using Microsoft.Xna.Framework;

namespace Kyoob.Terrain
{
    /// <summary>
    /// A simple perlin noise terrain generator.
    /// </summary>
    public class PerlinTerrain : TerrainGenerator
    {
        private LibNoise.Perlin _noise;
        private float _hBias;
        private float _vBias;
        private float _maxHeight;
        private bool _invert;

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
        /// Gets or sets the horizontal bias (prime numbers work best).
        /// </summary>
        public float HorizontalBias
        {
            get
            {
                return _hBias;
            }
            set
            {
                _hBias = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical bias.
        /// </summary>
        public float VerticalBias
        {
            get
            {
                return _vBias;
            }
            set
            {
                _vBias = value;
                _maxHeight = 1.0f / _vBias;
            }
        }

        /// <summary>
        /// Gets or sets whether or not to invert the generated Perlin noise values.
        /// </summary>
        public bool Invert
        {
            get
            {
                return _invert;
            }
            set
            {
                _invert = value;
            }
        }

        /// <summary>
        /// Gets the highest point this Perlin terrain generator supports.
        /// </summary>
        public override float HighestPoint
        {
            get
            {
                return _maxHeight;
            }
        }

        /// <summary>
        /// Gets or sets the current octave.
        /// </summary>
        public int Octave
        {
            get
            {
                return _noise.OctaveCount;
            }
            set
            {
                _noise.OctaveCount = value;
            }
        }

        /// <summary>
        /// Creates a new Perlin terrain generator.
        /// </summary>
        /// <param name="seed">The Perlin terrain generator's seed.</param>
        public PerlinTerrain( int seed )
            : base( seed )
        {
            _noise = new LibNoise.Perlin();
            _noise.Seed = seed;

            HorizontalBias = 1.0f / 57; // prime numbers work best
            VerticalBias   = 1.0f / 24; // use property to set max height
        }

        /// <summary>
        /// Gets the block type for the given world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates of the block type.</param>
        public override BlockType GetBlockType( Vector3 world )
        {
            // we don't want any "destructible" blocks at or below 0
            if ( world.Y == 0.0f )
            {
                return BlockType.Bedrock;
            }
            else if ( world.Y < 0.0f )
            {
                return BlockType.Air;
            }


            // modify world coordinates
            world.X *= _hBias;
            world.Y *= _vBias;
            world.Z *= _hBias;


            // get noise value
            float noise = (float)_noise.GetValue( world.X, world.Y, world.Z );
            noise = MathHelper.Clamp( noise, -1.0f, 1.0f ) / 2.0f + 0.5f;
            if ( _invert )
            {
                noise = 1.0f - noise;
            }

            // get the block type based on the noise value
            if ( world.Y <= noise )
            {
                //return Levels.GetType( world.Y );
                return Levels.GetType( noise );
            }

            if ( world.Y <= Levels.WaterLevel )
            {
                return BlockType.Water;
            }
            return BlockType.Air;
        }
    }
}