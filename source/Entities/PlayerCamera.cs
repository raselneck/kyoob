using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kyoob.Entities
{
    /// <summary>
    /// The camera specifically used by the player.
    /// </summary>
    /// <remarks>
    /// This is very similar to a free camera, but uses the player's position as the camera position.
    /// </remarks>
    public sealed class PlayerCamera : Camera
    {
        private Player _player;
        private float _yaw;
        private float _pitch;

        /// <summary>
        /// Gets the position of this camera.
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                return _player.Position;
            }
            protected set
            {
                Debug.WriteLine( "Cannot set the position of the player camera" );
            }
        }

        /// <summary>
        /// Gets this camera's rotation matrix.
        /// </summary>
        public Matrix Rotation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets this camera's forward vector.
        /// </summary>
        public Vector3 Forward
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets this camera's up vector.
        /// </summary>
        public Vector3 Up
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets this camera's yaw.
        /// </summary>
        public float Yaw
        {
            get
            {
                return _yaw;
            }
        }

        /// <summary>
        /// Gets this camera's pitch.
        /// </summary>
        public float Pitch
        {
            get
            {
                return _pitch;
            }
        }

        /// <summary>
        /// Creates a new player camera.
        /// </summary>
        /// <param name="player">The player that is using this camera.</param>
        public PlayerCamera( Player player )
        {
            _player = player;
            _yaw = -MathHelper.PiOver4; // points us towards the sun
        }

        /// <summary>
        /// Rotates this camera.
        /// </summary>
        /// <param name="xAxis">The amount to rotate around the X-axis.</param>
        /// <param name="yAxis">The amount to rotate around the Y-axis.</param>
        public void Rotate( float xAxis, float yAxis )
        {
            // xAxis == yaw
            if ( !float.IsNaN( xAxis ) )
            {
                _yaw += xAxis;
                while ( _yaw >= MathHelper.TwoPi ) _yaw -= MathHelper.TwoPi;
                while ( _yaw < 0.0f ) _yaw += MathHelper.TwoPi;
            }

            // yAxis == pitch
            if ( !float.IsNaN( Yaw ) )
            {
                _pitch = MathHelper.Clamp( _pitch + yAxis, -MathHelper.PiOver2, MathHelper.PiOver2 );
            }
        }

        /// <summary>
        /// Updates this free camera.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update( GameTime gameTime )
        {
            // calculate rotation vector
            Rotation = Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f );

            // calculate forward and up
            Forward = Vector3.Transform( Vector3.Forward, Rotation );
            Up = Vector3.Transform( Vector3.Up, Rotation );

            // re-calculate view
            Vector3 eye = _player.EyePosition;
            View = Matrix.CreateLookAt( eye, eye + Forward, Up );

            base.Update( gameTime );
        }
    }
}