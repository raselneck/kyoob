using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob.Blocks
{
    /// <summary>
    /// A class containing block data.
    /// </summary>
    public sealed class Block : IBoundable
    {
        private Vector3 _position;
        private BlockType _type;
        private bool _isActive;
        private BoundingBox _bounds;

        /// <summary>
        /// Gets this block's position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Gets or sets this block's type.
        /// </summary>
        public BlockType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this block is active.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
            }
        }

        /// <summary>
        /// Checks to see if this block is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _type == BlockType.Air;
            }
        }

        /// <summary>
        /// Gets the block's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Creates a new block.
        /// </summary>
        /// <param name="position">The block's position.</param>
        /// <param name="type">The block's type.</param>
        public Block( Vector3 position, BlockType type )
        {
            _position = position;
            _type = type;

            // basically this block is active if it's not air
            _isActive = !IsEmpty;

            // set the bounds of this block
            _bounds = new BoundingBox(
                new Vector3(
                    _position.X - Cube.Size / 2.0f,
                    _position.Y - Cube.Size / 2.0f,
                    _position.Z - Cube.Size / 2.0f
                ),
                new Vector3(
                    _position.X + Cube.Size / 2.0f,
                    _position.Y + Cube.Size / 2.0f,
                    _position.Z + Cube.Size / 2.0f
                )
            );
        }

        /// <summary>
        /// Checks to see if this block intersects a ray.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool Intersects( Ray ray )
        {
            return _bounds.Intersects( ray ).HasValue;
        }

        /// <summary>
        /// Gets the intersection distance.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public float? GetInstersectionDistance( Ray ray )
        {
            return _bounds.Intersects( ray );
        }
    }
}