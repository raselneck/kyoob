using Kyoob.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// An implementation for the player.
    /// </summary>
    public class Player : Entity
    {
        private const float JumpVelocity   = 10.00f;
        private const float MoveVelocity   =  2.00f;
        private const float SprintVelocity =  1.50f * MoveVelocity;

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
            _camera.LookVelocity = 0.0016f;

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
            return Vector3.Normalize( transformed ) * units;
        }

        /// <summary>
        /// Checks user input.
        /// </summary>
        /// <param name="time">The total number of seconds since the last frame.</param>
        private void CheckUserInput( float time )
        {
            // calculate our "units per second"
            float units = time * MoveVelocity;
            if ( _currKeys.IsKeyDown( Keys.LeftShift ) )
            {
                units = time * SprintVelocity;
            }

            // get transformed basis vectors
            Vector3 forward = GetMovementVector( Vector3.Forward, units );
            Vector3 up      = GetMovementVector( Vector3.Up,      units );
            Vector3 right   = GetMovementVector( Vector3.Right,   units );
            if ( _camera.Pitch >= 0.0f )
            {
                up = -up;
            }

            // check if we need to strafe forward
            if ( _currKeys.IsKeyDown( Keys.W ) )
            {
                Move( forward );
                Move( up );
            }

            // check if we need to strafe backward
            if ( _currKeys.IsKeyDown( Keys.S ) )
            {
                Move( -forward );
                Move( -up );
            }

            // check if we need to strafe right
            if ( _currKeys.IsKeyDown( Keys.D ) )
            {
                Move( right );
            }

            // check if we need to strafe left
            if ( _currKeys.IsKeyDown( Keys.A ) )
            {
                Move( -right );
            }

            // check if we need to jump
            if ( _currKeys.IsKeyDown( Keys.Space ) )
            {
                Jump( JumpVelocity * time );
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

                // call base.Update() to move the player
                base.Update( gameTime );
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
            Bounds.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
#endif
        }
    }
}