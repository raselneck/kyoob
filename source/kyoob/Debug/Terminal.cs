using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Graphics;

using XnaGame = Microsoft.Xna.Framework.Game;

namespace Kyoob.Debug
{
    /// <summary>
    /// The "console" that will be drawn on screen.
    /// </summary>
    public static class Terminal
    {
        /// <summary>
        /// The maximum number of messages to display on screen at a time.
        /// </summary>
        private const int MaxMessagesOnScreen = 12;

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
        private static SpriteFont _font;
        private static float _lineHeight;
        private static TerminalInput _input;
        private static bool _isHidden;

        // FPS monitoring is delegated to the terminal
        private static int _frameCount;
        private static double _tickCount;

        /// <summary>
        /// The event that is called when the terminal requests control of user input.
        /// </summary>
        public static event EventHandler<EventArgs> RequestControl;

        /// <summary>
        /// The event that is called when the terminal releases control of user input.
        /// </summary>
        public static event EventHandler<EventArgs> ReleaseControl;

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
                _lineHeight = _font.MeasureString( "M" ).Y;
                _input.Font = _font;
            }
        }

        /// <summary>
        /// Gets the terminal's command handler.
        /// </summary>
        public static CommandHandler Commands
        {
            get
            {
                return _input.Commands;
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
#if DEBUG
            _isHidden = false;
#else
            _isHidden = true;
#endif

            // setup the input
            _input = new TerminalInput( null );
            _input.ReleaseControl += ( sender, args ) =>
            {
                if ( ReleaseControl != null )
                {
                    ReleaseControl( sender, args );
                }
            };
            _input.RequestControl += ( sender, args ) =>
            {
                if ( RequestControl != null )
                {
                    RequestControl( sender, args );
                }
            };

            // setup some terminal commands
            AddCommand( "terminal", "show", ( string[] param ) =>
            {
                _isHidden = false;
            } );
            AddCommand( "terminal", "hide", ( string[] param ) =>
            {
                _isHidden = true;
            } );
        }

        /// <summary>
        /// Draws a highlighted message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="position">The position to draw the image at.</param>
        private static void DrawHighlighted( TerminalMessage message, Vector2 position )
        {
            // draw a black border around the text
            Renderer2D.DrawString( _font, message.Message, new Vector2( position.X - 1, position.Y ), Color.Black );
            Renderer2D.DrawString( _font, message.Message, new Vector2( position.X + 1, position.Y ), Color.Black );
            Renderer2D.DrawString( _font, message.Message, new Vector2( position.X, position.Y - 1 ), Color.Black );
            Renderer2D.DrawString( _font, message.Message, new Vector2( position.X, position.Y + 1 ), Color.Black );

            // now draw the actual message
            Renderer2D.DrawString( _font, message.Message, position, message.TextColor );
        }

        /// <summary>
        /// Updates the terminal.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public static void Update( GameTime gameTime )
        {
            _input.Update( gameTime );

            // update only the messages that will show on screen, and remove them if they're dead
            TerminalMessage message;
            int count = 0;
            for ( int i = 0; i < Math.Min( _messages.Count, MaxMessagesOnScreen ); ++i )
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
                    ++count;
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
                Renderer2D.Begin();

                // draw all messages from the top of the screen down (checking for input first)
                Vector2 position = new Vector2( 10.0f, 10.0f );
                if ( _input.HasControl )
                {
                    TerminalMessage msg = new TerminalMessage( Color.White, 0.0, _input.Text + "|" );
                    DrawHighlighted( msg, position );
                }
                if ( !_isHidden )
                {
                    for ( int i = 0; i < Math.Min( _messages.Count, MaxMessagesOnScreen ); ++i )
                    {
                        // add the line height first for when we implement command input
                        position.Y += _lineHeight;
                        DrawHighlighted( _messages[ i ], position );
                    }
                }

                Renderer2D.End();
            }
        }

        /// <summary>
        /// Adds a command to the terminal.
        /// </summary>
        /// <param name="obj">The object name.</param>
        /// <param name="func">The function name.</param>
        /// <param name="callback">The function callback.</param>
        public static void AddCommand( string obj, string func, CommandCallback callback )
        {
            _input.Commands.AddCallback( obj, func, callback );
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

        /// <summary>
        /// Writes a normal message to the terminal.
        /// </summary>
        /// <param name="message">The string message.</param>
        /// <param name="options">The formatting options.</param>
        public static void WriteLine( string message, params object[] options )
        {
            WriteLine( Color.White, 5.0, message, options );
        }

        /// <summary>
        /// Writes an information message to the terminal.
        /// </summary>
        /// <param name="message">The string message.</param>
        /// <param name="options">The formatting options.</param>
        public static void WriteInfo( string message, params object[] options )
        {
            WriteLine( Color.Cyan, 5.0, message, options );
        }

        /// <summary>
        /// Writes an error message to the terminal.
        /// </summary>
        /// <param name="message">The string message.</param>
        /// <param name="options">The formatting options.</param>
        public static void WriteError( string message, params object[] options )
        {
            Color lightRed = Color.Lerp( Color.Red, Color.White, 0.3f );
            WriteLine( lightRed, 5.0, message, options );
        }

        /// <summary>
        /// Writes a warning message to the terminal.
        /// </summary>
        /// <param name="message">The string message.</param>
        /// <param name="options">The formatting options.</param>
        public static void WriteWarning( string message, params object[] options )
        {
            WriteLine( Color.Cyan, 5.0, message, options );
        }
    }
}