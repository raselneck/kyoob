using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob
{
    /// <summary>
    /// A data structure containing a chunk of blocks.
    /// </summary>
    public sealed class Chunk
    {
        private Block[ , , ] _blocks;
        private Vector3 _position;

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="device">The graphics device to be created on.</param>
        /// <param name="position">The chunk's position (the center of the chunk).</param>
        public Chunk( GraphicsDevice device, Vector3 position )
        {
            _blocks = new Block[ 16, 16, 16 ];
            _position = position;

            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        _blocks[ x, y, z ] = new Block( device, new Vector3(
                            _position.X + ( x - 8 ),
                            _position.Y + ( y - 8 ),
                            _position.Z + ( z - 8 )
                        ) );
                    }
                }
            }
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="device">The device to draw to.</param>
        /// <param name="effect">The effect to use to draw.</param>
        /// <param name="camera">The camera to use for culling.</param>
        public void Draw( GraphicsDevice device, Effect effect, Camera camera )
        {
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        if ( camera.CanSee( _blocks[ x, y, z ].Bounds ) )
                        {
                            _blocks[ x, y, z ].Draw( device, effect );
                        }
                    }
                }
            }
        }
    }
}