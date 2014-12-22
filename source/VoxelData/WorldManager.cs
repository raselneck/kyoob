using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly int _maxChunksY;
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
        /// <param name="initialPosition">The initial position to begin generation around.</param>
        public WorldManager( Vector3 initialPosition )
        {
            ShouldBeRunning = false;

            // create our data structures
            _chunks = new Dictionary<Vector3, Chunk>();

            // calculate max number of chunks in the Y direction
            _maxChunksY = 1 + (int)Math.Ceiling( TerrainGenerator.Instance.HighestPoint / (float)ChunkData.Size );

            // create some initial chunks (should take less than a second on most systems)
            const int initialSquareSize = 2;
            Vector3 center = new Vector3();
            int minX = (int)( initialPosition.X - initialSquareSize ),
                maxX = (int)( initialPosition.X + initialSquareSize ),
                minZ = (int)( initialPosition.Z - initialSquareSize ),
                maxZ = (int)( initialPosition.Z + initialSquareSize );
            for ( int x = minX; x <= maxX; ++x )
            {
                for ( int z = minZ; z <= maxZ; ++z )
                {
                    for ( int y = 0; y < _maxChunksY; ++y )
                    {
                        center.X = x * ChunkData.Size;
                        center.Y = y * ChunkData.Size;
                        center.Z = z * ChunkData.Size;
                        LoadChunk( ref center );
                    }
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
        public void DrawChunks()
        {
            lock ( _chunks )
            {
                foreach ( var chunk in _chunks.Values )
                {
                    chunk.Draw();
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
        /// Begins chunk maintenance.
        /// </summary>
        /// <param name="sender">The X direction to maintain in.</param>
        private void MaintainChunks()
        {
            // TODO : Create some kind of centroid to be used at the center of chunk generation instead of using the player's
            // position each frame. Then, incrementally create chunks-- instead of going from 0 to currentIndexDist in the
            // X and Z direction, go 0..1 then 0..2 then 0..3, etc. until currentIndexDist. If the user ventures too far
            // from the centroid, then reset the max iteration for the X and Z generation and update the centroid. If the
            // iteration variable reaches currentIndexDist, then all chunks that need to be generated have been generated so
            // none need to be unloaded/re-loaded until the user is too far from the centroid.

            var player = Player.Instance;

            // prepare for loop
            Vector3 chunkCenter, chunkIndex, currentPosition, currentForward, toCheck;
            Vector3 chunkCenterNoY = Vector3.Zero, currentPositionNoY = Vector3.Zero;
            float currentViewDistance;
            int currentIndexDist = 0;
            int x, y, z;
            var toUnload = new List<Vector3>( ChunkData.Size * ChunkData.Size * _maxChunksY );

            // enter into loop
            while ( ShouldBeRunning )
            {
                // get current chunk center and index and calculate the max distance in the X and Z direction
                currentForward = player.Camera.Forward;
                currentPosition = player.Position;
                currentPositionNoY.X = currentPosition.X;
                currentPositionNoY.Z = currentPosition.Z;
                currentViewDistance = Settings.Instance.ViewDistance;
                currentIndexDist = (int)Math.Ceiling( currentViewDistance / ChunkData.Size );
                Position.WorldToLocal( currentPosition, out chunkIndex, out chunkCenter );


                // go through and create chunks that need to be loaded
                for ( x = 0; x <= currentIndexDist && ShouldBeRunning; ++x )
                {
                    for ( z = 0; z <= currentIndexDist && ShouldBeRunning; ++z )
                    {
                        for ( y = 0; y < _maxChunksY && ShouldBeRunning; ++y )
                        {
                            chunkCenter.Y = y * ChunkData.Size;

                            // check +x +z
                            chunkCenter.X = chunkCenterNoY.X = ( chunkIndex.X + x ) * ChunkData.Size;
                            chunkCenter.Z = chunkCenterNoY.Z = ( chunkIndex.Z + z ) * ChunkData.Size;
                            if ( Vector3.Distance( currentPositionNoY, chunkCenterNoY ) <= currentViewDistance )
                            {
                                LoadChunk( ref chunkCenter );
                            }

                            // check +x -z
                            chunkCenter.X = chunkCenterNoY.X = ( chunkIndex.X + x ) * ChunkData.Size;
                            chunkCenter.Z = chunkCenterNoY.Z = ( chunkIndex.Z - z ) * ChunkData.Size;
                            if ( Vector3.Distance( currentPositionNoY, chunkCenterNoY ) <= currentViewDistance )
                            {
                                LoadChunk( ref chunkCenter );
                            }

                            // check -x +z
                            chunkCenter.X = chunkCenterNoY.X = ( chunkIndex.X - x ) * ChunkData.Size;
                            chunkCenter.Z = chunkCenterNoY.Z = ( chunkIndex.Z + z ) * ChunkData.Size;
                            if ( Vector3.Distance( currentPositionNoY, chunkCenterNoY ) <= currentViewDistance )
                            {
                                LoadChunk( ref chunkCenter );
                            }

                            // check -x -z
                            chunkCenter.X = chunkCenterNoY.X = ( chunkIndex.X - x ) * ChunkData.Size;
                            chunkCenter.Z = chunkCenterNoY.Z = ( chunkIndex.Z - z ) * ChunkData.Size;
                            if ( Vector3.Distance( currentPositionNoY, chunkCenterNoY ) <= currentViewDistance )
                            {
                                LoadChunk( ref chunkCenter );
                            }
                        }
                    }
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
                    toCheck.Y = 0;
                    if ( Vector3.Distance( currentPositionNoY, toCheck ) > currentViewDistance )
                    {
                        UnloadChunk( ref toCheck );
                    }
                }
            }
        }

        /// <summary>
        /// Loads (creates) the chunk at the given position.
        /// </summary>
        /// <param name="pos"></param>
        private void LoadChunk( ref Vector3 pos )
        {
            var chunk = new Chunk( pos, true );
            lock ( _chunks )
            {
                _chunks[ pos ] = chunk;
            }

            // With the following method, I believe that sometimes when a chunk is added
            // it doesn't have its terrain generated for some reason
            /*
            bool added = false;
            var chunk = new Chunk( pos );
            
            lock ( _chunks )
            {
                if ( !_chunks.ContainsKey( pos ) )
                {
                    _chunks.Add( pos, chunk );
                    added = true;
                }
            }
            
            if ( added )
            {
                chunk.PopulateTerrain();
            }
            */
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
            if ( chunk != null )
            {
                chunk.Dispose();
            }
        }
    }
}