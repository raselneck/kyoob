using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kyoob.Game
{
    /// <summary>
    /// Contains game settings.
    /// </summary>
    public sealed class GameSettings
    {
        private float _mouseSensitivity;
        private Keys _keyStrafeLeft;
        private Keys _keyStrafeRight;
        private Keys _keyStrafeForward;
        private Keys _keyStrafeBackward;
        private Keys _keySprint;
        private Keys _keyJump;
        private Keys _keyConsole;

        /// <summary>
        /// Gets or sets the mouse velocity.
        /// </summary>
        [JsonProperty( "mouse_sensitivity" )]
        public float MouseSensitivity
        {
            get
            {
                return _mouseSensitivity;
            }
            set
            {
                _mouseSensitivity = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for strafing to the left.
        /// </summary>
        [JsonProperty( "strafe_left" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys StrafeLeftKey
        {
            get
            {
                return _keyStrafeLeft;
            }
            set
            {
                _keyStrafeLeft = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for strafing to the right.
        /// </summary>
        [JsonProperty( "strafe_right" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys StrafeRightKey
        {
            get
            {
                return _keyStrafeRight;
            }
            set
            {
                _keyStrafeRight = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for strafing forward.
        /// </summary>
        [JsonProperty( "strafe_forward" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys StrafeForwardKey
        {
            get
            {
                return _keyStrafeForward;
            }
            set
            {
                _keyStrafeForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for strafing backward.
        /// </summary>
        [JsonProperty( "strafe_backward" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys StrafeBackwardKey
        {
            get
            {
                return _keyStrafeBackward;
            }
            set
            {
                _keyStrafeBackward = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for sprinting.
        /// </summary>
        [JsonProperty( "sprint_key" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys SprintKey
        {
            get
            {
                return _keySprint;
            }
            set
            {
                _keySprint = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for jumping.
        /// </summary>
        [JsonProperty( "jump_key" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys JumpKey
        {
            get
            {
                return _keyJump;
            }
            set
            {
                _keyJump = value;
            }
        }

        /// <summary>
        /// Gets or sets the console key.
        /// </summary>
        [JsonProperty( "console_key" ), JsonConverter( typeof( StringEnumConverter ) )]
        public Keys ConsoleKey
        {
            get
            {
                return _keyConsole;
            }
            set
            {
                _keyConsole = value;
            }
        }

        /// <summary>
        /// Creates a new game settings object.
        /// </summary>
        public GameSettings()
        {
            _mouseSensitivity = 0.06f;
            _keyStrafeLeft = Keys.A;
            _keyStrafeRight = Keys.D;
            _keyStrafeForward = Keys.W;
            _keyStrafeBackward = Keys.S;
            _keySprint = Keys.LeftShift;
            _keyJump = Keys.Space;
            _keyConsole = Keys.OemTilde;
        }

        /// <summary>
        /// Copies settings from another GameSettings object.
        /// </summary>
        /// <param name="other">The other settings.</param>
        private void CopyFrom( GameSettings other )
        {
            _mouseSensitivity   = other._mouseSensitivity;
            _keyStrafeLeft      = other._keyStrafeLeft;
            _keyStrafeRight     = other._keyStrafeRight;
            _keyStrafeForward   = other._keyStrafeForward;
            _keyStrafeBackward  = other._keyStrafeBackward;
            _keySprint          = other._keySprint;
            _keyJump            = other._keyJump;
            _keyConsole         = other._keyConsole;
        }

        /// <summary>
        /// Imports settings information from a file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public bool Import( string file )
        {
            // make sure the file exists
            if ( !File.Exists( file ) )
            {
                return false;
            }

            // deserialize the file
            GameSettings that = null;
            using ( StreamReader sr = new StreamReader( File.OpenRead( file ) ) )
            {
                JsonTextReader reader = new JsonTextReader( sr );
                JsonSerializer serializer = new JsonSerializer();
                that = serializer.Deserialize<GameSettings>( reader );
            }

            // copy over the settings
            if ( that != null )
            {
                CopyFrom( that );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Exports settings information to a file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public void Export( string file )
        {
            using ( StreamWriter sw = new StreamWriter( File.Create( file ) ) )
            {
                JsonTextWriter writer = new JsonTextWriter( sw );
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize( writer, this );
            }
        }
    }
}