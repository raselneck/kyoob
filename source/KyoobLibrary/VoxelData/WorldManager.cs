using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Kyoob.Entities;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains a way to manage a world.
    /// </summary>
    public sealed class WorldManager : IDisposable
    {
        private volatile Dictionary<Vector3, Chunk> _chunks;
        private volatile World _world;
        private Thread _threadLoad;

        /// <summary>
        /// Checks to see if this world manager is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Checks to see if this world manager should be running.
        /// </summary>
        private bool ShouldBeRunning
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new world manager.
        /// </summary>
        /// <param name="world">The world to manage.</param>
        /// <param name="initialPosition">The initial position to begin generation around.</param>
        public WorldManager( World world, Vector3 initialPosition )
        {
            _world = world;
            _chunks = new Dictionary<Vector3, Chunk>();

            // create some initial chunks (should take less than a second on most systems)
            const int initialSquareSize = 2;
            Vector3 center = new Vector3();
            int minX = (int)( initialPosition.X * ChunkData.InverseSizeXZ - initialSquareSize ),
                maxX = (int)( initialPosition.X * ChunkData.InverseSizeXZ + initialSquareSize ),
                minZ = (int)( initialPosition.Z * ChunkData.InverseSizeXZ - initialSquareSize ),
                maxZ = (int)( initialPosition.Z * ChunkData.InverseSizeXZ + initialSquareSize );
            for ( int x = minX; x <= maxX; ++x )
            {
                for ( int z = minZ; z <= maxZ; ++z )
                {
                    center.X = x * ChunkData.SizeXZ;
                    center.Z = z * ChunkData.SizeXZ;
                    LoadChunk( ref center );
                }
            }

            // begin the threads
            ShouldBeRunning = true;
            _threadLoad = new Thread( new ThreadStart( MaintainChunks ) );
            _threadLoad.Start();
        }

        /// <summary>
        /// Ensures cleanup for this world manager.
        /// </summary>
        ~WorldManager()
        {
            Dispose( false );
        }

        /// <summary>
        /// Gets the chunk that contains the given world position.
        /// </summary>
        /// <param name="world">The position in the world.</param>
        /// <returns></returns>
        public Chunk GetChunkFromPosition( Vector3 world )
        {
            Vector3 center, index;
            Position.WorldToLocal( world, out index, out center );
            return GetChunkFromCenter( center );
        }

        /// <summary>
        /// Gets the chunk with the given center position.
        /// </summary>
        /// <param name="center">The center of the desired chunk.</param>
        /// <returns></returns>
        public Chunk GetChunkFromCenter( Vector3 center )
        {
            // TODO : Rename this to be GetChunk(Vector3 index)

            Chunk chunk = null;
            lock ( _chunks )
            {
                if ( _chunks.ContainsKey( center ) )
                {
                    chunk = _chunks[ center ];
                }
            }
            return chunk;
        }

        /// <summary>
        /// Draws all of the chunks, assuming that an effect is currently active.
        /// </summary>
        /// <param name="camera">The camera to use for visibility testing.</param>
        public void DrawChunks( Camera camera )
        {
            lock ( _chunks )
            {
                // render each chunk that the camera can see
                foreach ( var chunk in _chunks.Values )
                {
                    if ( Game.Instance.Settings.CullFrustum  )
                    {
                        if ( camera.CanSee( chunk.Bounds ) )
                        {
                            chunk.Draw();
                        }
                    }
                    else
                    {
                        chunk.Draw();
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of this world manager.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of this world manager.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose( bool disposing )
        {
            ShouldBeRunning = false;
            _threadLoad.Join();
            lock ( _chunks ) // just in case
            {
                foreach ( var pair in _chunks )
                {
                    pair.Value.Dispose();
                }
                _chunks.Clear();
            }
            IsDisposed = true;
        }

        /// <summary>
        /// Maintains the chunks.
        /// </summary>
        private void MaintainChunks()
        {
            var player = Player.Instance;

            // prepare for loop
            Vector3 chunkIndex          = Vector3.Zero,
                    chunkLocalIndex     = Vector3.Zero,
                    currentPosition     = Vector3.Zero,
                    toCheck             = Vector3.Zero,
                    currentPositionNoY  = Vector3.Zero,
                    centroid            = Vector3.Zero;
            float currentViewDistance;
            int maxIndexDist = 0, currentIndexDist = 0;
            int x, z;
            var toUnload = new List<Vector3>( ChunkData.SizeXZ * ChunkData.SizeXZ );

            // enter into loop
            while ( ShouldBeRunning )
            {
                // get current chunk center and index and calculate the max distance in the X and Z direction
                currentPosition = player.Position;
                currentPositionNoY.X = currentPosition.X;
                currentPositionNoY.Z = currentPosition.Z;
                currentViewDistance = Game.Instance.Settings.ViewDistance;
                Position.WorldToLocal( currentPosition, out chunkLocalIndex, out chunkIndex );

                // update index distance
                var playerDistFromCentroid = Vector3.Distance( centroid, currentPositionNoY );
                var maxDistFromCentroid = currentViewDistance * 0.25f;
                maxIndexDist = (int)Math.Ceiling( currentViewDistance / ChunkData.SizeXZ );
                if ( ++currentIndexDist > maxIndexDist || playerDistFromCentroid >= maxDistFromCentroid )
                {
                    centroid.X = currentPositionNoY.X;
                    centroid.Z = currentPositionNoY.Z;
                    currentIndexDist = 0;
                }



                // put all chunks up on the chopping block
                toUnload.Clear();
                lock ( _chunks )
                {
                    toUnload.AddRange( _chunks.Keys );
                }

                // now go through all of those chunks and see which ones we need to unload
                for ( int i = 0; i < toUnload.Count && ShouldBeRunning; ++i )
                {
                    // if it's too far away, unload it
                    toCheck = toUnload[ i ];
                    if ( Vector3.Distance( currentPositionNoY, toCheck ) > currentViewDistance )
                    {
                        UnloadChunk( ref toCheck );
                    }
                }



                // go through and create chunks that need to be loaded
                for ( x = 0; x <= currentIndexDist && ShouldBeRunning; ++x )
                {
                    for ( z = 0; z <= currentIndexDist && ShouldBeRunning; ++z )
                    {
                        // check +x +z
                        toCheck.X = chunkIndex.X + ( x * ChunkData.SizeXZ );
                        toCheck.Z = chunkIndex.Z + ( z * ChunkData.SizeXZ );
                        if ( Vector3.Distance( currentPositionNoY, toCheck ) <= currentViewDistance )
                        {
                            LoadChunk( ref toCheck );
                        }

                        // check +x -z
                        toCheck.X = chunkIndex.X + ( x * ChunkData.SizeXZ );
                        toCheck.Z = chunkIndex.Z - ( z * ChunkData.SizeXZ );
                        if ( Vector3.Distance( currentPositionNoY, toCheck ) <= currentViewDistance )
                        {
                            LoadChunk( ref toCheck );
                        }

                        // check -x +z
                        toCheck.X = chunkIndex.X - ( x * ChunkData.SizeXZ );
                        toCheck.Z = chunkIndex.Z + ( z * ChunkData.SizeXZ );
                        if ( Vector3.Distance( currentPositionNoY, toCheck ) <= currentViewDistance )
                        {
                            LoadChunk( ref toCheck );
                        }

                        // check -x -z
                        toCheck.X = chunkIndex.X - ( x * ChunkData.SizeXZ );
                        toCheck.Z = chunkIndex.Z - ( z * ChunkData.SizeXZ );
                        if ( Vector3.Distance( currentPositionNoY, toCheck ) <= currentViewDistance )
                        {
                            LoadChunk( ref toCheck );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads (creates) the chunk at the given position.
        /// </summary>
        /// <param name="index">The index of the chunk.</param>
        private void LoadChunk( ref Vector3 index )
        {
            // check if the chunk already exists
            bool contains = false;
            lock ( _chunks )
            {
                contains = _chunks.ContainsKey( index );
            }

            // if the chunk doesn't exist, create it and add it
            if ( !contains )
            {
                var chunk = new Chunk( _world, index );
                lock ( _chunks )
                {
                    _chunks[ index ] = chunk;
                }
            }
        }

        /// <summary>
        /// Unloads (disposes and removes) the chunk at the given position.
        /// </summary>
        /// <param name="pos">The position.</param>
        private void UnloadChunk( ref Vector3 pos )
        {
            Chunk chunk = null;
            lock ( _chunks )
            {
                if ( _chunks.ContainsKey( pos ) )
                {
                    chunk = _chunks[ pos ];
                    _chunks.Remove( pos );
                }
            }
            chunk.Dispose();
        }
    }
}