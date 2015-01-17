using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Kyoob
{
    /// <summary>
    /// Contains Kyoob game settings.
    /// </summary>
    public sealed class GameSettings
    {
        private Game _game;

        /// <summary>
        /// Gets or sets the desired window width.
        /// </summary>
        [JsonProperty( "window_width" )]
        public int WindowWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the desired window height.
        /// </summary>
        [JsonProperty( "window_height" )]
        public int WindowHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether window resizing should be enabled.
        /// </summary>
        [JsonProperty( "window_resizable" )]
        public bool WindowResizable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the game should be windowed.
        /// </summary>
        [JsonProperty( "windowed" )]
        public bool Windowed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether vertical-retrace syncing should be enabled.
        /// </summary>
        [JsonProperty( "vsync" )]
        public bool VSync
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the desired view distance.
        /// </summary>
        [JsonProperty( "view_distance" )]
        public float ViewDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether anti-aliasing should be enabled.
        /// </summary>
        [JsonProperty( "antialias" )]
        public bool AntiAlias
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the anti-aliasing level.
        /// </summary>
        [JsonProperty( "antialias_level" )]
        public int AntiAliasLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether smooth lighting should be enabled.
        /// </summary>
        [JsonProperty( "smooth_lighting" )]
        public bool SmoothLighting
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether frustum culling should be enabled.
        /// </summary>
        [JsonProperty( "cull_frustum" )]
        public bool CullFrustum
        {
            get;
            set;
        }
        
        /// <summary>
        /// Creates a new game settings object.
        /// </summary>
        /// <param name="game">The game these settings belong to.</param>
        internal GameSettings( Game game )
        {
            _game = game;

            WindowWidth         = 800;
            WindowHeight        = 600;
            WindowResizable     = false;
            Windowed            = true;
            VSync               = true;
            ViewDistance        = 128.0f;
            AntiAlias           = false;
            AntiAliasLevel      = 0;
            SmoothLighting      = true;
            CullFrustum         = false;
        }

        /// <summary>
        /// The constructor used by JSON serialization.
        /// </summary>
        [JsonConstructor]
        private GameSettings()
            : this( null )
        {
        }

        /// <summary>
        /// Applies any changes made to the game these settings belong to.
        /// </summary>
        public void ApplyChanges()
        {
            Debug.WriteLine( "TODO : Implement GameSettings.ApplyChanges()" );
        }

        /// <summary>
        /// Attempts to load the given settings file.
        /// </summary>
        /// <param name="fname">The file name.</param>
        /// <returns></returns>
        public bool Load( string fname )
        {
            // ensure file exists
            if ( !File.Exists( fname ) )
            {
                return false;
            }

            // try to serialize
            try
            {
                using ( var r = new StreamReader( fname ) )
                {
                    Copy( JsonConvert.DeserializeObject<GameSettings>( r.ReadToEnd() ) );
                }
                return true;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( "Exception while reading game settings: \"{0}\"\n{1}", ex.Message, ex.StackTrace );
                return false;
            }
        }

        /// <summary>
        /// Saves these settings to a file.
        /// </summary>
        /// <param name="fname">The file name.</param>
        public void Save( string fname )
        {
            using ( var w = new StreamWriter( fname ) )
            {
                // use the property instance in case the field is null
                w.WriteLine( JsonConvert.SerializeObject( this, Formatting.Indented ) );
            }
        }

        /// <summary>
        /// Copies the given game settings object.
        /// </summary>
        /// <param name="other">The other settings.</param>
        private void Copy( GameSettings other )
        {
            AntiAlias           = other.AntiAlias;
            AntiAliasLevel      = other.AntiAliasLevel;
            CullFrustum         = other.CullFrustum;
            Windowed            = other.Windowed;
            SmoothLighting      = other.SmoothLighting;
            ViewDistance        = other.ViewDistance;
            VSync               = other.VSync;
            WindowHeight        = other.WindowHeight;
            WindowResizable     = other.WindowResizable;
            WindowWidth         = other.WindowWidth;
        }
    }
}