using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob
{
    /// <summary>
    /// The "console" that will be drawn on screen.
    /// </summary>
    public static class Terminal
    {
        private static SpriteFont _font;
        private static Color _fontColor;

        /// <summary>
        /// Gets or sets the terminal's font.
        /// </summary>
        public static SpriteFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
            }
        }

        /// <summary>
        /// Gets or sets the terminal's font color.
        /// </summary>
        public static Color FontColor
        {
            get
            {
                return _fontColor;
            }
            set
            {
                _fontColor = value;
            }
        }

        /// <summary>
        /// Static terminal initialization.
        /// </summary>
        static Terminal()
        {
            _font = null;
            _fontColor = Color.White;
        }

        /// <summary>
        /// Updates the terminal.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public static void Update( GameTime gameTime )
        {

        }

        /// <summary>
        /// Draws the terminal.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="sb">The sprite batch to use to draw text.</param>
        public static void Draw( GameTime gameTime, SpriteBatch sb )
        {

        }

        /// <summary>
        /// Writes a message to the terminal.
        /// </summary>
        /// <param name="message">The string message.</param>
        /// <param name="options">The formatting options.</param>
        public static void WriteLine( string message, params object[] options )
        {

        }
    }
}