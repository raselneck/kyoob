using System;
using Microsoft.Xna.Framework;
using Kyoob.Blocks;
using Kyoob.NoiseUtils;

#warning TODO : Look into a perlin noise smoothing algorithm.
#warning TODO : Terrain levels don't really work.

namespace Kyoob.Terrain
{
    /// <summary>
    /// A simple perlin noise terrain generator.
    /// </summary>
    public class PerlinTerrain : TerrainGenerator
    {
        private LibNoise.Perlin _noise;
        private PlaneMapBuilder _builder;
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
                _builder.SourceModule = _noise;
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

            _builder = new PlaneMapBuilder();
            _builder.SourceModule = _noise;
            _builder.DestinationWidth = Chunk.Size + 2;
            _builder.DestinationHeight = Chunk.Size + 2;

            _hBias = 17.0f; // prime numbers work best
            _vBias = 10.0f;

            ChunkChanged += OnChunkChanged;
        }

        /// <summary>
        /// Handles when the chunk is changed.
        /// </summary>
        /// <param name="chunk">The new chunk.</param>
        protected virtual void OnChunkChanged( Chunk chunk )
        {
            // need to grow these by 1 for when the chunk checks just outside its bounds
            Vector3 position = chunk.Center;
            float lowerX = ( position.X - Chunk.Size / 2 - 1 ) * _hBias;
            float upperX = ( position.X + Chunk.Size / 2 + 1 ) * _hBias;
            float lowerZ = ( position.Z - Chunk.Size / 2 - 1 ) * _hBias;
            float upperZ = ( position.Z + Chunk.Size / 2 + 1 ) * _hBias;

            _builder.SetBounds( lowerX, upperX, lowerZ, upperZ );
            _builder.Build();
        }

        /// <summary>
        /// Gets the block type for the given world coordinates.
        /// </summary>
        /// <param name="position">The position in the world.</param>
        /// <returns></returns>
        public override BlockType GetBlockType( Vector3 position )
        {
            // get the local coordinates and then get the designated height value
            Vector3 local = CurrentChunk.World.WorldToLocal( CurrentChunk.Center, position );
            int x = (int)( local.X ) + 1;
            int z = (int)( local.Z ) + 1;
            float value = _builder.NoiseMap[ x, z ];
            value = MathHelper.Clamp( value, -1.0f, 1.0f ) / 2.0f + 0.5f; // value will now be in [0,1] range
            value *= _vBias;

            // get the actual block type
            BlockType type = BlockType.Air;
            if ( position.Y == 0 )
            {
                // we want bedrock to pad the bottom of the world
                type = BlockType.Bedrock;
            }
            // y = 0 is the absolute minimum (which is bedrock), so we need to check above it
            else if ( position.Y > 0 )
            {
                /*
                // get the designated block type for this height value
                type = Levels.GetBlockType( value );

                // some hard coding to force anything just below water to be sand
                if ( position.Y < Levels.WaterLevel && ( type == BlockType.Dirt || type == BlockType.Grass ) )
                {
                    type = BlockType.Sand;
                }
                */

                if ( position.Y < value )
                {
                    type = Levels.GetBlockType( value );
                }

                // if the type should still be air and we're below the water level, then the type should be water
                if ( type == BlockType.Air && position.Y <= Levels.WaterLevel )
                {
                    type = BlockType.Water;
                }
            }
            return type;
        }
    }
}