using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Game
{
    /// <summary>
    /// The base class for all cameras.
    /// </summary>
    public abstract class Camera
    {
        private CameraSettings _settings;
        private Matrix _view;
        private Matrix _projection;
        private Vector3 _position;
        private BoundingFrustum _frustum;

        /// <summary>
        /// Gets the camera's view matrix.
        /// </summary>
        public virtual Matrix View
        {
            get
            {
                return _view;
            }
            protected set
            {
                _view = value;
            }
        }

        /// <summary>
        /// Gets the camera's projection matrix.
        /// </summary>
        public virtual Matrix Projection
        {
            get
            {
                return _projection;
            }
            protected set
            {
                _projection = value;
            }
        }

        /// <summary>
        /// Gets the camera's position.
        /// </summary>
        public virtual Vector3 Position
        {
            get
            {
                return _position;
            }
            protected set
            {
                _position = value;
            }
        }

        /// <summary>
        /// Gets the camera's frustum.
        /// </summary>
        public BoundingFrustum Frustum
        {
            get
            {
                return _frustum;
            }
            protected set
            {
                _frustum = value;
            }
        }

        /// <summary>
        /// Creates a new camera with the given settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public Camera( CameraSettings settings )
        {
            _settings = settings;

            _projection = _settings.GetProjection();
            _view = Matrix.Identity;
            _frustum = new BoundingFrustum( _view * _projection );
            _position = _settings.InitialPosition;
        }

        /// <summary>
        /// Gets the camera's settings.
        /// </summary>
        /// <returns></returns>
        public CameraSettings GetSettings()
        {
            return _settings;
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
        /// Checks to see if this camera can see an object.
        /// </summary>
        /// <param name="bounds">The object's bounds.</param>
        /// <returns></returns>
        public bool CanSee( BoundingSphere bounds )
        {
            ContainmentType ct = _frustum.Contains( bounds );
            return ct == ContainmentType.Contains
                || ct == ContainmentType.Intersects;
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public abstract void Update( GameTime gameTime );
    }
}