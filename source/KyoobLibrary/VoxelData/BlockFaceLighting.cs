using System;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains lighting information for a block face.
    /// </summary>
    internal struct BlockFaceLighting
    {
        /// <summary>
        /// The lighting value for the lower left face corner.
        /// </summary>
        public float LowerLeft;

        /// <summary>
        /// The lighting value for the lower right face corner.
        /// </summary>
        public float LowerRight;

        /// <summary>
        /// The lighting value for the upper left face corner.
        /// </summary>
        public float UpperLeft;

        /// <summary>
        /// The lighting value for the upper right face corner.
        /// </summary>
        public float UpperRight;

        /// <summary>
        /// Creates new block face lighting information.
        /// </summary>
        /// <param name="all">The initial lighting value for all face corners.</param>
        public BlockFaceLighting( float all )
        {
            LowerLeft = all;
            LowerRight = all;
            UpperLeft = all;
            UpperRight = all;
        }
    }
}