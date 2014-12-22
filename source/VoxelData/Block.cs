using System;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains block data.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Gets or sets this block's type.
        /// </summary>
        public BlockType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Checks to see if this block's type is an empty (non-interactable) type.
        /// </summary>
        public bool IsEmptyType
        {
            get
            {
                return Type == BlockType.Air
                    || Type == BlockType.Water;
            }
        }

        /// <summary>
        /// Creates a new block.
        /// </summary>
        public Block()
            : this( BlockType.Air )
        {
        }

        /// <summary>
        /// Creates a new block.
        /// </summary>
        /// <param name="type">The block's type.</param>
        public Block( BlockType type )
        {
            Type = type;
        }

        /// <summary>
        /// Gets the textual representation of this block.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + Type.ToString() + "]";
        }
    }
}