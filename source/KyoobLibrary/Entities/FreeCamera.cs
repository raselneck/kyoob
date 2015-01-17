using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kyoob.Entities
{
    /// <summary>
    /// A camera that can be used to freely navigate space.
    /// </summary>
    public class FreeCamera : Camera
    {
        private Vector3 _translation;
        private float _yaw;
        private float _pitch;
        private MouseState _lastMouse;
        private MouseState _currMouse;
        private KeyboardState _lastKeys;
        private KeyboardState _currKeys;

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
        /// Tells this camera whether or not to process input during each update.
        /// </summary>
        public bool HandleInput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this camera's look velocity.
        /// </summary>
        public float LookVelocity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this camera's movement velocity.
        /// </summary>
        public float MoveVelocity
        {
            get;
            set;
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
        /// Creates a new free camera.
        /// </summary>
        public FreeCamera()
            : this( Vector3.Zero )
        {
        }

        /// <summary>
        /// Creates a new free camera.
        /// </summary>
        /// <param name="position">The camera's initial position.</param>
        public FreeCamera( Vector3 position )
        {
            _translation = new Vector3();

            HandleInput = true;
            LookVelocity = 0.0025f;
            MoveVelocity = 6.0f;
            Position = position;

            _lastMouse = _currMouse = Mouse.GetState();
            _lastKeys = _currKeys = Keyboard.GetState();
        }

        /// <summary>
        /// Rotates this camera.
        /// </summary>
        /// <param name="xAxis">The amount to rotate around the X-axis.</param>
        /// <param name="yAxis">The amount to rotate around the Y-axis.</param>
        public void Rotate( float xAxis, float yAxis )
        {
            // xAxis == yaw
            _yaw += xAxis;
            while ( _yaw >= MathHelper.TwoPi ) _yaw -= MathHelper.TwoPi;
            while ( _yaw < 0.0f ) _yaw += MathHelper.TwoPi;

            // yAxis == pitch
            _pitch = MathHelper.Clamp( _pitch + yAxis, -MathHelper.PiOver2, MathHelper.PiOver2 );
        }

        /// <summary>
        /// Moves the camera.
        /// </summary>
        /// <param name="x">The amount to move along the local X axis.</param>
        /// <param name="y">The amount to move along the local Y axis.</param>
        /// <param name="z">The amount to move along the local Z axis.</param>
        public void Move( float x, float y, float z )
        {
            _translation.X += x;
            _translation.Y += y;
            _translation.Z += z;
        }

        /// <summary>
        /// Moves the camera.
        /// </summary>
        /// <param name="amount">The amount to move.</param>
        public void Move( Vector3 amount )
        {
            _translation += amount;
        }

        /// <summary>
        /// Updates this free camera.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update( GameTime gameTime )
        {
            // handle input if necessary
            if ( HandleInput )
            {
                ProcessInput( (float)gameTime.ElapsedGameTime.TotalSeconds );
            }

            // calculate rotation vector
            Rotation = Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f );

            // move
            _translation = Vector3.Transform( _translation, Rotation );
            Position += _translation;
            _translation = Vector3.Zero;

            // calculate forward and up
            Forward = Vector3.Transform( Vector3.Forward, Rotation );
            Up = Vector3.Transform( Vector3.Up, Rotation );

            // re-calculate view
            View = Matrix.CreateLookAt( Position, Position + Forward, Up );

            base.Update( gameTime );
        }

        /// <summary>
        /// Processes user input.
        /// </summary>
        /// <param name="time">The current time step.</param>
        private void ProcessInput( float time )
        {
            _currMouse = Mouse.GetState();
            _currKeys = Keyboard.GetState();
            float lookUnits = time * LookVelocity;
            float moveUnits = time * MoveVelocity;

            // move faster if holding shift
            if ( _currKeys.IsKeyDown( Keys.LeftShift ) )
            {
                moveUnits *= 2.25f;
            }

            // check keyboard for which direction to move
            if ( _currKeys.IsKeyDown( Keys.W ) )
            {
                Move( Vector3.Forward * moveUnits );
            }
            if ( _currKeys.IsKeyDown( Keys.S ) )
            {
                Move( Vector3.Backward * moveUnits );
            }
            if ( _currKeys.IsKeyDown( Keys.A ) )
            {
                Move( Vector3.Left * moveUnits );
            }
            if ( _currKeys.IsKeyDown( Keys.D ) )
            {
                Move( Vector3.Right * moveUnits );
            }
            if ( _currKeys.IsKeyDown( Keys.Space ) )
            {
                Move( Vector3.Up * moveUnits );
            }
            if ( _currKeys.IsKeyDown( Keys.LeftControl ) )
            {
                Move( Vector3.Down * moveUnits );
            }

            // check mouse for rotating
            float lx = ( _lastMouse.X - _currMouse.X ) * LookVelocity;
            float ly = ( _lastMouse.Y - _currMouse.Y ) * LookVelocity;
            Rotate( lx, ly );

            // update previous states
            Game.Instance.CenterMouse();
            _lastMouse = Mouse.GetState();
            _lastKeys = _currKeys;
        }

        /// <summary>
        /// Creates a free camera from a player camera.
        /// </summary>
        /// <param name="playerCamera">The player camera to use as a basis.</param>
        /// <returns></returns>
        internal static FreeCamera FromPlayerCamera( PlayerCamera playerCamera )
        {
            var fc = new FreeCamera( playerCamera.Player.EyePosition );
            fc._yaw = playerCamera.Yaw;
            fc._pitch = playerCamera.Pitch;
            return fc;
        }
    }
}