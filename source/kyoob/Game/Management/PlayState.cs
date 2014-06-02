using System;
using Kyoob.Blocks;
using Kyoob.Debug;
using Kyoob.Effects;
using Kyoob.Game.Entities;
using Kyoob.Graphics;
using Microsoft.Xna.Framework;

namespace Kyoob.Game.Management
{
    /// <summary>
    /// The game state for actually "playing."
    /// </summary>
    public sealed class PlayState : GameState
    {
        private Player _player;
        private WorldEffect _effect;

        /// <summary>
        /// Gets the player.
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
        }

        /// <summary>
        /// Creates a new play state.
        /// </summary>
        /// <param name="controller">The state system controlling this state.</param>
        /// <param name="effect">The world effect to use.</param>
        public PlayState( StateSystem controller, WorldEffect effect )
            : base( controller )
        {
            _effect = effect;
            _player = new Player( Controller.Settings );
            _player.World = Controller.World;
        }

        /// <summary>
        /// Updates the play state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            Terminal.Update( gameTime );
            _player.Update( gameTime );
            Controller.World.Update( gameTime, _player.Camera );

            // update the effect
            _effect.LightPosition = _player.Camera.EyePosition;
            _effect.Projection = _player.Camera.Projection;
            _effect.View = _player.Camera.View;
        }

        /// <summary>
        /// Draws the play state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Draw( GameTime gameTime )
        {
            Controller.World.Draw( gameTime, _player.Camera );
            _player.Draw( gameTime, _effect );
            Terminal.Draw( gameTime );
        }
    }
}