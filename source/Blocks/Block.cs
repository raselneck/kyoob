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
    public sealed class Block
    {
        private Vector3 _position;
        private BlockType _type;
        private bool _isActive;

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
        /// Gets this block's type.
        /// </summary>
        public BlockType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Checks to see if this block is active or not.
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
        /// Creates a new block.
        /// </summary>
        /// <param name="position">The block's position.</param>
        /// <param name="type">The block's type.</param>
        public Block( Vector3 position, BlockType type )
        {
            _position = position;
            _type = type;
            _isActive = !IsEmpty;
        }
    }
}