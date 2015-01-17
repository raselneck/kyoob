using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kyoob
{
    /// <summary>
    /// An enumeration of possible message box results.
    /// </summary>
    public enum MessageBoxResult : uint
    {
        /// <summary>
        /// The result value of the message box is OK.
        /// </summary>
        OK = 1,

        /// <summary>
        /// The result value of the message box is Cancel.
        /// </summary>
        Cancel,

        /// <summary>
        /// The result value of the message box is Abort.
        /// </summary>
        Abort,

        /// <summary>
        /// The result value of the message box is Retry.
        /// </summary>
        Retry,

        /// <summary>
        /// The result value of the message box is Ignore.
        /// </summary>
        Ignore,

        /// <summary>
        /// The result value of the message box is Yes.
        /// </summary>
        Yes,

        /// <summary>
        /// The result value of the message box is No.
        /// </summary>
        No,

        /// <summary>
        /// The result value of the message box is Close.
        /// </summary>
        Close
    }

    ///<summary>
    /// Specifies constants defining which buttons to display on a MessageBox.
    /// </summary>
    public enum MessageBoxButtons : uint
    {
        /// <summary>
        /// The message box contains an OK button.
        /// </summary>
        OK = 0x000000,

        /// <summary>
        /// The message box contains an OK and Cancel button.
        /// </summary>
        OKCancel = 0x000001,

        /// <summary>
        /// The message box contains Abort, Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore = 0x000002,

        /// <summary>
        /// The message box contains Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel = 0x000003,

        /// <summary>
        /// The message box contains Yes and No buttons.
        /// </summary>
        YesNo = 0x000004,

        /// <summary>
        /// The message box contains Retry and Cancel buttons.
        /// </summary>
        RetryCancel = 0x000005
    }

    /// <summary>
    /// Specifies constants defining which information to display.
    /// </summary>
    public enum MessageBoxIcon
    {
        /// <summary>
        /// The message box contains a symbol consisting of a white X in a circle with a red background.
        /// </summary>
        Hand = 0x000010,

        /// <summary>
        /// The message box contains a symbol consisting of a question mark in a circle.
        /// </summary>
        Question = 0x000020,

        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Exclamation = 0x000030,

        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Asterisk = 0x000040,

        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Warning = Exclamation,

        /// <summary>
        /// The message box contains a symbol consisting of a white X in a circle with a red background.
        /// </summary>
        Error = Hand,

        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Information = Asterisk,

        /// <summary>
        /// The message box contains a symbol consisting of a white X in a circle with a red background.
        /// </summary>
        Stop = Hand,
    }

    /// <summary>
    /// Shows a modal message box to the user.
    /// </summary>
    public static class MessageBox
    {
#if WINDOWS
        [DllImport( "user32.dll", CharSet = CharSet.Unicode, EntryPoint = "MessageBoxW" )]
        private static extern MessageBoxResult ShowMessageBox( IntPtr hWnd, String text, String caption, int options );
#endif

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static MessageBoxResult Show( string message )
        {
#if WINDOWS
            return ShowMessageBox( IntPtr.Zero, message, string.Empty, 0 );
#else
            Debug.WriteLine( "Cannot show a message box on this system." );
            return MessageBoxResult.Cancel;
#endif
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">The message box's caption.</param>
        public static MessageBoxResult Show( string message, string caption )
        {
#if WINDOWS
            return ShowMessageBox( IntPtr.Zero, message, caption, 0 );
#else
            Debug.WriteLine( "Cannot show a message box on this system." );
            return MessageBoxResult.Cancel;
#endif
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">The message box's caption.</param>
        /// <param name="buttons">The buttons to display.</param>
        public static MessageBoxResult Show( string message, string caption, MessageBoxButtons buttons )
        {
#if WINDOWS
            return ShowMessageBox( IntPtr.Zero, message, caption, (int)buttons );
#else
            Debug.WriteLine( "Cannot show a message box on this system." );
            return MessageBoxResult.Cancel;
#endif
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">The message box's caption.</param>
        /// <param name="buttons">The buttons to display.</param>
        /// <param name="icon">The icon to display.</param>
        public static MessageBoxResult Show( string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon )
        {
#if WINDOWS
            return ShowMessageBox( IntPtr.Zero, message, caption, (int)buttons | (int)icon );
#else
            Debug.WriteLine( "Cannot show a message box on this system." );
            return MessageBoxResult.Cancel;
#endif
        }
    }
}