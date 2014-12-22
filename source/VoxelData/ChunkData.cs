using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains chunk block data.
    /// </summary>
    public sealed class ChunkData
    {
        private Block[,,] _blocks;
        private Vector3 _center;

        /// <summary>
        /// The size of each dimension of the chunk data.
        /// </summary>
        public const int Size = 16;

        /// <summary>
        /// Gets the center of this chunk data.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return _center;
            }
        }

        /// <summary>
        /// Gets the block at the given index.
        /// </summary>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        /// <returns></returns>
        public Block this[ int x, int y, int z ]
        {
            get
            {
                if ( x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size )
                {
                    throw new ArgumentOutOfRangeException( "Fix this to return the block in the world instance." );
                }
                return _blocks[ x, y, z ];
            }
        }

        /// <summary>
        /// Gets the block at the given index.
        /// </summary>
        /// <param name="index">The index as a Vector3.</param>
        /// <returns></returns>
        public Block this[ Vector3 index ]
        {
            get
            {
                return this[ (int)index.X, (int)index.Y, (int)index.Z ];
            }
        }

        /// <summary>
        /// Creates a new chunk data object.
        /// </summary>
        /// <param name="center">The center of the chunk data.</param>
        public ChunkData( Vector3 center )
        {
            _center = center;
            _blocks = new Block[ Size, Size, Size ];
            for ( int x = 0; x < Size; ++x )
            {
                for ( int y = 0; y < Size; ++y )
                {
                    for ( int z = 0; z < Size; ++z )
                    {
                        _blocks[ x, y, z ] = new Block();
                    }
                }
            }
        }
    }
}