using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Debug;
using Kyoob.Effects;

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// An implementation for the player.
    /// </summary>
    public class Player : Entity
    {
        private const float JumpVelocity = 12.0f;

        private PlayerCamera _camera;
        private KeyboardState _currKeys;
        private KeyboardState _prevKeys;

        /// <summary>
        /// Gets the player's camera.
        /// </summary>
        public PlayerCamera Camera
        {
            get
            {
                return _camera;
            }
        }

        /// <summary>
        /// Gets the player's size.
        /// </summary>
        public override Vector3 Size
        {
            get
            {
                return new Vector3( 0.8f, 1.6f, 0.8f );
            }
        }

        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="device">The graphics device to create the player on.</param>
        /// <param name="settings">The settings for the player's camera.</param>
        public Player( GraphicsDevice device, CameraSettings settings )
            : base( device )
        {
            _camera = new PlayerCamera( settings, this );
            _camera.EyeHeight = 0.6f;
            _currKeys = Keyboard.GetState();
            _prevKeys = Keyboard.GetState();
        }

        /// <summary>
        /// Gets a movement vector.
        /// </summary>
        /// <param name="basis">The basis vector.</param>
        /// <param name="units">The "units per second" to multiply by.</param>
        /// <returns></returns>
        private Vector3 GetMovementVector( Vector3 basis, float units )
        {
            Vector3 transformed = Vector3.Transform( basis, _camera.Rotation );
            transformed.Y = 0.0f;
            return transformed * units;
        }

        /// <summary>
        /// Checks user input.
        /// </summary>
        /// <param name="time">The total number of seconds since the last frame.</param>
        private void CheckUserInput( float time )
        {
            // calculate our "units per second"
            float units = time * 6.0f;
            if ( _currKeys.IsKeyDown( Keys.LeftShift ) )
            {
                units *= 2.25f;
            }

            // get transformed basis vectors
            Vector3 forward = GetMovementVector( Vector3.Forward, units );
            Vector3 up      = GetMovementVector( Vector3.Up, units );
            Vector3 right   = GetMovementVector( Vector3.Right, units );
            if ( _camera.Pitch >= 0.0f )
            {
                up = -up;
            }

            // check if we need to strafe forward
            if ( _currKeys.IsKeyDown( Keys.W ) )
            {
                Translation += forward;
                Translation += up;
            }

            // check if we need to strafe backward
            if ( _currKeys.IsKeyDown( Keys.S ) )
            {
                Translation -= forward;
                Translation -= up;
            }

            // check if we need to strafe right
            if ( _currKeys.IsKeyDown( Keys.D ) )
            {
                Translation += right;
            }

            // check if we need to strafe left
            if ( _currKeys.IsKeyDown( Keys.A ) )
            {
                Translation -= right;
            }

            // check if we need to jump
            if ( _currKeys.IsKeyDown( Keys.Space ) )
            {
                Jump( time, JumpVelocity );
            }
        }

        /// <summary>
        /// Updates the player.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            _currKeys = Keyboard.GetState();

            // we really only need to update if we're in a world
            if ( World != null )
            {
                float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if ( _camera.HasControl )
                {
                    CheckUserInput( time );
                    ApplyPhysics( time );
                }

                // move the player and reset out translation vector
                Move( Translation );
                Translation = Vector3.Zero;
            }
            _camera.Update( gameTime );

            _prevKeys = _currKeys;
        }

        /// <summary>
        /// Draws the player.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="effect">The effect to use when drawing.</param>
        public override void Draw( GameTime gameTime, BaseEffect effect )
        {
#if DEBUG
            //Chunk current = _world.GetChunk( Position );
            //if ( current != null )
            //{
            //    current.Octree.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
            //    current.Bounds.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Black );
            //}

            BoundingBox box = GetBounds( 0.0f, 0.0f, 0.0f );
            box.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
#endif
        }
    }
}