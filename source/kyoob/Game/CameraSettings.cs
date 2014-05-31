using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Game
{
    /// <summary>
    /// A class containing some camera settings.
    /// </summary>
    public sealed class CameraSettings
    {
        private Vector3 _initialPosition;
        private float _initialYaw;
        private float _initialPitch;
        private Vector2 _windowSize;
        private float _fieldOfView;
        private float _clipNear;
        private float _clipFar;

        /// <summary>
        /// Gets or sets the camera's initial position.
        /// </summary>
        public Vector3 InitialPosition
        {
            get
            {
                return _initialPosition;
            }
            set
            {
                _initialPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets the camera's initial yaw.
        /// </summary>
        public float InitialYaw
        {
            get
            {
                return _initialYaw;
            }
            set
            {
                _initialYaw = value;
            }
        }

        /// <summary>
        /// Gets or sets the camera's initial pitch.
        /// </summary>
        public float InitialPitch
        {
            get
            {
                return _initialPitch;
            }
            set
            {
                _initialPitch = value;
            }
        }

        /// <summary>
        /// Gets the window's size.
        /// </summary>
        public Vector2 WindowSize
        {
            get
            {
                return _windowSize;
            }
        }

        /// <summary>
        /// Gets the aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return _windowSize.X / _windowSize.Y;
            }
        }

        /// <summary>
        /// Gets or sets the camera's field of view.
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return _fieldOfView;
            }
            set
            {
                _fieldOfView = value;
            }
        }

        /// <summary>
        /// Gets the near clipping distance.
        /// </summary>
        public float ClipNear
        {
            get
            {
                return _clipNear;
            }
            set
            {
                _clipNear = value;
            }
        }

        /// <summary>
        /// Gets the far clipping distance.
        /// </summary>
        public float ClipFar
        {
            get
            {
                return _clipFar;
            }
            set
            {
                _clipFar = value;
            }
        }

        /// <summary>
        /// Creates new camera settings.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        public CameraSettings( GraphicsDevice device )
        {
            PresentationParameters pp = device.PresentationParameters;

            _windowSize = new Vector2( pp.BackBufferWidth, pp.BackBufferHeight );
            _initialPosition = new Vector3();
            _initialYaw = 0.0f;
            _initialPitch = 0.0f;
            _fieldOfView = MathHelper.PiOver4;
            _clipNear = 0.001f;
            _clipFar = 128.0f;
        }

        /// <summary>
        /// Gets the projection matrix based off of the current settings.
        /// </summary>
        /// <returns></returns>
        public Matrix GetProjection()
        {
            return Matrix.CreatePerspectiveFieldOfView( _fieldOfView, AspectRatio, _clipNear, _clipFar );
        }
    }
}