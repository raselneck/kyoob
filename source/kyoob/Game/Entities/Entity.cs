using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// The base class for entities.
    /// </summary>
    public abstract class Entity
    {
        private GraphicsDevice _device;
        private Vector3 _position;

        /// <summary>
        /// Gets the graphics device this entity is on.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
        }

        /// <summary>
        /// Gets the position of the entity.
        /// </summary>
        public virtual Vector3 Position
        {
            get
            {
                // position is also the center of the size
                return _position;
            }
        }

        /// <summary>
        /// Gets the entity's world matrix.
        /// </summary>
        public virtual Matrix WorldMatrix
        {
            get
            {
                return Matrix.CreateTranslation( _position );
            }
        }

        /// <summary>
        /// Gets this entity's size.
        /// </summary>
        public virtual Vector3 Size
        {
            get
            {
                return new Vector3( 1.0f, 2.0f, 1.0f );
            }
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="device">The graphics device to create the entity on.</param>
        public Entity( GraphicsDevice device )
        {
            _device = device;
        }

        /// <summary>
        /// Gets the bounding box for this entity.
        /// </summary>
        /// <param name="xmod">The position X modifier.</param>
        /// <param name="ymod">The position Y modifier.</param>
        /// <param name="zmod">The position Z modifier.</param>
        protected virtual BoundingBox GetBounds( float xmod, float ymod, float zmod )
        {
            Vector3 min = new Vector3(
                ( Position.X + xmod ) - ( Size.X / 2.0f ),
                ( Position.Y + ymod ) - ( Size.Y / 2.0f ),
                ( Position.Z + zmod ) - ( Size.Z / 2.0f )
            );
            Vector3 max = new Vector3(
                ( Position.X + xmod ) + ( Size.X / 2.0f ),
                ( Position.Y + ymod ) + ( Size.Y / 2.0f ),
                ( Position.Z + zmod ) + ( Size.Z / 2.0f )
            );
            return new BoundingBox( min, max );
        }

        /// <summary>
        /// Moves the entity.
        /// </summary>
        /// <param name="x">The X amount to move.</param>
        /// <param name="y">The Y amount to move.</param>
        /// <param name="z">The Z amount to move.</param>
        public void Move( float x, float y, float z )
        {
            Move( new Vector3( x, y, z ) );
        }

        /// <summary>
        /// Moves the entity.
        /// </summary>
        /// <param name="amount">The amount to move.</param>
        public void Move( Vector3 amount )
        {
            _position += amount;
        }

        /// <summary>
        /// Moves the entity to the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate to move to.</param>
        /// <param name="y">The Y coordinate to move to.</param>
        /// <param name="z">The Z coordinate to move to.</param>
        public void MoveTo( float x, float y, float z )
        {
            MoveTo( new Vector3( x, y, z ) );
        }

        /// <summary>
        /// Moves the entity to the given coordinates.
        /// </summary>
        /// <param name="pos">The coordinates to move to.</param>
        public void MoveTo( Vector3 pos )
        {
            _position = pos;
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public abstract void Update( GameTime gameTime );

        /// <summary>
        /// Draws the entity.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="effect">The effect to use to draw the entity.</param>
        public abstract void Draw( GameTime gameTime, BaseEffect effect );
    }
}