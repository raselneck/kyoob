using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Kyoob
{
    /// <summary>
    /// Contains Kyoob settings.
    /// </summary>
    public sealed class Settings
    {
        private static Settings _instance;

        /// <summary>
        /// Gets the singleton settings instance.
        /// </summary>
        public static Settings Instance
        {
            get
            {
                if ( _instance == null && !Load() )
                {
                    _instance = new Settings();
                }
                return _instance;
            }
        }

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
        /// Gets or sets whether or not to enable vertical retrace synchronization.
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
        /// Gets or sets whether or not to log verbose output.
        /// </summary>
        [JsonProperty( "log_verbose" )]
        public bool UseVerboseOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not to use frustum culling.
        /// </summary>
        [JsonProperty( "cull_frustum" )]
        public bool CullFrustum
        {
            get;
            set;
        }

        /// <summary>
        /// Attempts to load the settings file.
        /// </summary>
        /// <returns></returns>
        public static bool Load()
        {
            // check if the file exists
            if ( !File.Exists( "./settings.json" ) )
            {
                return false;
            }

            // now try to serialize the file
            try
            {
                using ( var r = new StreamReader( "./settings.json" ) )
                {
                    _instance = JsonConvert.DeserializeObject<Settings>( r.ReadToEnd() );
                }
                return true;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex.Message );
                Debug.WriteLine( "Stack trace:" + Environment.NewLine + ex.StackTrace );
                return false;
            }
        }

        /// <summary>
        /// Saves the settings to a file.
        /// </summary>
        public static void Save()
        {
            using ( var w = new StreamWriter( "./settings.json" ) )
            {
                // use the property instance in case the field is null
                w.WriteLine( JsonConvert.SerializeObject( Instance, Formatting.Indented ) );
            }
        }

        /// <summary>
        /// Creates a new settings object.
        /// </summary>
        private Settings()
        {
            WindowWidth = 1280;
            WindowHeight = 720;
#if DEBUG
            UseVerboseOutput = true;
#else
            UseVerboseOutput = false;
#endif
            CullFrustum = false;
            VSync = true;
            ViewDistance = 128.0f;
        }
    }
}