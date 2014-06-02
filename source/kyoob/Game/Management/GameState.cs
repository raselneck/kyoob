using System;
using Microsoft.Xna.Framework;

namespace Kyoob.Game.Management
{
    /// <summary>
    /// The base class for game states.
    /// </summary>
    public abstract class GameState
    {
        private StateSystem _controller;

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
        }

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