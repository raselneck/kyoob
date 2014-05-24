using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Debug;

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// An implementation of a camera specifically for the player.
    /// </summary>
    public class PlayerCamera : Camera
    {
        private MouseState _currMouse;
        private MouseState _prevMouse;
        private Player _player;
        private Matrix _rotation;
        private float _yaw;
        private float _pitch;
        private bool _hasControl;

        /// <summary>
        /// Gets or sets the attached player's position.
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                return _player.Position;
            }
            protected set
            {
                _player.MoveTo( value );
            }
        }

        /// <summary>
        /// Gets the camera's rotation matrix.
        /// </summary>
        public Matrix Rotation
        {
            get
            {
                return _rotation;
            }
        }

        /// <summary>
        /// Gets the camera's yaw.
        /// </summary>
        public float Yaw
        {
            get
            {
                return _yaw;
            }
        }

        /// <summary>
        /// Gets the camera's pitch.
        /// </summary>
        public float Pitch
        {
            get
            {
                return _pitch;
            }
        }

        /// <summary>
        /// Gets whether or not the camera has control.
        /// </summary>
        public bool HasControl
        {
            get
            {
                return _hasControl;
            }
        }

        /// <summary>
        /// Creates a new player camera.
        /// </summary>
        /// <param name="settings">The camera settings to use.</param>
        /// <param name="player">The player to be linked to.</param>
        public PlayerCamera( CameraSettings settings, Player player )
            : base( settings )
        {
            _currMouse = Mouse.GetState();
            _prevMouse = Mouse.GetState();
            _player = player;
            _player.MoveTo( settings.InitialPosition );
            _yaw = settings.InitialYaw;
            _pitch = settings.InitialPitch;

            // set our control data
            _hasControl = true;
            Terminal.RequestControl += ( sender, args ) =>
            {
                _hasControl = false;
            };
            Terminal.ReleaseControl += ( sender, args ) =>
            {
                _hasControl = true;
            };

            SetTerminalCommands();
        }

        /// <summary>
        /// Sets the terminal commands.
        /// </summary>
        private void SetTerminalCommands()
        {
            // camera.pos
            Terminal.AddCommand( "camera", "pos", ( string[] param ) =>
            {
                Vector3 pos = Position;
                Terminal.WriteInfo( "[{0:0.00},{1:0.00},{2:0.00}]", pos.X, pos.Y, pos.Z );
            } );
        }

        /// <summary>
        /// Centers the mouse.
        /// </summary>
        private void CenterMouse()
        {
            int width  = _player.GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = _player.GraphicsDevice.PresentationParameters.BackBufferHeight;
            Mouse.SetPosition( width / 2, height / 2 );
        }

        /// <summary>
        /// Rotates the camera based on cached mouse states.
        /// </summary>
        private void Rotate()
        {
            // modify the yaw and pitch
            int dYaw    = _prevMouse.X - _currMouse.X;
            int dPitch  = _prevMouse.Y - _currMouse.Y;
            _yaw       += 0.0025f * dYaw;
            _pitch     += 0.0025f * dPitch;

            // clamp pitch
            if ( _pitch > MathHelper.PiOver2 )
            {
                _pitch = MathHelper.PiOver2;
            }
            if ( _pitch < -MathHelper.PiOver2 )
            {
                _pitch = -MathHelper.PiOver2;
            }

            // clamp yaw
            if ( _yaw >= MathHelper.TwoPi )
            {
                _yaw -= MathHelper.TwoPi;
            }
            if ( _yaw <= -MathHelper.TwoPi )
            {
                _yaw += MathHelper.TwoPi;
            }
        }

        /// <summary>
        /// Applies transformations.
        /// </summary>
        private void ApplyTransformations()
        {
            // calculate camera's rotation
            _rotation = Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f ); ;

            // get new target and up vectors
            Vector3 forward = Vector3.Transform( Vector3.Forward, _rotation );
            Vector3 target = Position + forward;
            Vector3 up = Vector3.Transform( Vector3.Up, _rotation );

            // update view matrix and bounding frustum
            View = Matrix.CreateLookAt( Position, target, up );
            Frustum = new BoundingFrustum( View * Projection );
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            _currMouse = Mouse.GetState();

            // update based on whether or not we have control
            if ( _hasControl )
            {
                Rotate();
                CenterMouse();
            }
            ApplyTransformations();

            _prevMouse = Mouse.GetState();
        }
    }
}