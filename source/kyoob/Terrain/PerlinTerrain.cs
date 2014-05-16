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
        private TerrainPlane _plane;
        private float[ , ] _heightMap;
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

            _plane = new TerrainPlane( _noise );

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

            _plane.SetBounds( lowerX, upperX, lowerZ, upperZ );
            _heightMap = _plane.GenerateHeightMap( Chunk.Size + 2, Chunk.Size + 2 );
        }

        /// <summary>
        /// Gets the block type for the given world coordinates.
        /// </summary>
        /// <param name="x">The local X coordinate.</param>
        /// <param name="y">The local Y coordinate.</param>
        /// <param name="z">The local Z coordinate.</param>
        public override BlockType GetBlockType( int x, int y, int z )
        {
            // get the local coordinates and then get the designated height value
            Vector3 world = CurrentChunk.World.LocalToWorld( CurrentChunk.Center, x, y, z );
            float value = _heightMap[ x + 1, z + 1 ];
            value = MathHelper.Clamp( value, -1.0f, 1.0f ) / 2.0f + 0.5f; // value will now be in [0,1] range
            value *= _vBias;
            // value *= _vBias / ( position.Y / _vBias );

            // get the actual block type
            BlockType type = BlockType.Air;
            if ( world.Y == 0 )
            {
                // we want bedrock to pad the bottom of the world
                type = BlockType.Bedrock;
            }
            // y = 0 is the absolute minimum (which is bedrock), so we need to check above it
            else if ( world.Y > 0 )
            {
                // only change the type if the world height is less than the noise height
                if ( world.Y < value )
                {
                    type = Levels.GetTypeForLevel( world.Y );
                }

                // if the type should still be air and we're below the water level, then the type should be water
                if ( type == BlockType.Air && world.Y <= Levels.WaterLevel )
                {
                    type = BlockType.Water;
                }
            }
            return type;
        }
    }
}