using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// TODO : Add draw texture overloads and remove access to sprite batch so that the LensFlare doesn't hijack the sprite batch

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains a way to perform immediate drawing of a GUI.
    /// </summary>
    public static class GUI
    {
        private static GraphicsDevice _graphicsDevice;
        private static ContentManager _content;
        private static SpriteBatch _spriteBatch;
        private static SpriteFont _spriteFont;
        private static Texture2D _pixel;


        /// <summary>
        /// Gets the sprite batch used by the GUI.
        /// </summary>
        public static SpriteBatch SpriteBatch
        {
            get
            {
                return _spriteBatch;
            }
        }


        /// <summary>
        /// Statically initializes the GUI drawing utility.
        /// </summary>
        static GUI()
        {
            _graphicsDevice = Game.Instance.GraphicsDevice;
            _content = Game.Instance.Content;

            _spriteBatch = new SpriteBatch( _graphicsDevice );
            LoadFont( "fonts/arial" ); // default font

            _pixel = new Texture2D( _graphicsDevice, 1, 1 );
            _pixel.SetData<Color>( new Color[ 1 ] { Color.White } );
        }


        /// <summary>
        /// Loads the given asset as a font.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        public static void LoadFont( string assetName )
        {
            _spriteFont = Kyoob.Game.Instance.Content.Load<SpriteFont>( assetName );
        }

        /// <summary>
        /// Prepares to begin drawing GUI elements.
        /// </summary>
        public static void Begin()
        {
            // save graphics state
            GraphicsState.Push();

            // set blend state
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            // tell the sprite batch to begin
            _spriteBatch.Begin();
        }

        /// <summary>
        /// Restores the graphics device's rendering states and tells the
        /// underlying sprite batch to end.
        /// </summary>
        public static void End()
        {
            _spriteBatch.End();
            GraphicsState.Pop();
        }


        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="text">The text.</param>
        public static void DrawText( float x, float y, string text )
        {
            _spriteBatch.DrawString( _spriteFont, text, new Vector2( x, y ), Color.White );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The format arguments.</param>
        public static void DrawText( float x, float y, string text, params object[] args )
        {
            _spriteBatch.DrawString( _spriteFont, string.Format( text, args ), new Vector2( x, y ), Color.White );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="color">The text color.</param>
        /// <param name="text">The text.</param>
        public static void DrawText( float x, float y, Color color, string text )
        {
            _spriteBatch.DrawString( _spriteFont, text, new Vector2( x, y ), color );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="color">The text color.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The format arguments.</param>
        public static void DrawText( float x, float y, Color color, string text, params object[] args )
        {
            _spriteBatch.DrawString( _spriteFont, string.Format( text, args ), new Vector2( x, y ), color );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="text">The text.</param>
        public static void DrawText( Vector2 pos, string text )
        {
            _spriteBatch.DrawString( _spriteFont, text, pos, Color.White );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The format arguments.</param>
        public static void DrawText( Vector2 pos, string text, params object[] args )
        {
            _spriteBatch.DrawString( _spriteFont, string.Format( text, args ), pos, Color.White );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="text">The text.</param>
        public static void DrawText( Vector2 pos, Color color, string text )
        {
            _spriteBatch.DrawString( _spriteFont, text, pos, color );
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="text">The text.</param>
        /// <param name="args">The format arguments.</param>
        public static void DrawText( Vector2 pos, Color color, string text, params object[] args )
        {
            _spriteBatch.DrawString( _spriteFont, string.Format( text, args ), pos, color );
        }
    }
}