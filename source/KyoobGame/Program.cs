using System;
using System.Diagnostics;
using System.IO;

namespace Kyoob
{
#if WINDOWS
    /// <summary>
    /// The internal program class.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// A custom trace listener.
        /// </summary>
        private class CustomTraceListener : TextWriterTraceListener
        {
            /// <summary>
            /// Creates a new custom trace listener.
            /// </summary>
            /// <param name="writer">The writer to use.</param>
            public CustomTraceListener( TextWriter writer )
                : base( writer )
            {
            }

            /// <summary>
            /// Creates a new custom trace listener.
            /// </summary>
            /// <param name="stream">The stream to write to.</param>
            public CustomTraceListener( Stream stream )
                : base( stream )
            {
            }

            /// <summary>
            /// Creates a new custom trace listener.
            /// </summary>
            /// <param name="file">The file to append to.</param>
            public CustomTraceListener( string file )
                : base( file )
            {
            }

            /// <summary>
            /// Writes to this trace listener.
            /// </summary>
            /// <param name="message">The message.</param>
            public override void Write( string message )
            {
                base.Write( message );
            }

            /// <summary>
            /// Writes a line to this trace listener.
            /// </summary>
            /// <param name="message">The message.</param>
            public override void WriteLine( string message )
            {
                base.WriteLine( CreateTimestamp() + message );
            }

            /// <summary>
            /// Creates a timestamp to use.
            /// </summary>
            /// <returns></returns>
            private string CreateTimestamp()
            {
                DateTime now = DateTime.Now.ToLocalTime();
                return string.Format(
                    "[{0:00}/{1:00}/{2:00} {3}:{4:00}:{5:00}] ",
                    now.Month, now.Day, now.Year - 2000, now.Hour, now.Minute, now.Second
                );
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main( string[] args )
        {
            // setup the debugger
            if ( File.Exists( "debug.log" ) )
            {
                File.Delete( "debug.log" );
            }
            Debug.Listeners.Clear(); // remove default listener
            Debug.Listeners.Add( new CustomTraceListener( Console.Out ) );
            Debug.Listeners.Add( new CustomTraceListener( "debug.log" ) );
            Debug.IndentSize = 2;
            Debug.AutoFlush = true;

            // run the game
            using ( Game game = Game.Instance )
            {
                game.Run();
            }
        }
    }
#endif
}