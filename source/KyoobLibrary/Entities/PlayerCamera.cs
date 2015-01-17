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
        public override Matrix Rotation
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets this camera's forward vector.
        /// </summary>
        public override Vector3 Forward
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets this camera's up vector.
        /// </summary>
        public override Vector3 Up
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the player used by this player camera.
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
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
            private set
            {
                _yaw = value;
                while ( _yaw >= MathHelper.TwoPi )
                {
                    _yaw -= MathHelper.TwoPi;
                }
                while ( _yaw < 0.0f )
                {
                    _yaw += MathHelper.TwoPi;
                }
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
            private set
            {
                _pitch = value;
                _pitch = MathHelper.Clamp( _pitch, -MathHelper.PiOver2, MathHelper.PiOver2 );
            }
        }

        /// <summary>
        /// Creates a new player camera.
        /// </summary>
        /// <param name="player">The player that is using this camera.</param>
        public PlayerCamera( Player player )
        {
            _player = player;
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
                //_yaw += xAxis;
                //while ( _yaw >= MathHelper.TwoPi ) _yaw -= MathHelper.TwoPi;
                //while ( _yaw < 0.0f ) _yaw += MathHelper.TwoPi;
                Yaw += xAxis;
            }

            // yAxis == pitch
            if ( !float.IsNaN( Yaw ) )
            {
                //_pitch = MathHelper.Clamp( _pitch + yAxis, -MathHelper.PiOver2, MathHelper.PiOver2 );
                Pitch += yAxis;
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

        /// <summary>
        /// Creates a player camera from a free camera.
        /// </summary>
        /// <param name="freeCamera">The free camera to use as a basis.</param>
        /// <param name="player">The player to attach to.</param>
        /// <returns></returns>
        internal static PlayerCamera FromFreeCamera( FreeCamera freeCamera, Player player )
        {
            var pc = new PlayerCamera( player );
            pc._yaw = freeCamera.Yaw;
            pc._pitch = freeCamera.Pitch;
            return pc;
        }
    }
}