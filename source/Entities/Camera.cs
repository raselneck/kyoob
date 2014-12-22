using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.VoxelData;

namespace Kyoob.Entities
{
    /// <summary>
    /// The base class for all cameras.
    /// </summary>
    public abstract class Camera
    {
        private BoundingFrustum _frustum;

        /// <summary>
        /// Gets the camera's view matrix.
        /// </summary>
        public virtual Matrix View
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the camera's projection matrix.
        /// </summary>
        public virtual Matrix Projection
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the camera's position.
        /// </summary>
        public virtual Vector3 Position
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the camera's frustum.
        /// </summary>
        public BoundingFrustum Frustum
        {
            get
            {
                return new BoundingFrustum( View * Projection );
            }
        }

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        public Camera()
        {
            View = Matrix.Identity;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                Game.Instance.GraphicsDevice.Viewport.AspectRatio,
                0.01f, 1000.0f
            );
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
        public bool CanSee( BoundingSphere bounds )
        {
            ContainmentType ct = _frustum.Contains( bounds );
            return ct == ContainmentType.Contains
                || ct == ContainmentType.Intersects;
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update( GameTime gameTime )
        {
            _frustum = Frustum;
        }
    }
}