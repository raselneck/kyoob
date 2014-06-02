using System;
using System.Collections.Generic;
using Kyoob.Blocks;
using Kyoob.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Game.Management
{
    /// <summary>
    /// The main class for dealing with game states.
    /// </summary>
    public sealed class StateSystem
    {
        private World _world;
        private GameState _currentState;
        private KyoobEngine _engine;
        private KyoobSettings _settings;
        private Dictionary<string, GameState> _states;

        /// <summary>
        /// Gets or sets the current world to be used by the game states.
        /// </summary>
        public World World
        {
            get
            {
                return _world;
            }
        }

        /// <summary>
        /// Gets the Kyoob engine controlling this state system.
        /// </summary>
        public KyoobEngine Engine
        {
            get
            {
                return _engine;
            }
        }

        /// <summary>
        /// Gets the global settings in use by this state system.
        /// </summary>
        public KyoobSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        /// <summary>
        /// Creates a new state system.
        /// </summary>
        /// <param name="engine">The Kyoob engine to use.</param>
        /// <param name="settings">The Kyoob global settings to use.</param>
        /// <param name="world">The world to use.</param>
        public StateSystem( KyoobEngine engine, KyoobSettings settings, World world )
        {
            _world = world;
            _engine = engine;
            _settings = settings;
            _currentState = null;
            _states = new Dictionary<string, GameState>();
        }

        /// <summary>
        /// Adds a game state to the state system.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <param name="state">The state to switch to.</param>
        public void AddState( string name, GameState state )
        {
            _states.Add( name, state );
        }

        /// <summary>
        /// Gets the state with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public GameState GetState( string name )
        {
            GameState state;
            _states.TryGetValue( name, out state );
            return state;
        }

        /// <summary>
        /// Changes the current state.
        /// </summary>
        /// <param name="name">The name of the state to switch to.</param>
        public void ChangeState( string name )
        {
            _states.TryGetValue( name, out _currentState );
        }

        /// <summary>
        /// Updates the game state system.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public void Update( GameTime gameTime )
        {
            if ( _currentState != null )
            {
                _currentState.Update( gameTime );
            }
        }

        /// <summary>
        /// Draws the game state system.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public void Draw( GameTime gameTime )
        {
            if ( _currentState != null )
            {
                _currentState.Draw( gameTime );
            }
        }
    }
}