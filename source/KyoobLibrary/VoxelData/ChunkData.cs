using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

// TODO : Make chunks 16x??x16 where ?? is the maximum terrain height in the Y direction / 16. Basically, make chunks be entire "pillars"

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains chunk block data.
    /// </summary>
    public sealed class ChunkData
    {
        /// <summary>
        /// The size of the XZ dimension of chunk data.
        /// </summary>
        public const int SizeXZ = 16;

        /// <summary>
        /// The inverse size of the XZ dimension of chunk data.
        /// </summary>
        public const float InverseSizeXZ = 1.0f / SizeXZ;

        /// <summary>
        /// The size of the Y dimension of chunk data.
        /// </summary>
        public const int SizeY = 64;

        /// <summary>
        /// The inverse size of the Y dimension of chunk data.
        /// </summary>
        public const float InverseSizeY = 1.0f / SizeY;


        private Block[,,] _blocks;
        private Vector3 _index;

        /// <summary>
        /// Gets the index of this chunk data.
        /// </summary>
        public Vector3 Index
        {
            get
            {
                return _index;
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
                if ( x < 0 || x >= SizeXZ || y < 0 || y >= SizeY || z < 0 || z >= SizeXZ )
                {
                    throw new ArgumentOutOfRangeException( "Fix this to return the block in the world instance." );
                }
                return _blocks[ x, y, z ];
            }
            set
            {
                if ( x < 0 || x >= SizeXZ || y < 0 || y >= SizeY || z < 0 || z >= SizeXZ )
                {
                    throw new ArgumentOutOfRangeException( "Fix this to return the block in the world instance." );
                }
                _blocks[ x, y, z ] = value;
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
        /// <param name="index">The index of this chunk data.</param>
        public ChunkData( Vector3 index )
        {
            _blocks = new Block[ SizeXZ, SizeY, SizeXZ ];
            _index = index;
        }
    }
}