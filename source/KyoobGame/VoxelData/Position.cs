using System;
using Microsoft.Xna.Framework;

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
        /// <param name="center">The center of the chunk.</param>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        /// <param name="world">The world position to populate.</param>
        public static void LocalToWorld( Vector3 center, int x, int y, int z, out Vector3 world )
        {
            world.X = center.X + x - ChunkData.Size * 0.5f;
            world.Y = center.Y + y - ChunkData.Size * 0.5f;
            world.Z = center.Z + z - ChunkData.Size * 0.5f;
        }

        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        /// <returns></returns>
        public static Vector3 LocalToWorld( Vector3 center, int x, int y, int z )
        {
            Vector3 world;
            LocalToWorld( center, x, y, z, out world );
            return world;
        }

        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="index">The index as a Vector3.</param>
        /// <param name="world">The world position to populate.</param>
        public static void LocalToWorld( Vector3 center, Vector3 index, out Vector3 world )
        {
            world.X = center.X + index.X - ChunkData.Size * 0.5f;
            world.Y = center.Y + index.Y - ChunkData.Size * 0.5f;
            world.Z = center.Z + index.Z - ChunkData.Size * 0.5f;
        }

        /// <summary>
        /// Converts a local position to a world position.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="index">The index as a Vector3.</param>
        /// <returns></returns>
        public static Vector3 LocalToWorld( Vector3 center, Vector3 index )
        {
            Vector3 world;
            LocalToWorld( center, index, out world );
            return world;
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="chunkCenter">The center of the chunk to convert to.</param>
        /// <param name="local">The local position to populate.</param>
        public static void WorldToLocal( Vector3 position, Vector3 chunkCenter, out Vector3 local )
        {
            local.X = position.X - chunkCenter.X + ChunkData.Size * 0.5f;
            local.Y = position.Y - chunkCenter.Y + ChunkData.Size * 0.5f;
            local.Z = position.Z - chunkCenter.Z + ChunkData.Size * 0.5f;
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="chunkCenter">The center of the chunk to convert to.</param>
        /// <returns></returns>
        public static Vector3 WorldToLocal( Vector3 position, Vector3 chunkCenter )
        {
            Vector3 local;
            WorldToLocal( position, chunkCenter, out local );
            return local;
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="index">The calculated index.</param>
        /// <param name="center">The calculated center of the chunk.</param>
        /// <param name="local">The local position to populate.</param>
        public static void WorldToLocal( Vector3 position, out Vector3 index, out Vector3 center, out Vector3 local )
        {
            // set index
            index.X = (int)Math.Round( position.X / ChunkData.Size );
            index.Y = (int)Math.Round( position.Y / ChunkData.Size );
            index.Z = (int)Math.Round( position.Z / ChunkData.Size );

            // set center
            center = index * ChunkData.Size;

            // return the final conversion
            WorldToLocal( position, center, out local );
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <param name="index">The calculated index.</param>
        /// <param name="center">The calculated center of the chunk.</param>
        /// <returns></returns>
        public static Vector3 WorldToLocal( Vector3 position, out Vector3 index, out Vector3 center )
        {
            Vector3 local;
            WorldToLocal( position, out index, out center, out local );
            return local;
        }
    }
}