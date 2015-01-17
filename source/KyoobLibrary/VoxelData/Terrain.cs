using System;
using Microsoft.Xna.Framework;
using LibNoise;
using LibNoise.Modifiers;

// NOTE: The following links were extremely helpful:
// * http://www.gamedev.net/blog/33/entry-2227887-more-on-minecraft-type-world-gen/
// * http://www.gamedev.net/blog/33/entry-2249106-more-procedural-voxel-world-generation/
// * http://www.gamedev.net/blog/33/entry-2249114-experimental-framework/
// * http://sourceforge.net/projects/accidentalnoise/

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains terrain information.
    /// </summary>
    public sealed class Terrain
    {
        /// <summary>
        /// The maximum height an entity is allowed to go to.
        /// </summary>
        public const float MaxEntityHeight = 96.0f;

        /// <summary>
        /// Gets the maximum height that this terrain will generate.
        /// </summary>
        public const float MaximumHeight = 96.0f;

        /// <summary>
        /// Gets 1/4 of the maximum height that this terrain will generate.
        /// </summary>
        public const float MaximumHeight_1_4 = MaximumHeight * 0.25f;

        /// <summary>
        /// Gets 1/2 of the maximum height that this terrain will generate.
        /// </summary>
        public const float MaximumHeight_1_2 = MaximumHeight * 0.5f;

        /// <summary>
        /// Gets 3/4 of the maximum height that this terrain will generate.
        /// </summary>
        public const float MaximumHeight_3_4 = MaximumHeight * 0.75f;

        /// <summary>
        /// Gets the water level.
        /// </summary>
        public const float WaterLevel = 0.0f;

        private static Terrain _instance;
        private TerrainDensityCollection _densities;
        private BlockType[,,] _currentBlocks;
        private IModule _noiseModule;
        private Perlin _perlin;
        private readonly int _noiseSeed;
        private float _hBias;
        private float _vBias;

        /// <summary>
        /// Gets the singleton terrain instance.
        /// </summary>
        public static Terrain Instance
        {
            get
            {
                return CreateInstance( (int)DateTime.Now.Ticks );
            }
        }

        /// <summary>
        /// Gets the seed used to generate terrain.
        /// </summary>
        public int Seed
        {
            get
            {
                return _noiseSeed;
            }
        }

        /// <summary>
        /// Gets the collection of terrain densities used by this terrain.
        /// </summary>
        public TerrainDensityCollection Densities
        {
            get
            {
                return _densities;
            }
        }

        /// <summary>
        /// Creates a new terrain object.
        /// </summary>
        /// <param name="seed">The seed to use for terrain generation.</param>
        private Terrain( int seed )
        {
            _densities = new TerrainDensityCollection();
            _currentBlocks = new BlockType[ ChunkData.SizeXZ + 2, ChunkData.SizeY, ChunkData.SizeXZ + 2 ];

            _noiseSeed = seed;
            _noiseModule = CreateNoiseModule( _noiseSeed );
            _perlin = new Perlin() { Seed = _noiseSeed + 1 };

            _hBias = 1.0f / 127;
            _vBias = 1.0f / 51;
        }

        /// <summary>
        /// Creates the terrain instance if it does not already exist.
        /// </summary>
        /// <param name="seed">The terrain seed.</param>
        /// <returns></returns>
        public static Terrain CreateInstance( int seed )
        {
            if ( _instance == null )
            {
                _instance = new Terrain( seed );
            }
            return _instance;
        }

        /// <summary>
        /// Sets the current chunk that is having its terrain generated.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        public void SetCurrentChunk( Chunk chunk )
        {
            const int max = ChunkData.SizeXZ + 2;
            Vector3 world;
            for ( int x = 0; x < max; ++x )
            {
                for ( int y = 0; y < ChunkData.SizeY; ++y )
                {
                    for ( int z = 0; z < max; ++z )
                    {
                        Position.LocalToWorld( chunk.Index, x - 1, y, z - 1, out world );
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
            // below chunks is void, above is air
            if ( localY < 0 )
            {
                return BlockType.Void;
            }
            if ( localY >= ChunkData.SizeY )
            {
                return BlockType.Air;
            }

            // ensure we're within given XZ bounds
            const int max = ChunkData.SizeXZ + 1;
            if ( localX < -1 || localX > max ||
                 localZ < -1 || localZ > max )
            {
                throw new ArgumentOutOfRangeException();
            }

            // return the block
            return _currentBlocks[ localX + 1, localY, localZ + 1 ];
        }

        /// <summary>
        /// Gets the block type for the given position.
        /// </summary>
        /// <param name="world">The world coordinates of the block type.</param>
        private BlockType GetBlockType( Vector3 world )
        {
            // we don't want any "destructible" blocks at or below 0
            if ( world.Y == 0.0f )
            {
                return BlockType.Bedrock;
            }
            else if ( world.Y < 0.0f )
            {
                return BlockType.Void;
            }
            
            
            // modify world coordinates
            world.Y -= 10.0f; // skew the height to avoid showing bedrock
            world.X *= _hBias;
            world.Y *= _vBias;
            world.Z *= _hBias;
            
            
            // get noise value
            float noise = (float)_noiseModule.GetValue( world.X, world.Y, world.Z );
            noise = MathHelper.Clamp( noise, -1.0f, 1.0f ) / 2.0f + 0.5f;
            noise = 1.0f - noise;
            
            // get the block type based on the noise value
            if ( world.Y <= noise )
            {
                //return Levels.GetType( world.Y );
                return _densities.FindBlockType( noise );
            }
            
            if ( world.Y <= WaterLevel )
            {
                return BlockType.Water;
            }
            return BlockType.Air;
        }

        /// <summary>
        /// Creates the base noise module to be used for generating terrain.
        /// </summary>
        private static IModule CreateNoiseModule( int seed )
        {
            var landBase = new Perlin()
            {
                Seed = seed,
                OctaveCount = 4
            };
            var other = new Perlin()
            {
                Seed = seed - 1,
                OctaveCount = 2
            };
            return new Multiply( landBase, other );
        }
    }
}