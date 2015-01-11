using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kyoob
{
    /// <summary>
    /// The internal class used for profiling.
    /// </summary>
    internal class Profiler
    {
        /// <summary>
        /// Contains profiling information.
        /// </summary>
        private class Profile
        {
            private Stopwatch _stopwatch;
            private double _totalTime;
            private long _totalCount;

            /// <summary>
            /// Gets the total number of times this profile was run.
            /// </summary>
            public long TotalCount
            {
                get
                {
                    return _totalCount;
                }
            }

            /// <summary>
            /// Gets the total elapsed time for this profile.
            /// </summary>
            public double TotalTime
            {
                get
                {
                    return _totalTime;
                }
            }

            /// <summary>
            /// Gets the average time for each time this profile was run.
            /// </summary>
            public double AverageTime
            {
                get
                {
                    return _totalTime / _totalCount;
                }
            }

            /// <summary>
            /// Checks to see if this profile is running.
            /// </summary>
            public bool IsRunning
            {
                get
                {
                    return _stopwatch.IsRunning;
                }
            }

            /// <summary>
            /// Creates a new profile.
            /// </summary>
            public Profile()
            {
                _stopwatch = new Stopwatch();
                _totalCount = 0;
                _totalTime = 0.0;
            }

            /// <summary>
            /// Starts this profile's stat checking.
            /// </summary>
            public void Start()
            {
                _stopwatch.Start();
            }

            /// <summary>
            /// Stops this profile's stat checking.
            /// </summary>
            public void Stop()
            {
                _stopwatch.Stop();

                _totalCount++;
                _totalTime += _stopwatch.Elapsed.TotalSeconds;

                _stopwatch.Reset();
            }
        }

        private static Profiler _instance;
        private Dictionary<string, Profile> _profiles;

        /// <summary>
        /// Gets the profile collection being used.
        /// </summary>
        private static Dictionary<string, Profile> Profiles
        {
            get
            {
                return _instance._profiles;
            }
        }

        /// <summary>
        /// Ensures that the profiler instance exists.
        /// </summary>
        static Profiler()
        {
            _instance = new Profiler();
        }

        /// <summary>
        /// Creates a new profiler.
        /// </summary>
        private Profiler()
        {
            _profiles = new Dictionary<string, Profile>();
        }

        /// <summary>
        /// Stops all profiles and writes out their information to the debugger.
        /// </summary>
        ~Profiler()
        {
            lock ( _instance )
            {
                foreach ( var pair in _profiles )
                {
                    // stop the profiler
                    var profile = pair.Value;
                    if ( profile.IsRunning )
                    {
                        profile.Stop();
                    }

                    Debug.WriteLine( "Profile: " + pair.Key );
                    Debug.Indent();
                    Debug.WriteLine( "Total Runs: {0}", profile.TotalCount );
                    Debug.WriteLine( "Total Time: {0}s", profile.TotalTime );
                    Debug.WriteLine( "Time / Run: {0}s", profile.AverageTime );
                    Debug.Unindent();
                }
            }
        }

        /// <summary>
        /// Starts the profile with the given name.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        public static void Start( string name )
        {
            lock ( _instance )
            {
                if ( !Profiles.ContainsKey( name ) )
                {
                    Profiles.Add( name, new Profile() );
                }
                Profiles[ name ].Start();
            }
        }

        /// <summary>
        /// Starts the profile with the given name.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        public static void Stop( string name )
        {
            lock ( _instance )
            {
                if ( Profiles.ContainsKey( name ) )
                {
                    Profile profile = Profiles[ name ];
                    if ( profile.IsRunning )
                    {
                        profile.Stop();
                    }
                }
            }
        }
    }
}