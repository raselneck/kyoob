using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kyoob.Entities
{
    /// <summary>
    /// The entity representing the player.
    /// </summary>
    public sealed class Player : Entity
    {
        private static Player _instance;
        private MouseState _lastMouse;
        private MouseState _currMouse;
        private KeyboardState _lastKeys;
        private KeyboardState _currKeys;
        private PlayerCamera _camera;
        private bool _isFirstUpdate;

        /// <summary>
        /// Gets the singleton player instance.
        /// </summary>
        public static Player Instance
        {
            get
            {
                return CreateInstance( Vector3.Zero );
            }
        }

        /// <summary>
        /// Creates the player instance if it isn't already.
        /// </summary>
        /// <param name="position">The player's initial position.</param>
        /// <returns></returns>
        public static Player CreateInstance( Vector3 position )
        {
            if ( _instance == null )
            {
                _instance = new Player( position );
            }
            return _instance;
        }

        /// <summary>
        /// Tells this player whether or not to process input during each update.
        /// </summary>
        public bool HandleInput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this player's look velocity.
        /// </summary>
        public float LookVelocity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the camera used by this player.
        /// </summary>
        public PlayerCamera Camera
        {
            get
            {
                return _camera;
            }
        }

        /// <summary>
        /// Gets the position of this player's eyes level.
        /// </summary>
        public Vector3 EyePosition
        {
            get
            {
                var pos = Position;
                pos.Y += Size.Y / 3.0f;
                return pos;
            }
        }

        /// <summary>
        /// Creates a new player.
        /// </summary>
        private Player()
            : this( Vector3.Zero )
        {
        }

        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="position">The player's initial position.</param>
        private Player( Vector3 position )
            : base()
        {
            Position = position;
            Size = new Vector3( 0.8f, 1.6f, 0.8f );
            _camera = new PlayerCamera( this );
            _isFirstUpdate = true;

            HandleInput = true;
            LookVelocity = 0.0025f;
        }

        /// <summary>
        /// Updates the player.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update( GameTime gameTime )
        {
            if ( HandleInput )
            {
                // if this is the first time, we need to center the mouse
                if ( _isFirstUpdate )
                {
                    Game.Instance.CenterMouse();
                    _currMouse = _lastMouse = Mouse.GetState();
                    _isFirstUpdate = false;
                }

                // process input
                ProcessInput( gameTime );
            }

            // update camera and apply physics irregardless of input handling
            _camera.Update( gameTime );
            ApplyPhysics( gameTime );

            base.Update( gameTime );
        }

        /// <summary>
        /// Transforms a basis vector that can be used for movement.
        /// </summary>
        /// <param name="basis">The basis vector.</param>
        /// <param name="transformed">The transformed vector to populate.</param>
        private void TransformBasisVector( Vector3 basis, out Vector3 transformed )
        {
            Matrix rotation = _camera.Rotation;
            Vector3.Transform( ref basis, ref rotation, out transformed );
            transformed.Y = 0.0f;
            transformed.Normalize();
        }

        /// <summary>
        /// Processes user input.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void ProcessInput( GameTime gameTime )
        {
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currMouse = Mouse.GetState();
            _currKeys = Keyboard.GetState();
            float lookUnits = time * LookVelocity;
            float moveUnits = time * MoveVelocity;

            // move faster if holding shift
            if ( _currKeys.IsKeyDown( Keys.LeftShift ) )
            {
                moveUnits *= SprintModifier;
            }

            // get our movement basis vectors
            Vector3 forward, up, right;
            TransformBasisVector( Vector3.Forward, out forward );
            TransformBasisVector( Vector3.Right,   out right );
            if ( _camera.Pitch >= 0.0f )
            {
                TransformBasisVector( Vector3.Down, out up );
            }
            else
            {
                TransformBasisVector( Vector3.Up, out up );
            }

            // check keyboard for which direction to move
            Vector3 move = Vector3.Zero;
            if ( _currKeys.IsKeyDown( Keys.W ) )
            {
                move += forward;
                move += up;
            }
            if ( _currKeys.IsKeyDown( Keys.S ) )
            {
                move -= forward;
                move -= up;
            }
            if ( _currKeys.IsKeyDown( Keys.D ) )
            {
                move += right;
            }
            if ( _currKeys.IsKeyDown( Keys.A ) )
            {
                move -= right;
            }
            if ( _currKeys.IsKeyDown( Keys.Space ) )
            {
                Jump( gameTime );
            }

            // check mouse for rotating
            float lx = ( _lastMouse.X - _currMouse.X ) * LookVelocity;
            float ly = ( _lastMouse.Y - _currMouse.Y ) * LookVelocity;

            // move and rotate the camera
            move.Normalize();
            Move( move * moveUnits );
            _camera.Rotate( lx, ly );

            // update previous states
            Game.Instance.CenterMouse();
            _lastMouse = Mouse.GetState();
            _lastKeys = _currKeys;
        }
    }
}