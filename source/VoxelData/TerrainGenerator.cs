using System;
using Microsoft.Xna.Framework;
using LibNoise;

// TODO : Rename to terrain?

namespace Kyoob.VoxelData
{
    /// <summary>
    /// The terrain generator.
    /// </summary>
    public sealed class TerrainGenerator
    {
        /// <summary>
        /// The maximum height an entity is allowed to go to.
        /// </summary>
        public const float MaxEntityHeight = 256.0f;

        private static TerrainGenerator _instance;
        private TerrainLevels _levels;
        private BlockType[,,] _currentBlocks;
        private Perlin _noise;
        private float _hBias;
        private float _vBias;

        /// <summary>
        /// Gets the singleton terrain generator instance.
        /// </summary>
        public static TerrainGenerator Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new TerrainGenerator();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the seed for the seed for the terrain generator.
        /// </summary>
        public int Seed
        {
            get
            {
                return _noise.Seed;
            }
            set
            {
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
                HighestPoint = 1.0f / _vBias;
            }
        }

        /// <summary>
        /// Gets or sets whether or not to invert the generated Perlin noise values.
        /// </summary>
        public bool Invert
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the highest point this Perlin terrain generator supports.
        /// </summary>
        public float HighestPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the current octave.
        /// </summary>
        public int OctaveCount
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
        private TerrainGenerator()
        {
            _levels = new TerrainLevels();
            _currentBlocks = new BlockType[ ChunkData.Size + 2, ChunkData.Size + 2, ChunkData.Size + 2 ];

            _noise = new Perlin();

            HorizontalBias = 1.0f / 57; // prime numbers work best
            VerticalBias = 1.0f / 24;   // use property to also set highest point
        }

        /// <summary>
        /// Sets the current chunk that is having its terrain generated.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        public void SetCurrentChunk( Chunk chunk )
        {
            const int max = ChunkData.Size + 2;
            Vector3 world;
            for ( int x = 0; x < max; ++x )
            {
                for ( int y = 0; y < max; ++y )
                {
                    for ( int z = 0; z < max; ++z )
                    {
                        Position.LocalToWorld( chunk.Center, x - 1, y - 1, z - 1, out world );
                        _currentBlocks[ x, y, z ] = GetBlockType( world );
                    }
                }
            }
        }

        /// <summary>
        /// Queries the terrain chunk for the generated block type. Each coordinate
        /// can be in the range [-1,ChunkData.Size+1].
        /// </summary>
        /// <param name="localX">The local X coordinate for the current chunk.</param>
        /// <param name="localY">The local Y coordinate for the current chunk.</param>
        /// <param name="localZ">The local Z coordinate for the current chunk.</param>
        /// <returns></returns>
        public BlockType Query( int localX, int localY, int localZ )
        {
            const int max = ChunkData.Size + 1;
            if ( localX < -1 || localX > max || localY < -1 || localY > max || localZ < -1 || localZ > max )
            {
                throw new ArgumentOutOfRangeException();
            }
            return _currentBlocks[ localX + 1, localY + 1, localZ + 1 ];
        }

        /// <summary>
        /// Gets the block type for the given position.
        /// </summary>
        /// <param name="world">The world coordinates of the block type.</param>
        public BlockType GetBlockType( Vector3 world )
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
            if ( Invert )
            {
                noise = 1.0f - noise;
            }

            // get the block type based on the noise value
            if ( world.Y <= noise )
            {
                return Levels.GetType( world.Y );
                //return Levels.GetType( noise );
            }

            if ( world.Y <= Levels.WaterLevel )
            {
                return BlockType.Water;
            }
            return BlockType.Air;
        }
    }
}