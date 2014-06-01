using Kyoob.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#warning TODO : Figure out how to push the camera view position forward a bit based on rotation so its closer to the bound's edge.

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
        private GraphicsDevice _device;
        private Matrix _rotation;
        private float _yaw;
        private float _pitch;
        private float _eyeHeight;
        private float _lookVelocity;
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
        /// Gets or sets the camera's look velocity.
        /// </summary>
        public float LookVelocity
        {
            get
            {
                return _lookVelocity;
            }
            set
            {
                _lookVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the height that the eyes lie at based off of the position's Y value.
        /// </summary>
        public float EyeHeight
        {
            get
            {
                return _eyeHeight;
            }
            set
            {
                _eyeHeight = value;
            }
        }

        /// <summary>
        /// The eye position of the camera.
        /// </summary>
        public Vector3 EyePosition
        {
            get
            {
                return new Vector3(
                    Position.X,
                    Position.Y + _eyeHeight,
                    Position.Z
                );
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
        /// <param name="settings">The global settings to use.</param>
        /// <param name="player">The player to be linked to.</param>
        public PlayerCamera( KyoobSettings settings, Player player )
            : base( settings )
        {
            _device = settings.GraphicsDevice;
            _currMouse = Mouse.GetState();
            _prevMouse = Mouse.GetState();
            _player = player;
            _player.MoveTo( Settings.InitialPosition );
            _yaw = Settings.InitialYaw;
            _pitch = Settings.InitialPitch;
            _eyeHeight = _player.Size.Y / 4.0f;
            _lookVelocity = 0.04f;

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
            // none yet
        }

        /// <summary>
        /// Centers the mouse.
        /// </summary>
        private void CenterMouse()
        {
            int width  = _device.PresentationParameters.BackBufferWidth;
            int height = _device.PresentationParameters.BackBufferHeight;
            Mouse.SetPosition( width / 2, height / 2 );
        }

        /// <summary>
        /// Rotates the camera based on cached mouse states.
        /// </summary>
        private void Rotate( float time )
        {
            // modify the yaw and pitch
            int dYaw    = _prevMouse.X - _currMouse.X;
            int dPitch  = _prevMouse.Y - _currMouse.Y;
            _yaw       += _lookVelocity * dYaw * time;
            _pitch     += _lookVelocity * dPitch * time;

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

            // get new vectors
            Vector3 forward  = Vector3.Transform( Vector3.Forward, _rotation );
            Vector3 target   = Position + forward;
            Vector3 up       = Vector3.Transform( Vector3.Up, _rotation );
            
            // update view matrix and bounding frustum
            View = Matrix.CreateLookAt( EyePosition, target, up );
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
                float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Rotate( time );
                CenterMouse();
            }
            ApplyTransformations();

            _prevMouse = Mouse.GetState();
        }
    }
}