using System;

namespace Kyoob.Blocks
{
    /// <summary>
    /// 3D index structure.
    /// </summary>
    public struct Index3D
    {
        /// <summary>
        /// X index.
        /// </summary>
        public int X;

        /// <summary>
        /// Y index.
        /// </summary>
        public int Y;

        /// <summary>
        /// Z index.
        /// </summary>
        public int Z;

        /// <summary>
        /// Creates a new 3D index.
        /// </summary>
        /// <param name="x">The X index.</param>
        /// <param name="y">The X index.</param>
        /// <param name="z">The X index.</param>
        public Index3D( int x, int y, int z )
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}