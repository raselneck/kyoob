using System.IO;
using System.Text;

namespace Kyoob.Debug
{
    /// <summary>
    /// The text writer used to re-route console output.
    /// </summary>
    internal class TerminalWriter : TextWriter
    {
        /// <summary>
        /// Gets the Terminal writer's encoding.
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return Encoding.ASCII;
            }
        }

        /// <summary>
        /// Creates a new terminal text writer.
        /// </summary>
        public TerminalWriter()
        {
        }

        public override void Write( bool value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( char value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( char[] buffer )
        {
            Terminal.WriteLine( new string( buffer ) );
        }

        public override void Write( char[] buffer, int index, int count )
        {
            Terminal.WriteLine( new string( buffer, index, count ) );
        }

        public override void Write( decimal value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( double value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( float value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( int value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( long value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( object value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( string format, object arg0 )
        {
            Terminal.WriteLine( format, arg0 );
        }

        public override void Write( string format, object arg0, object arg1 )
        {
            Terminal.WriteLine( format, arg0, arg1 );
        }

        public override void Write( string format, object arg0, object arg1, object arg2 )
        {
            Terminal.WriteLine( format, arg0, arg1, arg2 );
        }

        public override void Write( string format, params object[] arg )
        {
            Terminal.WriteLine( format, arg );
        }

        public override void Write( string value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( uint value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void Write( ulong value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine()
        {
            Terminal.WriteLine( "" );
        }

        public override void WriteLine( bool value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( char value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( char[] buffer )
        {
            Terminal.WriteLine( new string( buffer ) );
        }

        public override void WriteLine( char[] buffer, int index, int count )
        {
            Terminal.WriteLine( new string( buffer, index, count ) );
        }

        public override void WriteLine( decimal value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( double value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( float value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( int value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( long value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( object value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( string format, object arg0 )
        {
            Terminal.WriteLine( format, arg0 );
        }

        public override void WriteLine( string format, object arg0, object arg1 )
        {
            Terminal.WriteLine( format, arg0, arg1 );
        }

        public override void WriteLine( string format, object arg0, object arg1, object arg2 )
        {
            Terminal.WriteLine( format, arg0, arg1, arg2 );
        }

        public override void WriteLine( string format, params object[] arg )
        {
            Terminal.WriteLine( format, arg );
        }

        public override void WriteLine( string value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( uint value )
        {
            Terminal.WriteLine( value.ToString() );
        }

        public override void WriteLine( ulong value )
        {
            Terminal.WriteLine( value.ToString() );
        }
    }
}