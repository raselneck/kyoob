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
        /// <summary>
        /// Contains terminal message data.
        /// </summary>
        private struct TerminalMessage
        {
            /// <summary>
            /// The terminal's message.
            /// </summary>
            public string Message;

            /// <summary>
            /// The remaining time that the terminal message has to be alive.
            /// </summary>
            public double RemainingTime;

            /// <summary>
            /// The message's text color.
            /// </summary>
            public Color TextColor;

            /// <summary>
            /// Creates a new terminal message.
            /// </summary>
            /// <param name="color">The message's color.</param>
            /// <param name="time">The time that the message should be displayed in seconds.</param>
            /// <param name="message">The message.</param>
            public TerminalMessage( Color color, double time, string message )
            {
                TextColor = color;
                Message = message;
                RemainingTime = time;
            }
        }

        private static List<TerminalMessage> _messages;
        private static GraphicsDevice _device;
        private static DepthStencilState _depthState;
        private static SpriteBatch _spriteBatch;
        private static SpriteFont _font;
        private static float _lineHeight;

        // FPS monitoring is delegated to the terminal
        private static int _frameCount;
        private static double _tickCount;

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
                _lineHeight = _font.MeasureString( "F" ).Y;
            }
        }

        /// <summary>
        /// Static terminal initialization.
        /// </summary>
        static Terminal()
        {
            _font = null;
            _frameCount = 0;
            _tickCount = 0;
            _messages = new List<TerminalMessage>();
        }

        /// <summary>
        /// Draws a highlighted message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="position">The position to draw the image at.</param>
        private static void DrawHighlighted( TerminalMessage message, Vector2 position )
        {
            // draw a black border around the text
            _spriteBatch.DrawString( _font, message.Message, new Vector2( position.X - 1, position.Y ), Color.Black );
            _spriteBatch.DrawString( _font, message.Message, new Vector2( position.X + 1, position.Y ), Color.Black );
            _spriteBatch.DrawString( _font, message.Message, new Vector2( position.X, position.Y - 1 ), Color.Black );
            _spriteBatch.DrawString( _font, message.Message, new Vector2( position.X, position.Y + 1 ), Color.Black );

            // now draw the actual message
            _spriteBatch.DrawString( _font, message.Message, position, message.TextColor );
        }

        /// <summary>
        /// Initializes the terminal to work with a game.
        /// </summary>
        /// <param name="game">The game.</param>
        public static void Initialize( Game game )
        {
            _device = game.GraphicsDevice;
            _depthState = _device.DepthStencilState;
            _spriteBatch = new SpriteBatch( _device );
        }

        /// <summary>
        /// Updates the terminal.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public static void Update( GameTime gameTime )
        {
            // update all messages, and remove them if they're dead
            TerminalMessage message;
            for ( int i = 0; i < _messages.Count; ++i )
            {
                message = _messages[ i ];
                message.RemainingTime -= gameTime.ElapsedGameTime.TotalSeconds;
                if ( _messages[ i ].RemainingTime <= 0.0 )
                {
                    _messages.RemoveAt( i );
                    --i;
                }
                else
                {
                    _messages[ i ] = message;
                }
            }
        }

        /// <summary>
        /// Draws the terminal.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public static void Draw( GameTime gameTime )
        {
            // update FPS data
            ++_frameCount;
            _tickCount += gameTime.ElapsedGameTime.TotalSeconds;
            if ( _tickCount >= 1.0 )
            {
                WriteLine( Color.White, 0.95, _frameCount + " FPS" );
                _frameCount = 0;
                _tickCount -= 1.0;
            }

            // we can only draw if we have a font
            if ( _font != null )
            {
                _spriteBatch.Begin();
                
                // draw FPS and then all messages from the top of the screen down
                Vector2 position = new Vector2( 10.0f, 10.0f );
                for ( int i = 0; i < _messages.Count; ++i )
                {
                    // add the line height first for when we implement command input
                    position.Y += _lineHeight;
                    DrawHighlighted( _messages[ i ], position );
                }

                _spriteBatch.End();
                _device.DepthStencilState = _depthState;
            }
        }

        /// <summary>
        /// Writes a message to the terminal.
        /// </summary>
        /// <param name="color">The color to display the message in.</param>
        /// <param name="time">The time that the message should be displayed in seconds.</param>
        /// <param name="message">The string message.</param>
        /// <param name="options">The formatting options.</param>
        public static void WriteLine( Color color, double time, string message, params object[] options )
        {
            lock ( _messages )
            {
                // format the message
                message = string.Format( message, options );

                // if there are any new lines, then we need to split them up
                if ( message.Contains( "\n" ) )
                {
                    string[] parts = message.Split( '\n' );
                    for ( int i = 0; i < parts.Length; ++i )
                    {
                        if ( !string.IsNullOrEmpty( parts[ i ] ) )
                        {
                            _messages.Add( new TerminalMessage( color, time, message ) );
                        }
                    }
                }
                // otherwise we just add the message
                else
                {
                    _messages.Add( new TerminalMessage( color, time, message ) );
                }
            }
        }
    }
}