using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob.Blocks
{
    /// <summary>
    /// Creates a new world.
    /// </summary>
    public class World
    {
        /// <summary>
        /// The magic number for worlds. (FourCC = 'WRLD')
        /// </summary>
        private const int MagicNumber = 0x57524C44;

        private Stopwatch _watch;
        private GraphicsDevice _device;
        private BaseEffect _effect;
        private SpriteSheet _spriteSheet;
        private Dictionary<Index3D, Chunk> _chunks;
        private Dictionary<Vector3, double> _noiseCache;
        private LibNoise.Perlin _noise;

        /// <summary>
        /// Gets the graphics device this world is on.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
        }

        /// <summary>
        /// Gets the world's sprite sheet.
        /// </summary>
        public SpriteSheet SpriteSheet
        {
            get
            {
                return _spriteSheet;
            }
        }

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="effect">The base effect.</param>
        /// <param name="spriteSheet">The sprite sheet to use with each cube.</param>
        public World( GraphicsDevice device, BaseEffect effect, SpriteSheet spriteSheet )
        {
            _device = device;
            _effect = effect;
            _spriteSheet = spriteSheet;
            _chunks = new Dictionary<Index3D, Chunk>();
            _noise = new LibNoise.Perlin();
            _noiseCache = new Dictionary<Vector3, double>();

            // add some arbitrary chunks
            for ( int x = -5; x <= 5; ++x )
            {
                for ( int y = -5; y <= 5; ++y )
                {
                    for ( int z = -5; z <= 5; ++z )
                    {
                        CreateChunk( x, y, z );
                    }
                }
            }
        }

        /// <summary>
        /// Creates a chunk with the given indices.
        /// </summary>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        private void CreateChunk( int x, int y, int z )
        {
            // create the chunk index
            Index3D index = new Index3D( x, y, z );

            // make sure we don't already have that chunk created
            if ( _chunks.ContainsKey( index ) )
            {
                return;
            }

            // create the chunk and store it
            Chunk chunk = new Chunk( this, new Vector3(
                x * 8.0f,
                y * 8.0f,
                z * 8.0f
            ) );
            _chunks.Add( index, chunk );
        }

        /// <summary>
        /// Converts a chunk's local coordinates to world coordinates.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        /// <returns></returns>
        public Vector3 ChunkToWorld( Vector3 center, int x, int y, int z )
        {
            return new Vector3(
                center.X + ( x - 8 ),
                center.Y + ( y - 8 ),
                center.Z + ( z - 8 )
            );
        }

        /// <summary>
        /// Gets the block type for world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates.</param>
        /// <returns></returns>
        public BlockType GetBlockType( Vector3 world )
        {
            // modify the world position for noise values
            world /= 17.0f;
            double noise = 0.0;

            // if we have the noise value cached, use that instead of generating the value
            if ( _noiseCache.ContainsKey( world ) )
            {
                noise = _noiseCache[ world ];
            }
            else
            {
                noise = _noise.GetValue( world.X, world.Y, world.Z );
                _noiseCache.Add( world, noise );
            }

            // just some arbitrary values
            BlockType type = BlockType.Air;
            if ( Math.Abs( noise ) >= 0.60 )
            {
                type = BlockType.Dirt;
            }
            else if ( Math.Abs( noise ) >= 0.40 && Math.Abs( noise ) < 0.50 )
            {
                type = BlockType.Stone;
            }
            else if ( Math.Abs( noise ) >= 0.50 && Math.Abs( noise ) < 0.60 )
            {
                type = BlockType.Sand;
            }

            return type;
        }

        /// <summary>
        /// Draws the world.
        /// </summary>
        /// <param name="camera">The current camera to use for getting visible tiles.</param>
        public void Draw( Camera camera )
        {
            _watch = Stopwatch.StartNew();


            // set the sprite sheet texture and draw each chunk
            int count = 0;
            foreach ( Chunk chunk in _chunks.Values )
            {
                if ( !camera.CanSee( chunk.Bounds ) )
                {
                    continue;
                }
                chunk.Draw( _effect );
                ++count;
            }


            _watch.Stop();
            Console.WriteLine( "Draw {0} chunks: {1:0.00}ms", count, _watch.Elapsed.TotalMilliseconds );
        }

        /// <summary>
        /// Saves this world to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void SaveTo( Stream stream )
        {

        }
    }
}