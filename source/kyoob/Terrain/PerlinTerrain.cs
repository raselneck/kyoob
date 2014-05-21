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
        private float _hBias;
        private float _vBias;

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

            _hBias = 1.0f / 57; // prime numbers work best
            _vBias = 1.0f / 24;

            ChunkChanged += OnChunkChanged;
        }

        /// <summary>
        /// Handles when the chunk is changed.
        /// </summary>
        /// <param name="chunk">The new chunk.</param>
        protected virtual void OnChunkChanged( Chunk chunk )
        {

        }

        /// <summary>
        /// Gets the block type for the given world coordinates.
        /// </summary>
        /// <param name="x">The local X coordinate.</param>
        /// <param name="y">The local Y coordinate.</param>
        /// <param name="z">The local Z coordinate.</param>
        public override BlockType GetBlockType( int x, int y, int z )
        {
            // convert local coordinates to world coordinates
            Vector3 world = CurrentChunk.LocalToWorld( x, y, z );

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
            noise = 1.0f - noise;

            // tbh not entirely sure why this works
            if ( world.Y <= noise )
            {
                return Levels.GetType( world.Y );
            }
            if ( world.Y <= Levels.WaterLevel )
            {
                return BlockType.Water;
            }
            return BlockType.Air;
        }
    }
}