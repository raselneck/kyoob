﻿using System;
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
        private CameraSettings _settings;
        private Matrix _view;
        private Matrix _projection;
        private BoundingFrustum _frustum;
        private BoundingSphere _viewSphere;
        private Vector3 _position;
        private Vector3 _translation;
        private Vector3 _target;
        private float _yaw;
        private float _pitch;
        private MouseState _lastMouse;
        private bool _updated;

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
        /// Gets the camera's position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Gets the camera's rotation.
        /// </summary>
        public Matrix Rotation
        {
            get
            {
                return Matrix.CreateFromYawPitchRoll( _yaw, _pitch, 0.0f );
            }
        }

        /// <summary>
        /// Gets the camera's target direction.
        /// </summary>
        public Vector3 Target
        {
            get
            {
                return _target;
            }
        }

        /// <summary>
        /// Gets the camera's view frustum.
        /// </summary>
        public BoundingFrustum Frustum
        {
            get
            {
                return _frustum;
            }
        }

        /// <summary>
        /// Gets the camera's view sphere.
        /// </summary>
        public BoundingSphere ViewSphere
        {
            get
            {
                return _viewSphere;
            }
        }

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        /// <param name="settings">The camera settings to use.</param>
        public Camera( CameraSettings settings )
        {
            _settings = settings;

            _projection = _settings.GetProjection();
            _view = Matrix.Identity;
            _frustum = new BoundingFrustum( _view * _projection );
            _position = _settings.InitialPosition;
            _yaw = _settings.InitialYaw;
            _pitch = _settings.InitialPitch;
            _target = Vector3.Zero;

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
            _updated = true;

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
            _updated = true;

            _translation += translation;
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public void Update( GameTime gameTime )
        {
            _updated = false;
            CheckUserInput( gameTime );
            if ( _updated )
            {
                ApplyTransformations();
            }
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
            Mouse.SetPosition( (int)_settings.WindowSize.X / 2, (int)_settings.WindowSize.Y / 2 );
            _lastMouse = Mouse.GetState();
        }

        /// <summary>
        /// Applies transformations.
        /// </summary>
        private void ApplyTransformations()
        {
            // calculate camera's rotation
            Matrix rotation = Rotation;

            // translate based on rotation
            _translation = Vector3.Transform( _translation, rotation );
            _position += _translation;
            _translation = Vector3.Zero;

            // get new target and up vectors
            Vector3 forward = Vector3.Transform( Vector3.Forward, rotation );
            _target = _position + forward;
            Vector3 up = Vector3.Transform( Vector3.Up, rotation );

            // update view matrix and bounding frustum
            _view = Matrix.CreateLookAt( _position, _target, up );
            _frustum = new BoundingFrustum( _view * _projection );
            _viewSphere = new BoundingSphere( _position, _settings.ClipFar );
        }
    }
}