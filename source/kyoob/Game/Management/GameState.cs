using System;
using Microsoft.Xna.Framework;

namespace Kyoob.Game.Management
{
    /// <summary>
    /// The base class for game states.
    /// </summary>
    public abstract class GameState : IDisposable
    {
        private StateSystem _controller;

        /// <summary>
        /// The event that is called when this game state gains focus.
        /// </summary>
        public event EventHandler<EventArgs> SwitchTo;

        /// <summary>
        /// Gets the state system controlling this game state.
        /// </summary>
        public StateSystem Controller
        {
            get
            {
                return _controller;
            }
        }

        /// <summary>
        /// Creates a new game state.
        /// </summary>
        /// <param name="controller">The state system controlling this game state.</param>
        public GameState( StateSystem controller )
        {
            _controller = controller;

            // add a dummy event handler just so that it's not null
            SwitchTo += ( object sender, EventArgs args ) => { };
        }

        /// <summary>
        /// Triggers the SwitchTo event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public virtual void OnSwitchTo( EventArgs args )
        {
            SwitchTo( this, args );
        }

        /// <summary>
        /// Disposes of this game state.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Updates the game state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public abstract void Update( GameTime gameTime );

        /// <summary>
        /// Draws the game state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public abstract void Draw( GameTime gameTime );
    }
}