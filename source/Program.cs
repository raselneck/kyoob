using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Kyoob
{
#if WINDOWS
    /// <summary>
    /// The internal program class.
    /// </summary>
    internal static class Program
    {
#if DEBUG
        /// <summary>
        /// A custom trace listener.
        /// </summary>
        private class CustomTraceListener : TraceListener
        {
            private TextWriter _output;

            /// <summary>
            /// Creates a new custom trace listener.
            /// </summary>
            /// <param name="writer">The writer to use.</param>
            public CustomTraceListener( TextWriter writer )
            {
                _output = writer;
            }

            /// <summary>
            /// Creates a new custom trace listener.
            /// </summary>
            /// <param name="stream">The stream to write to.</param>
            public CustomTraceListener( Stream stream )
            {
                _output = new StreamWriter( stream );
            }

            /// <summary>
            /// Creates a new custom trace listener.
            /// </summary>
            /// <param name="file">The file to append to.</param>
            public CustomTraceListener( string file )
            {
                _output = new StreamWriter( File.Create( file ) );
            }

            /// <summary>
            /// Closes the underlying stream.
            /// </summary>
            public override void Close()
            {
                _output.Close();
                base.Close();
            }

            /// <summary>
            /// Flushes the underlying stream.
            /// </summary>
            public override void Flush()
            {
                _output.Flush();
                base.Flush();
            }

            /// <summary>
            /// Writes to this custom trace listener.
            /// </summary>
            /// <param name="message">The message to write.</param>
            public override void Write( string message )
            {
                _output.Write( message );
            }

            /// <summary>
            /// Writes a line to this trace listener.
            /// </summary>
            /// <param name="message">The message.</param>
            public override void WriteLine( string message )
            {
                string output = message.PadLeft( message.Length + IndentLevel * IndentSize, ' ' );
                _output.WriteLine( CreateTimestamp() + output );
            }

            /// <summary>
            /// Disposes of the underlying stream.
            /// </summary>
            /// <param name="disposing">???</param>
            protected override void Dispose( bool disposing )
            {
                if ( disposing )
                {
                    _output.Dispose();
                }
                base.Dispose( disposing );
            }

            /// <summary>
            /// Creates a timestamp to use.
            /// </summary>
            /// <returns></returns>
            protected virtual string CreateTimestamp()
            {
                DateTime now = DateTime.Now.ToLocalTime();
                return string.Format(
                    "[{0:00}/{1:00}/{2:00} {3}:{4:00}:{5:00}] ",
                    now.Month, now.Day, now.Year - 2000, now.Hour, now.Minute, now.Second
                );
            }
        }
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main( string[] args )
        {
            try
            {
#if DEBUG
                // setup the debugger
                Debug.Listeners.Clear(); // remove default listener
                Debug.Listeners.Add( new CustomTraceListener( Console.Out ) );
                Debug.Listeners.Add( new CustomTraceListener( "debug.log" ) );
                Debug.IndentSize = 2;
                Debug.AutoFlush = true;
#endif

                // load the settings
                Settings.Load();

                // run the game
                using ( Game game = Game.Instance )
                {
                    game.Run();
                }

                // save the settings
                Settings.Save();
            }
            catch ( Exception ex )
            {
#if DEBUG
                Debug.WriteLine( ex.Message + "\n\n" + ex.StackTrace );
                MessageBox.Show( "An error has occurred. Please check the log.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error );
#else
                MessageBox.Show( ex.Message + "\n\n" + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error );
#endif
            }
        }
    }
#endif
}