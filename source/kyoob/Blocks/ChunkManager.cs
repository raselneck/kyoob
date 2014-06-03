using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Kyoob.Game;

// http://msdn.microsoft.com/en-us/library/3dasc8as(v=vs.110).aspx

namespace Kyoob.Blocks
{
    /// <summary>
    /// The class for managing chunks.
    /// </summary>
    public sealed class ChunkManager : IDisposable
    {
        /// <summary>
        /// The delegate type for registering created chunks.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        private delegate void ChunkRegistrationDelegate( Chunk chunk );

        /// <summary>
        /// The delegate type for reporting progress.
        /// </summary>
        /// <param name="progress">The progress.</param>
        private delegate void ReportProgressDelegate( float progress );

        /// <summary>
        /// The objects used by the chunk manager for the thread pool.
        /// </summary>
        private sealed class ChunkCreator
        {
            private World _world;
            private KyoobSettings _settings;
            private ManualResetEvent _doneEvent;
            private ReportProgressDelegate _reportProgress;
            private ChunkRegistrationDelegate _registerChunk;

            /// <summary>
            /// Creates a new chunk creator.
            /// </summary>
            /// <param name="settings">The global settings to use.</param>
            /// <param name="world">The world to place the chunk in.</param>
            /// <param name="doneEvent">The event to call when done.</param>
            public ChunkCreator( KyoobSettings settings, World world, ManualResetEvent doneEvent )
            {
                _world = world;
                _settings = settings;
                _doneEvent = doneEvent;
            }

            /// <summary>
            /// Sets the callback used for reporting progress.
            /// </summary>
            /// <param name="callback">The callback.</param>
            public void SetReportProgressCallback( ReportProgressDelegate callback )
            {
                _reportProgress = callback;
            }

            /// <summary>
            /// Sets the callback used for registering chunks.
            /// </summary>
            /// <param name="callback">The callback.</param>
            public void SetRegisterChunkCallback( ChunkRegistrationDelegate callback )
            {
                _registerChunk = callback;
            }

            /// <summary>
            /// The thread pool callback for this chunk creator.
            /// </summary>
            /// <param name="data">The data to create. (Should be of type HashSet&lt;Index3D&gt;.)</param>
            public void ThreadPoolCallback( object data )
            {
                // make sure the data is what we actually need
                HashSet<Index3D> toCreate = data as HashSet<Index3D>;
                if ( toCreate == null )
                {
                    _doneEvent.Set();
                }

                // now create each index
                foreach ( Index3D idx in toCreate )
                {
                    // get the index and skip it if it's below the world
                    if ( idx.Y < 0 )
                    {
                        continue;
                    }

                    // create the chunk, register it, and report our progress
                    Chunk chunk = new Chunk( _settings, _world, World.IndexToPosition( idx ) );
                    _reportProgress( 1.0f / toCreate.Count );
                    _registerChunk( chunk );
                }
                _doneEvent.Set();
            }
        }

        private volatile int _updateRenderCount;
        private volatile bool _isDisposed;
        private volatile float _chunkCreationProgress;
        private volatile float _currentViewDistance;
        private          Vector3 _currentViewPosition;
        private volatile HashSet<Index3D> _toUnload;
        private volatile Dictionary<Index3D, Chunk> _chunks;

        private World _world;
        private Thread _mainThread;
        private KyoobSettings _settings;

        /// <summary>
        /// Gets or sets the number of chunks that will be created before updating the render list.
        /// </summary>
        public int RenderUpdateCount
        {
            get
            {
                return _updateRenderCount;
            }
            set
            {
                _updateRenderCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the view distance.
        /// </summary>
        public float ViewDistance
        {
            get
            {
                return _currentViewDistance;
            }
            set
            {
                // multiply by sqrt(2) to allow for "full square"
                _currentViewDistance = value * 1.41421356237f;
            }
        }

        /// <summary>
        /// Gets or sets the view position.
        /// </summary>
        public Vector3 ViewPosition
        {
            get
            {
                return _currentViewPosition;
            }
            set
            {
                _currentViewPosition = value;
            }
        }

        /// <summary>
        /// Checks to see if the chunk manager is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _mainThread != null;
            }
        }

        /// <summary>
        /// Gets the progress on the current batch of chunks to be created.
        /// </summary>
        public float ChunkCreationProgress
        {
            get
            {
                return _chunkCreationProgress;
            }
        }

        /// <summary>
        /// Creates a new chunk manager.
        /// </summary>
        /// <param name="settings">The global settings to use.</param>
        /// <param name="world">The world to manage.</param>
        public ChunkManager( KyoobSettings settings, World world )
        {
            _updateRenderCount = 16;
            _world = world;
            _settings = settings;
            _isDisposed = false;
            _currentViewDistance = 0.0f;
            _currentViewPosition = Vector3.Zero;
            // _toUnload = new HashSet<Index3D>(); // will be populated in thread
            // _toCreate = new HashSet<Index3D>(); // will be populated in thread
            _chunks = new Dictionary<Index3D, Chunk>();
        }


        /// <summary>
        /// Registers a chunk.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        private void RegisterChunk( Chunk chunk )
        {
            lock ( _chunks )
            {
                Index3D index = World.PositionToIndex( chunk.Center );
                if ( !_chunks.ContainsKey( index ) )
                {
                    _chunks.Add( index, chunk );
                }
            }
        }

        /// <summary>
        /// Determines whether or not an index is okay to create.
        /// </summary>
        /// <param name="current">The current index.</param>
        /// <param name="desired">The desired index.</param>
        /// <param name="maxDist">The current maximum distance.</param>
        /// <returns></returns>
        private bool IsOkayToCreate( Index3D current, Index3D desired, int maxDist )
        {
            int dx = Math.Abs( current.X - desired.X );
            int dy = Math.Abs( current.Y - desired.Y );
            int dz = Math.Abs( current.Z - desired.Z );
            return ( dx <= maxDist ) && ( dy <= maxDist ) && ( dz <= maxDist );
        }

        /// <summary>
        /// Unloads chunks that need to be unloaded.
        /// </summary>
        /// <param name="index">The current player index.</param>
        /// <param name="maxDist">The maximum distance allowed.</param>
        private void UnloadChunks( Index3D index, int maxDist )
        {
            // put ALL chunks on the chopping block
            List<Index3D> currentIndices;
            lock ( _chunks )
            {
                _toUnload = new HashSet<Index3D>( _chunks.Keys );
                currentIndices = new List<Index3D>( _chunks.Keys );
            }
            foreach ( Index3D idx in currentIndices )
            {
                // check if we can still see the chunk
                if ( IsOkayToCreate( index, idx, maxDist ) )
                {
                    _toUnload.Remove( idx );
                }
            }

            // unload all of the chunks that we need to unload first
            foreach ( Index3D idx in _toUnload )
            {
                _chunks[ idx ].Unload();
                _chunks.Remove( idx );
            }
            _toUnload.Clear();
        }

        /// <summary>
        /// The callback for the chunk management thread.
        /// </summary>
        private void ChunkManagementCallback()
        {
            // create some variables to help with chunk management
            //List<ChunkCreator> creators = new List<ChunkCreator>();
            ManualResetEvent[] doneEvents;
            Index3D index;
            int maxDist;
            int maxHeight = (int)Math.Ceiling(
                _settings.TerrainGenerator.HighestPoint / Chunk.Size
            );

            // create our report progress delegate
            float totalProgress = 0.0f;
            ReportProgressDelegate reportProgress = ( float progress ) =>
            {
                totalProgress += progress;
                _chunkCreationProgress = totalProgress / ( maxHeight + 1 );
            };

            // continue while we're not disposed
            while ( !_isDisposed )
            {
                index = World.PositionToIndex( _currentViewPosition );
                maxDist = (int)( _currentViewDistance / Chunk.Size );
                doneEvents = new ManualResetEvent[ maxHeight + 1 ];

                // create a list of all of the indices to check
                for ( int y = 0; y <= maxHeight; ++y )
                {
                    HashSet<Index3D> layer = new HashSet<Index3D>();

                    // go through each chunk in the XZ layer
                    for ( int x = 0; x < maxDist; ++x )
                    {
                        for ( int z = 0; z < maxDist; ++z )
                        {
                            Index3D temp = new Index3D( index.X + x, y, index.Z + z );
                            if ( IsOkayToCreate( index, temp, maxDist ) &&
                                !_chunks.ContainsKey( temp ) )
                            {
                                layer.Add( temp );
                            }

                            temp = new Index3D( index.X + x, y, index.Z - z );
                            if ( IsOkayToCreate( index, temp, maxDist ) &&
                                !_chunks.ContainsKey( temp ) )
                            {
                                layer.Add( temp );
                            }

                            temp = new Index3D( index.X - x, y, index.Z + z );
                            if ( IsOkayToCreate( index, temp, maxDist ) &&
                                !_chunks.ContainsKey( temp ) )
                            {
                                layer.Add( temp );
                            }

                            temp = new Index3D( index.X - x, y, index.Z - z );
                            if ( IsOkayToCreate( index, temp, maxDist ) &&
                                !_chunks.ContainsKey( temp ) )
                            {
                                layer.Add( temp );
                            }
                        }
                    }

                    // spawn the chunk creator
                    doneEvents[ y ] = new ManualResetEvent( false );
                    ChunkCreator creator = new ChunkCreator( _settings, _world, doneEvents[ y ] );
                    creator.SetRegisterChunkCallback( RegisterChunk );
                    creator.SetReportProgressCallback( reportProgress );
                    ThreadPool.QueueUserWorkItem( creator.ThreadPoolCallback, layer );
                }


                UnloadChunks( index, maxDist );
                

                // now let the thread pool run
                WaitHandle.WaitAll( doneEvents );
                totalProgress = 0.0f;
            }
        }


        /// <summary>
        /// Disposes of this chunk manager.
        /// </summary>
        public void Dispose()
        {
            // set our flag and join the thread
            _isDisposed = true;
            _mainThread.Join( 100 );
            _mainThread = null;

            // dispose of all loaded chunks
            lock ( _chunks )
            {
                foreach ( Chunk chunk in _chunks.Values )
                {
                    chunk.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts the chunk manager.
        /// </summary>
        /// <param name="viewPosition">The starting view position.</param>
        /// <param name="viewDistance">The starting view distance.</param>
        public void Start( Vector3 viewPosition, float viewDistance )
        {
            _currentViewPosition = viewPosition;
            _currentViewDistance = viewDistance;

            _mainThread = new Thread( new ThreadStart( ChunkManagementCallback ) );
            _mainThread.Name = "Kyoob - Chunk Management";
            _mainThread.IsBackground = true;
            _mainThread.Start();
        }

        /// <summary>
        /// Attempts to get the chunk with the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Chunk GetChunk( Index3D index )
        {
            Chunk chunk = null;
            _chunks.TryGetValue( index, out chunk );
            return chunk;
        }

        /// <summary>
        /// Gets the list of chunks to be rendered.
        /// </summary>
        /// <returns></returns>
        public List<Chunk> GetRenderList()
        {
            List<Chunk> toRender;
            lock ( _chunks )
            {
                toRender = new List<Chunk>( _chunks.Values );
            }
            return toRender;
        }
    }
}