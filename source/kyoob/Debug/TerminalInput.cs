using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#warning TODO : Add a navigable history. (I.e. use arrows to navigate previous commands.)

namespace Kyoob.Debug
{
    /// <summary>
    /// The class used for terminal input.
    /// </summary>
    public sealed class TerminalInput
    {
        private SpriteFont _font;
        private KeyboardState _oldKeys;
        private KeyboardState _newKeys;
        private bool _hasControl;
        private string _text;
        private CommandHandler _commands;

        /// <summary>
        /// The event that is called when the input requests control of user input.
        /// </summary>
        public event EventHandler<EventArgs> RequestControl;

        /// <summary>
        /// The event that is called when the input releases control of user input.
        /// </summary>
        public event EventHandler<EventArgs> ReleaseControl;

        /// <summary>
        /// Gets or sets the terminal input's font.
        /// </summary>
        public SpriteFont Font
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
        /// Gets whether or not the input has control.
        /// </summary>
        public bool HasControl
        {
            get
            {
                return _hasControl;
            }
        }

        /// <summary>
        /// Gets the entered text.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// Gets the command handler.
        /// </summary>
        public CommandHandler Commands
        {
            get
            {
                return _commands;
            }
        }

        /// <summary>
        /// Creates a new terminal input object.
        /// </summary>
        /// <param name="font">The font to use.</param>
        public TerminalInput( SpriteFont font )
        {
            _font = font;
            _oldKeys = Keyboard.GetState();
            _hasControl = false;
            _commands = new CommandHandler();
        }

        /// <summary>
        /// Checks to see if a key was pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private bool WasPressed( Keys key )
        {
            return _newKeys.IsKeyUp( key ) && _oldKeys.IsKeyDown( key );
        }

        /// <summary>
        /// Checks to see if our control has changed.
        /// </summary>
        private void CheckForControl()
        {
            // if tilde was pressed
            if ( WasPressed( Keys.OemTilde ) )
            {
                // get the event and change our control flag
                EventHandler<EventArgs> evt = null;
                if ( _hasControl )
                {
                    _hasControl = false;
                    _text = "";
                    evt = ReleaseControl;
                }
                else
                {
                    _hasControl = true;
                    evt = RequestControl;
                }
                
                // call the event if it's not null
                if ( evt != null )
                {
                    evt( this, new EventArgs() );
                }
            }
        }

        /// <summary>
        /// Converts a key to a string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="shift">Whether or not shift is being held down.</param>
        /// <returns></returns>
        private string KeyToString( Keys key, bool shift )
        {
            char ch = '\0';
            
            // convert the key, if it's known, to a character
            switch (key)
            {
                case Keys.OemMinus:  ch = '-'; break;
                case Keys.Space:     ch = ' '; break;
                case Keys.Decimal:
                case Keys.OemPeriod: ch = '.'; break;
                case Keys.Back:      ch = '\b'; break;

                case Keys.A: ch = 'a'; break;
                case Keys.B: ch = 'b'; break;
                case Keys.C: ch = 'c'; break;
                case Keys.D: ch = 'd'; break;
                case Keys.E: ch = 'e'; break;
                case Keys.F: ch = 'f'; break;
                case Keys.G: ch = 'g'; break;
                case Keys.H: ch = 'h'; break;
                case Keys.I: ch = 'i'; break;
                case Keys.J: ch = 'j'; break;
                case Keys.K: ch = 'k'; break;
                case Keys.L: ch = 'l'; break;
                case Keys.M: ch = 'm'; break;
                case Keys.N: ch = 'n'; break;
                case Keys.O: ch = 'o'; break;
                case Keys.P: ch = 'p'; break;
                case Keys.Q: ch = 'q'; break;
                case Keys.R: ch = 'r'; break;
                case Keys.S: ch = 's'; break;
                case Keys.T: ch = 't'; break;
                case Keys.U: ch = 'u'; break;
                case Keys.V: ch = 'v'; break;
                case Keys.W: ch = 'w'; break;
                case Keys.X: ch = 'x'; break;
                case Keys.Y: ch = 'y'; break;
                case Keys.Z: ch = 'z'; break;

                case Keys.NumPad0:
                case Keys.D0: ch = '0'; break;
                case Keys.NumPad1:
                case Keys.D1: ch = '1'; break;
                case Keys.NumPad2:
                case Keys.D2: ch = '2'; break;
                case Keys.NumPad3:
                case Keys.D3: ch = '3'; break;
                case Keys.NumPad4:
                case Keys.D4: ch = '4'; break;
                case Keys.NumPad5:
                case Keys.D5: ch = '5'; break;
                case Keys.NumPad6:
                case Keys.D6: ch = '6'; break;
                case Keys.NumPad7:
                case Keys.D7: ch = '7'; break;
                case Keys.NumPad8:
                case Keys.D8: ch = '8'; break;
                case Keys.NumPad9:
                case Keys.D9: ch = '9'; break;
            }

            // if they key was unrecognized, return an empty string
            if ( ch == '\0' )
            {
                return "";
            }

            // return the character based on whether or not shift is down
            if ( shift )
            {
                return ch.ToString().ToUpper();
            }
            return ch.ToString();
        }

        /// <summary>
        /// Appends some given text to the displayed text.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <returns></returns>
        private void AppendToText( string text )
        {
            for ( int i = 0; i < text.Length; ++i )
            {
                if ( text[ i ] == '\b' )
                {
                    if ( _text.Length > 0 )
                    {
                        _text = _text.Substring( 0, _text.Length - 1 );
                    }
                }
                else
                {
                    _text += text[ i ].ToString();
                }
            }
        }

        /// <summary>
        /// Updates the text.
        /// </summary>
        private void UpdateText()
        {
            HashSet<Keys> newDown = new HashSet<Keys>( _newKeys.GetPressedKeys() );
            Keys[] toCheck = _oldKeys.GetPressedKeys();

            foreach ( Keys key in toCheck )
            {
                if ( !newDown.Contains( key ) )
                {
                    AppendToText( KeyToString( key, _newKeys.IsKeyDown( Keys.LeftShift ) ) );
                }
            }
        }

        /// <summary>
        /// Updates the terminal input.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update( GameTime gameTime )
        {
            _newKeys = Keyboard.GetState();

            // check to see if we've requested or released control
            CheckForControl();

            // if we have control, update the text
            if ( _hasControl )
            {
                UpdateText();
            }

            // if enter was pressed, parse the command
            if ( _newKeys.IsKeyUp( Keys.Enter ) && _oldKeys.IsKeyDown( Keys.Enter ) )
            {
                _commands.ParseCommand( _text );
                _text = "";
                _hasControl = false;
                if ( ReleaseControl != null )
                {
                    ReleaseControl( this, new EventArgs() );
                }
            }

            _oldKeys = _newKeys;
        }
    }
}