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
        private const float PlayerSizeX = 0.8f;
        private const float PlayerSizeY = 1.6f;
        private const float PlayerSizeZ = 0.8f;

        private static Player _instance;
        private MouseState _lastMouse;
        private MouseState _currMouse;
        private KeyboardState _lastKeys;
        private KeyboardState _currKeys;
        private Camera _camera;
        private bool _isFirstUpdate;
        private bool _isNoClipEnabled;

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
        /// Gets or sets whether or not this player can no-clip through terrain.
        /// </summary>
        public bool CanNoClip
        {
            get
            {
                return _isNoClipEnabled;
            }
            set
            {
                if ( value != _isNoClipEnabled )
                {
                    if ( value )
                    {
                        EnableNoClip();
                    }
                    else
                    {
                        DisableNoClip();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the camera used by this player.
        /// </summary>
        public Camera Camera
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
                var pos = _camera.Position;
                if ( !_isNoClipEnabled )
                {
                    pos.Y += PlayerSizeY / 3.0f;
                }
                return pos;
            }
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
        /// Gets the player's position.
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                if ( _isNoClipEnabled && ( _camera is FreeCamera ) )
                {
                    return ( (FreeCamera)_camera ).Position;
                }
                return _position;
            }
            protected set
            {
                if ( _isNoClipEnabled )
                {
                    var camera = ( (FreeCamera)_camera );
                    var dp = value - camera.Position;
                    camera.Move( dp );
                }
                else
                {
                    _position = value;
                }
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
            if ( _isNoClipEnabled )
            {
                UpdateNoClip( gameTime );
            }
            else
            {
                UpdateClip( gameTime );
            }
            base.Update( gameTime );
        }

        /// <summary>
        /// Enables no-clip.
        /// </summary>
        private void EnableNoClip()
        {
            var pc = _camera as PlayerCamera;
            _camera = FreeCamera.FromPlayerCamera( pc );

            _isNoClipEnabled = true;
            ResetPhysics();
        }

        /// <summary>
        /// Disables no-clip.
        /// </summary>
        private void DisableNoClip()
        {
            var fc = _camera as FreeCamera;
            _position = fc.Position;
            _position.Y -= PlayerSizeY / 3.0f;

            _camera = PlayerCamera.FromFreeCamera( fc, this );
            _camera.Update( null );

            _isNoClipEnabled = false;
        }

        /// <summary>
        /// Processes user input.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void ProcessInput( GameTime gameTime )
        {
            // update input states
            _currMouse = Mouse.GetState();
            _currKeys = Keyboard.GetState();

            // get variables
            var camera = _camera as PlayerCamera;
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var lookUnits = time * LookVelocity;
            var moveUnits = time * MoveVelocity;
            if ( _currKeys.IsKeyDown( Keys.LeftShift ) )
            {
                moveUnits *= SprintModifier;
            }

            // get our movement basis vectors
            Vector3 forward, up, right;
            TransformBasisVector( Vector3.Forward, out forward );
            TransformBasisVector( Vector3.Right,   out right );
            if ( camera.Pitch >= 0.0f )
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
            camera.Rotate( lx, ly );

            // update previous states
            Game.Instance.CenterMouse();
            _lastMouse = Mouse.GetState();
            _lastKeys = _currKeys;
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
        /// Updates the player when no-clip is disabled.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void UpdateClip( GameTime gameTime )
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
        }

        /// <summary>
        /// Updates the player when no-clip is enabled.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void UpdateNoClip( GameTime gameTime )
        {
            // get the camera
            var camera = _camera as FreeCamera;
            camera.HandleInput = HandleInput;

            // update the camera
            camera.Update( gameTime );
        }
    }
}