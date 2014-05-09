using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#warning TODO : Need to be able to only render visible chunks.

namespace Kyoob.Blocks
{
    /// <summary>
    /// Creates a new world.
    /// </summary>
    public class World
    {
        private Stopwatch _watch;
        private GraphicsDevice _device;
        private BaseEffect _effect;
        private SpriteSheet _spriteSheet;
        private List<Chunk> _chunks;

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

            // add some arbitrary chunks
            _chunks = new List<Chunk>();
            for ( int x = -2; x <= 2; ++x )
            {
                for ( int z = -2; z <= 2; ++z )
                {
                    _chunks.Add( new Chunk( _device, new Vector3( x * 8.0f, 0.0f, z * 8.0f ), _spriteSheet ) );
                }
            }
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
            ( (TextureEffect)_effect ).Texture = _spriteSheet.Texture;
            for ( int i = 0; i < _chunks.Count; ++i )
            {
                if ( !camera.CanSee( _chunks[ i ].Bounds ) )
                {
                    continue;
                }
                _chunks[ i ].Draw( _effect );
                ++count;
            }


            _watch.Stop();
            Console.WriteLine( "Draw {0} chunks: {1:0.00}ms", count, _watch.Elapsed.TotalMilliseconds );
        }
    }
}