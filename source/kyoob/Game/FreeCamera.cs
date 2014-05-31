using Kyoob.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kyoob.Game
{
    /// <summary>
    /// An implementation of a free-moving camera.
    /// </summary>
    public sealed class FreeCamera : Camera
    {
        private Vector3 _translation;
        private float _yaw;
        private float _pitch;
        private MouseState _lastMouse;
        private bool _hasControl;

        /// <summary>
        /// Gets the view distance of the camera.
        /// </summary>
        public float ViewDistance
        {
            get
            {
                return GetSettings().ClipFar;
            }
        }

        /// <summary>
        /// Gets the free camera's rotation matrix.
        /// </summary>
        public Matrix Rotation
        {
            get
            {
                return Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f );
            }
        }

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        /// <param name="settings">The camera settings to use.</param>
        public FreeCamera( CameraSettings settings )
            : base( settings )
        {
            _yaw = settings.InitialYaw;
            _pitch = settings.InitialPitch;
            _lastMouse = Mouse.GetState();

            SetTerminalCommands();

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
        }

        /// <summary>
        /// Sets the camera commands in the terminal.
        /// </summary>
        private void SetTerminalCommands()
        {
            // get position
            Terminal.AddCommand( "camera", "pos", ( string[] param ) =>
            {
                Terminal.WriteLine( "[{0:0.00},{1:0.00},{2:0.00}]", Position.X, Position.Y, Position.Z );
            } );

            // set position
            Terminal.AddCommand( "camera", "move", ( string[] param ) =>
            {
                if ( param.Length < 3 )
                {
                    Terminal.WriteError( "Not enough parameters." );
                    return;
                }

                float x = float.Parse( param[ 0 ] );
                float y = float.Parse( param[ 1 ] );
                float z = float.Parse( param[ 2 ] );
                Position = new Vector3( x, y, z );
            } );
        }

        /// <summary>
        /// Takes control of the mouse.
        /// </summary>
        public void TakeControl()
        {
            _hasControl = true;
        }

        /// <summary>
        /// Releases control of the mouse.
        /// </summary>
        public void ReleaseControl()
        {
            _hasControl = false;
        }

        /// <summary>
        /// Rotates the camera.
        /// </summary>
        /// <param name="dYaw">Delta yaw.</param>
        /// <param name="dPitch">Delta pitch.</param>
        public void Rotate( float dYaw, float dPitch )
        {
            _yaw += dYaw;
            _pitch += dPitch;

            // clamp pitch
            if ( _pitch >= MathHelper.PiOver2 )
            {
                _pitch = MathHelper.PiOver2;
            }
            if ( _pitch <= -MathHelper.PiOver2 )
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
        /// Moves the camera.
        /// </summary>
        /// <param name="translation">The amount to move the camera by.</param>
        public void Move( Vector3 translation )
        {
            _translation += translation;
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            if ( _hasControl )
            {
                CheckUserInput( gameTime );
            }
            else
            {
                _lastMouse = Mouse.GetState();
            }
            ApplyTransformations();
        }

        /// <summary>
        /// Checks user input.
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckUserInput( GameTime gameTime )
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();
            float units = (float)gameTime.ElapsedGameTime.TotalSeconds * 6.0f;

            // check if "sprinting"
            if ( keyboard.IsKeyDown( Keys.LeftShift ) )
            {
                units *= 2.50f;
            }

            // check keyboard
            if ( keyboard.IsKeyDown( Keys.W ) )
            {
                Move( Vector3.Forward * units );
            }
            if ( keyboard.IsKeyDown( Keys.S ) )
            {
                Move( Vector3.Backward * units );
            }
            if ( keyboard.IsKeyDown( Keys.A ) )
            {
                Move( Vector3.Left * units );
            }
            if ( keyboard.IsKeyDown( Keys.D ) )
            {
                Move( Vector3.Right * units );
            }
            if ( keyboard.IsKeyDown( Keys.Space ) )
            {
                Move( Vector3.Up * units );
            }
            if ( keyboard.IsKeyDown( Keys.LeftControl ) )
            {
                Move( Vector3.Down * units );
            }

            // check mouse
            float dx = _lastMouse.X - mouse.X;
            float dy = _lastMouse.Y - mouse.Y;
            Rotate( dx * 0.0025f, dy * 0.0025f );

            // reset mouse
            Mouse.SetPosition( (int)GetSettings().WindowSize.X / 2, (int)GetSettings().WindowSize.Y / 2 );
            _lastMouse = Mouse.GetState();
        }

        /// <summary>
        /// Applies transformations.
        /// </summary>
        private void ApplyTransformations()
        {
            // calculate camera's rotation
            Matrix rotation = Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f ); ;

            // translate based on rotation
            _translation = Vector3.Transform( _translation, rotation );
            Position += _translation;
            _translation = Vector3.Zero;

            // get new target and up vectors
            Vector3 forward = Vector3.Transform( Vector3.Forward, rotation );
            Vector3 target = Position + forward;
            Vector3 up = Vector3.Transform( Vector3.Up, rotation );

            // update view matrix and bounding frustum
            View = Matrix.CreateLookAt( Position, target, up );
            Frustum = new BoundingFrustum( View * Projection );
        }
    }
}