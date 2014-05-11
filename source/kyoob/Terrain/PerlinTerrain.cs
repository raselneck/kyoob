using System;
using Microsoft.Xna.Framework;
using Kyoob.Blocks;
using Kyoob.NoiseUtils;

#pragma warning disable 1587 // disable "invalid XML comment placement"

#warning TODO : Improve this so it actually looks like terrain.

namespace Kyoob.Terrain
{
    /// <summary>
    /// A simple perlin noise terrain generator.
    /// </summary>
    public class PerlinTerrain : TerrainGenerator
    {
        private LibNoise.Perlin _noise;
        private PlaneMapBuilder _builder;
        private float _offset;

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
        /// Creates a new Perlin terrain generator.
        /// </summary>
        /// <param name="seed"></param>
        public PerlinTerrain( int seed )
            : base( seed )
        {
            _noise = new LibNoise.Perlin();
            _noise.Seed = seed;
            _builder = new PlaneMapBuilder();
            // _builder.IsSeamless = true;
            _builder.SourceModule = _noise;

            _offset = 17.0f; // prime numbers work best

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
            float lowerX = ( position.X - Chunk.Size / 2.0f - 1.0f ) / _offset;
            float upperX = ( position.X + Chunk.Size / 2.0f + 1.0f ) / _offset;
            float lowerZ = ( position.Z - Chunk.Size / 2.0f - 1.0f ) / _offset;
            float upperZ = ( position.Z + Chunk.Size / 2.0f + 1.0f ) / _offset;

            _builder.DestinationWidth  = Chunk.Size + 2;
            _builder.DestinationHeight = Chunk.Size + 2;
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
            /**
             * Ranges:
             *  0 - 2  = Stone
             *  3 - 4  = Sand
             *  4 - 9  = Dirt
             */

            Vector3 local = CurrentChunk.World.WorldToLocal( CurrentChunk.Center, position );
            int x = (int)( local.X ) + 1;
            int z = (int)( local.Z ) + 1;
            float value = ( _builder.NoiseMap[ x, z ] + 1.0f ) * 5.0f;

            BlockType type = BlockType.Air;
            if ( position.Y <= value )
            {
                if ( value <= 3.0f )
                {
                    type = BlockType.Stone;
                }
                else if ( value <= 5.0f )
                {
                    type = BlockType.Sand;
                }
                else if ( value <= 9.0f )
                {
                    type = BlockType.Dirt;
                }
            }
            return type;

            /*
            // old, infinite cave-like generation
            position /= 23.0f;
            double value = _noise.GetValue( position.X, position.Y, position.Z );
            value = Math.Abs( value );
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
            */
        }
    }
}