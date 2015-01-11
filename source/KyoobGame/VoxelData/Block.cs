using System;
using Microsoft.Xna.Framework;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains block data.
    /// </summary>
    public struct Block
    {
        /// <summary>
        /// The minimum lighting level allowed for each block.
        /// </summary>
        public const float MinimumLighting = 0.15f;

        /// <summary>
        /// The maximum lighting level allowed for each block.
        /// </summary>
        public const float MaximumLighting = 1.00f;


        private BlockType _type;
        private float _lighting;
        private Chunk _chunk;


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
        /// Gets the type information for this block's type.
        /// </summary>
        public BlockTypeInfo TypeInfo
        {
            get
            {
                return BlockTypeInfo.GetInfoForType( _type );
            }
        }

        /// <summary>
        /// Gets or sets the light level of this block.
        /// </summary>
        public float Lighting
        {
            get
            {
                return _lighting;
            }
            set
            {
                _lighting = MathHelper.Clamp( value, MinimumLighting, MaximumLighting );
                // TODO : Update chunk
            }
        }

        /// <summary>
        /// Gets the chunk this block is in.
        /// </summary>
        public Chunk Chunk
        {
            get
            {
                return _chunk;
            }
        }


        /// <summary>
        /// Creates a new block.
        /// </summary>
        /// <param name="type">The block's type.</param>
        public Block( BlockType type, Chunk chunk )
        {
            _type = type;
            _chunk = chunk;
            _lighting = MinimumLighting;
        }


        /// <summary>
        /// Gets the textual representation of this block.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format( "{{Type={0}, Lighting={1}}}", Type, Lighting );
        }
    }
}