using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

// NOTE : After switching chunks to their current "pillar" form, I had to rewrite these functions.
//        As of right now they work, but I am not entirely sure if they are 100% accurate.
// TODO : Ensure these functions work properly.

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Internal utility class for position conversion.
    /// </summary>
    internal static class Position
    {
        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="chunkIndex">The chunk's index.</param>
        /// <param name="x">The local X index.</param>
        /// <param name="y">The local Y index.</param>
        /// <param name="z">The local Z index.</param>
        /// <param name="world">The world position to populate.</param>
        public static void LocalToWorld( Vector3 chunkIndex, int x, int y, int z, out Vector3 world )
        {
#if DEBUG
            if ( chunkIndex.Y != 0 )
            {
                Debug.WriteLine( "Chunk centers must have a Y value of 0." );
            }
#endif
            world.X = chunkIndex.X + x - ChunkData.SizeXZ * 0.5f;
            world.Z = chunkIndex.Z + z - ChunkData.SizeXZ * 0.5f;
            world.Y = y;
        }

        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="chunkIndex">The chunk's index.</param>
        /// <param name="x">The local X index.</param>
        /// <param name="y">The local Y index.</param>
        /// <param name="z">The local Z index.</param>
        /// <returns></returns>
        public static Vector3 LocalToWorld( Vector3 chunkIndex, int x, int y, int z )
        {
            Vector3 world;
            LocalToWorld( chunkIndex, x, y, z, out world );
            return world;
        }

        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="chunkIndex">The chunk's index.</param>
        /// <param name="localIndex">The local chunk index.</param>
        /// <param name="world">The world position to populate.</param>
        public static void LocalToWorld( Vector3 chunkIndex, Vector3 localIndex, out Vector3 world )
        {
#if DEBUG
            if ( chunkIndex.Y != 0 )
            {
                Debug.WriteLine( "Chunk centers must have a Y value of 0." );
            }
#endif
            world.X = chunkIndex.X + localIndex.X - ChunkData.SizeXZ * 0.5f;
            world.Z = chunkIndex.Z + localIndex.Z - ChunkData.SizeXZ * 0.5f;
            world.Y = localIndex.Y;
        }

        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="chunkIndex">The chunk's index.</param>
        /// <param name="localIndex">The local chunk index.</param>
        /// <returns></returns>
        public static Vector3 LocalToWorld( Vector3 chunkIndex, Vector3 localIndex )
        {
            Vector3 world;
            LocalToWorld( chunkIndex, localIndex, out world );
            return world;
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="chunkIndex">The chunk's index.</param>
        /// <param name="localIndex">The local chunk index.</param>
        public static void WorldToLocal( Vector3 position, Vector3 chunkIndex, out Vector3 localIndex )
        {
            localIndex.X = position.X - chunkIndex.X + ChunkData.SizeXZ * 0.5f;
            localIndex.Z = position.Z - chunkIndex.Z + ChunkData.SizeXZ * 0.5f;
            localIndex.Y = (int)position.Y; // TODO : This can cause a crash
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="chunkIndex">The chunk's index.</param>
        /// <returns></returns>
        public static Vector3 WorldToLocal( Vector3 position, Vector3 chunkIndex )
        {
            Vector3 local;
            WorldToLocal( position, chunkIndex, out local );
            return local;
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="localIndex">The calculated local chunk index.</param>
        /// <param name="chunkIndex">The calculated center of the chunk.</param>
        /// <param name="local">The local position to populate.</param>
        public static void WorldToLocal( Vector3 position, out Vector3 localIndex, out Vector3 chunkIndex, out Vector3 local )
        {
            // set index
            localIndex.X = (int)Math.Round( position.X * ChunkData.InverseSizeXZ );
            localIndex.Z = (int)Math.Round( position.Z * ChunkData.InverseSizeXZ );
            localIndex.Y = (int)position.Y;

            // set center
            chunkIndex = localIndex * ChunkData.SizeXZ;
            chunkIndex.Y = 0;

            // return the final conversion
            WorldToLocal( position, chunkIndex, out local );
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="localIndex">The calculated local chunk index.</param>
        /// <param name="chunkIndex">The calculated chunk index.</param>
        /// <returns></returns>
        public static Vector3 WorldToLocal( Vector3 position, out Vector3 localIndex, out Vector3 chunkIndex )
        {
            Vector3 local;
            WorldToLocal( position, out localIndex, out chunkIndex, out local );
            return local;
        }
    }
}