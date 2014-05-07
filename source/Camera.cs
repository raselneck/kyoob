using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kyoob
{
    /// <summary>
    /// An implementation of a free-moving camera.
    /// </summary>
    public sealed class Camera
    {
        /// <summary>
        /// The matrix used to fix the bounding frustum so that it doesn't over-cull.
        /// </summary>
        private static readonly Matrix FrustumFix = Matrix.CreateScale( 0.125f, 0.45f, 1.0f );

        private GraphicsDevice _device;
        private Matrix _view;
        private Matrix _projection;
        private BoundingFrustum _frustum;
        private Vector3 _position;
        private Vector3 _translation;
        private float _yaw;
        private float _pitch;
        private MouseState _lastMouse;

        /// <summary>
        /// Gets the camera's view matrix.
        /// </summary>
        public Matrix View
        {
            get
            {
                return _view;
            }
        }

        /// <summary>
        /// Gets the camera's projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return _projection;
            }
        }

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        /// <param name="device">The device to use.</param>
        /// <param name="position">The camera's initial position.</param>
        /// <param name="yaw">The camera's initial yaw.</param>
        /// <param name="pitch">The camera's initial pitch.</param>
        public Camera( GraphicsDevice device, Vector3 position, float yaw, float pitch )
        {
            _device = device;
            PresentationParameters pp = _device.PresentationParameters;
            float aspect = (float)pp.BackBufferWidth / pp.BackBufferHeight;

            _projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver4, aspect, 0.01f, 500.0f );
            _view = Matrix.Identity;
            _frustum = new BoundingFrustum( _view * _projection );
            _position = position;
            _yaw = yaw;
            _pitch = pitch;

            _lastMouse = Mouse.GetState();
        }

        /// <summary>
        /// Checks to see if this camera can see an object.
        /// </summary>
        /// <param name="bounds">The object's bounds.</param>
        /// <returns></returns>
        public bool CanSee( BoundingBox bounds )
        {
            ContainmentType ct = _frustum.Contains( bounds );
            return ct == ContainmentType.Contains
                || ct == ContainmentType.Intersects;
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
        public void Update( GameTime gameTime )
        {
            CheckUserInput( gameTime );
            ApplyTranslations();
        }

        /// <summary>
        /// Checks user input.
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckUserInput( GameTime gameTime )
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();
            float units = (float)gameTime.ElapsedGameTime.TotalSeconds * 4.0f;

            // check if "sprinting"
            if ( keyboard.IsKeyDown( Keys.LeftShift ) )
            {
                units *= 2.25f;
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
            PresentationParameters pp = _device.PresentationParameters;
            Mouse.SetPosition( pp.BackBufferWidth / 2, pp.BackBufferHeight / 2 );
            _lastMouse = Mouse.GetState();
        }

        /// <summary>
        /// Applies translations.
        /// </summary>
        private void ApplyTranslations()
        {
            // calculate camera's rotation
            Matrix rotation = Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f );

            // translate based on rotation
            _translation = Vector3.Transform( _translation, rotation );
            _position += _translation;
            _translation = Vector3.Zero;

            // get new target and up vectors
            Vector3 forward = Vector3.Transform( Vector3.Forward, rotation );
            Vector3 target = _position + forward;
            Vector3 up = Vector3.Transform( Vector3.Up, rotation );

            // update view matrix and bounding frustum
            _view = Matrix.CreateLookAt( _position, target, up );
            _frustum = new BoundingFrustum( _view * _projection * FrustumFix );
        }
    }
}