using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using XnaGame = Microsoft.Xna.Framework.Game;

namespace Kyoob
{
    /// <summary>
    /// A frame counter component for Kyoob.
    /// </summary>
    public sealed class FrameCounter : GameComponent
    {
        private int _fps;
        private int _frameCount;
        private double _tickCount;

        /// <summary>
        /// Gets the current FPS.
        /// </summary>
        public int FPS
        {
            get
            {
                return _fps;
            }
        }

        /// <summary>
        /// Creates a new frame counter component.
        /// </summary>
        /// <param name="game">The game to monitor.</param>
        public FrameCounter( XnaGame game )
            : base( game )
        {
            _fps = 60;
            _frameCount = 0;
            _tickCount = 0.0;
        }

        /// <summary>
        /// Updates the frame counter.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update( GameTime gameTime )
        {
            ++_frameCount;
            _tickCount += gameTime.ElapsedGameTime.TotalSeconds;

            if ( _tickCount >= 1.0 )
            {
                _fps = _frameCount;
                _frameCount = 0;
                _tickCount -= 1.0;

                // if we're running slowly, say something
                if ( _fps < 60 )
                {
                    Debug.WriteLine( "Running slowly! (" + _fps + " FPS)" );
                }
            }

            base.Update( gameTime );
        }
    }
}