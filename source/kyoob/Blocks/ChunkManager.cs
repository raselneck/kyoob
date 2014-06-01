using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Kyoob.Game;

namespace Kyoob.Blocks
{
    /// <summary>
    /// The class for managing chunks.
    /// </summary>
    public sealed class ChunkManager : IDisposable
    {
        private volatile int _updateRenderCount;
        private volatile bool _isDisposed;
        private volatile float _currentViewDistance;
        private          Vector3 _currentViewPosition;
        private volatile List<Chunk> _toRender;
        private volatile HashSet<Index3D> _toUnload;
        private volatile HashSet<Index3D> _toCreate;
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
            _toRender = new List<Chunk>( 1024 );
            // _toUnload = new HashSet<Index3D>(); // will be populated in thread
            // _toCreate = new HashSet<Index3D>(); // will be populated in thread
            _chunks = new Dictionary<Index3D, Chunk>();
        }


        /// <summary>
        /// Gets the distance between two 3D indices.
        /// </summary>
        /// <param name="a">The first index.</param>
        /// <param name="b">The second index.</param>
        /// <returns></returns>
        private float GetDistanceBetween( Index3D a, Index3D b )
        {
            Vector3 av = World.IndexToPosition( a );
            Vector3 bv = World.IndexToPosition( b );
            return Vector3.Distance( av, bv );
        }

        /// <summary>
        /// Creates a chunk with the given index.
        /// </summary>
        /// <param name="index">The 3D index.</param>
        private void CreateChunk( Index3D index )
        {
            // if we're disposed, then there's no sense in creating a chunk
            if ( _isDisposed )
            {
                return;
            }

            // if the y is out of bounds, no need to create an empty chunk
            if ( index.Y < 0 )
            {
                return;
            }

            // make sure we don't already have that chunk created
            if ( !_chunks.ContainsKey( index ) )
            {
                // create the chunk and store it
                Chunk chunk = new Chunk( _settings, _world, World.IndexToPosition( index ) );
                _chunks.Add( index, chunk );
            }
        }

        /// <summary>
        /// Updates the render list.
        /// </summary>
        private void UpdateRenderList()
        {
            lock ( _toRender )
            {
                _toRender.Clear();
                _toRender.AddRange( _chunks.Values );
            }
        }

        /// <summary>
        /// The callback for the chunk management thread.
        /// </summary>
        private void ChunkManagementCallback()
        {
            // create some variables to help with chunk management
            HashSet<Index3D> indices = new HashSet<Index3D>();
            Index3D index;
            int maxDist;
            int maxHeight = (int)Math.Ceiling(
                _settings.TerrainGenerator.HighestPoint / Chunk.Size
            );
            int y = maxHeight;

            // continue while we're not disposed
            while ( !_isDisposed )
            {
                index = World.PositionToIndex( _currentViewPosition );
                maxDist = (int)( _currentViewDistance / Chunk.Size );

                // create a list of all of the indices to check
                indices.Clear();
                for ( int x = 0; x < maxDist; ++x )
                {
                    for ( int z = 0; z < maxDist; ++z )
                    {
                        indices.Add( new Index3D( index.X + x, y, index.Z + z ) );
                        indices.Add( new Index3D( index.X + x, y, index.Z - z ) );
                        indices.Add( new Index3D( index.X - x, y, index.Z + z ) );
                        indices.Add( new Index3D( index.X - x, y, index.Z - z ) );
                    }
                }
                // update distance creators
                if ( --y < 0 )
                {
                    y = maxHeight;
                }

                // put ALL chunks on the chopping block
                _toUnload = new HashSet<Index3D>( _chunks.Keys );
                foreach ( Index3D idx in _chunks.Keys )
                {
                    // check if we can still see the chunk
                    Vector3 pos = World.IndexToPosition( idx );
                    if ( Vector3.Distance( _currentViewPosition, pos ) <= _currentViewDistance )
                    {
                        _toUnload.Remove( idx );
                    }
                    else
                    {
                        // if we can't see it, we need to make sure it's not slated to be created
                        indices.Remove( idx );
                    }
                }


                // whatever's left in indices is what needs to be created
                _toCreate = new HashSet<Index3D>( indices );


                ChunkUnloadingCallback();
                ChunkCreationCallback();


                // update the render queue
                UpdateRenderList();
            }
        }

        /// <summary>
        /// Callback for chunk creation.
        /// </summary>
        private void ChunkCreationCallback()
        {
            Index3D current = World.PositionToIndex( _currentViewPosition );
            int count = 0;

            foreach ( Index3D idx in _toCreate )
            {
                // make sure we're not disposed and we can actually view the chunk
                if ( _isDisposed )
                {
                    break;
                }

                // create the chunk
                CreateChunk( idx );

                // check if we need to update the render list
                if ( ++count % _updateRenderCount == 0 )
                {
                    UpdateRenderList();
                }
            }
            _toCreate.Clear();
        }

        /// <summary>
        /// Callback for chunk unloading.
        /// </summary>
        private void ChunkUnloadingCallback()
        {
            foreach ( Index3D idx in _toUnload )
            {
                _chunks[ idx ].Unload();
                _chunks.Remove( idx );
            }
            _toUnload.Clear();
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
            List<Chunk> copy;
            lock ( _toRender )
            {
                copy = new List<Chunk>( _toRender );
            }
            return copy;
        }
    }
}