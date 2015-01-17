using System;
using System.Collections.Generic;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains terrain density information.
    /// </summary>
    public struct TerrainDensity
    {
        /// <summary>
        /// Gets this density's block type.
        /// </summary>
        public BlockType BlockType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets this block type's minimum density.
        /// </summary>
        public float MinimumDensity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets this block type's maximum density.
        /// </summary>
        public float MaximumDensity
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates new density information.
        /// </summary>
        /// <param name="blockTypeInfo">The block type information.</param>
        public TerrainDensity( BlockTypeInfo blockTypeInfo ) : this()
        {
            BlockType       = blockTypeInfo.BlockType;
            MinimumDensity  = blockTypeInfo.MinimumDensity;
            MaximumDensity  = blockTypeInfo.MaximumDensity;
        }

        /// <summary>
        /// Creates new density information.
        /// </summary>
        /// <param name="blockType">The block type.</param>
        /// <param name="minDensity">The minimum density.</param>
        /// <param name="maxDensity">The maximum density.</param>
        public TerrainDensity( BlockType blockType, float minDensity, float maxDensity ) : this()
        {
            BlockType       = blockType;
            MinimumDensity  = minDensity;
            MaximumDensity  = maxDensity;
        }

        /// <summary>
        /// Finds terrain density information for the given block type.
        /// </summary>
        /// <param name="blockType">The block type.</param>
        /// <returns></returns>
        public static TerrainDensity Find( BlockType blockType )
        {
            return new TerrainDensity( BlockTypeInfo.Find( blockType ) );
        }
    }
}