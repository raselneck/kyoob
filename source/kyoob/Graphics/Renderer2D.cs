using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XnaGame = Microsoft.Xna.Framework.Game;

namespace Kyoob.Graphics
{
    /// <summary>
    /// A static 2D rendering class.
    /// </summary>
    public static class Renderer2D
    {
        private static GraphicsDevice _device;
        private static DepthStencilState _depthState;
        private static SpriteBatch _spriteBatch;
        private static bool _hasBegun;

        /// <summary>
        /// Static initialization of the 2D renderer.
        /// </summary>
        static Renderer2D()
        {
            _hasBegun = false;
        }

        /// <summary>
        /// Initializes the 2D renderer based on the given game.
        /// </summary>
        /// <param name="game">The game.</param>
        public static void Initialize( XnaGame game )
        {
            _device = game.GraphicsDevice;
            _depthState = _device.DepthStencilState;
            _spriteBatch = new SpriteBatch( _device );
        }

        /// <summary>
        /// Begins the 2D renderer.
        /// </summary>
        public static void Begin()
        {
            if ( !_hasBegun )
            {
                _spriteBatch.Begin();
                _hasBegun = true;
            }
        }

        /// <summary>
        /// Draws a string to the screen.
        /// </summary>
        /// <param name="font">The font.</param>
        /// <param name="text">The text.</param>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public static void DrawString( SpriteFont font, string text, Vector2 position, Color color )
        {
            if ( _hasBegun )
            {
                _spriteBatch.DrawString( font, text, position, color );
            }
        }

        /// <summary>
        /// Ends the 2D renderer.
        /// </summary>
        public static void End()
        {
            if ( _hasBegun )
            {
                _spriteBatch.End();
                _device.DepthStencilState = _depthState;
                _hasBegun = false;
            }
        }
    }
}