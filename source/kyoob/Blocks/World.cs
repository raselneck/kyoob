using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Kyoob.Debug;
using Kyoob.Game;
using Kyoob.Graphics;
using Kyoob.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#pragma warning disable 1587 // disable "invalid XML comment placement"

#warning TODO : When chunks are unloaded, their data should be saved to a file somehow so that block data can be retrieved in case a chunk was modified.
#warning TODO : Create dedicated chunk manager.
#warning TODO : Split loading/unloading into two separate threads.
#warning TODO : Create some kind of utilities class to help remove circular dependencies. (I.e. World<->TerrainGenerator for LocalToWorld)

namespace Kyoob.Blocks
{
    /// <summary>
    /// Creates a new world.
    /// </summary>
    public class World : IDisposable
    {
        /// <summary>
        /// The magic number for worlds. (FourCC = 'WRLD')
        /// </summary>
        private const int MagicNumber = 0x444C5257;

        private EffectRenderer _renderer;
        private SpriteSheet _spriteSheet;
        private Dictionary<Index3D, Chunk> _chunks;
        private List<Chunk> _renderQueue;
        private HashSet<Index3D> _indices;
        private TerrainGenerator _terrain;
        private List<Index3D> _createList;
        private List<Index3D> _unloadList;
        private Vector3 _currentViewPosition;
        private float _currentViewDistance;
        private volatile bool _isDisposed;

        /// <summary>
        /// Gets the graphics device this world is on.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _renderer.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets the world's sprite sheet.
        /// </summary>
        public SpriteSheet SpriteSheet
        {
            get
            {
                return _spriteSheet;
            }
        }

        /// <summary>
        /// Gets this world's terrain generator.
        /// </summary>
        public TerrainGenerator TerrainGenerator
        {
            get
            {
                return _terrain;
            }
        }

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="settings">The global settings to use.</param>
        public World( KyoobSettings settings )
        {
            CommonInitialization( settings );
        }

        /// <summary>
        /// Creates a new world by loading it from a stream.
        /// </summary>
        /// <param name="bin">The binary reader to use when reading the world.</param>
        /// <param name="settings">The settings to use.</param>
        private World( BinaryReader bin, KyoobSettings settings )
        {
            CommonInitialization( settings );

            // load the seed (and terrain) and chunks
            _terrain.Seed = bin.ReadInt32();
            int count = bin.ReadInt32();
            for ( int i = 0; i < count; ++i )
            {
                // read the index, then the chunk, then record both
                Index3D index = new Index3D( bin.ReadInt32(), bin.ReadInt32(), bin.ReadInt32() );
                Chunk chunk = Chunk.ReadFrom( bin.BaseStream, this );
                if ( chunk == null )
                {
                    break;
                }
                _chunks.Add( index, chunk );
            }
        }

        /// <summary>
        /// Performs common initialization logic for the world.
        /// </summary>
        /// <param name="settings">The settings to use.</param>
        private void CommonInitialization( KyoobSettings settings )
        {
            // set variables
            _renderer = settings.EffectRenderer;
            _spriteSheet = settings.SpriteSheet;
            _chunks = new Dictionary<Index3D, Chunk>();
            _renderQueue = new List<Chunk>( 2048 );
            _indices = new HashSet<Index3D>();
            _terrain = settings.TerrainGenerator;
            _terrain.World = this;
            _isDisposed = false;

            // create our management lists
            _createList = new List<Index3D>( 4096 );
            _unloadList = new List<Index3D>( 4096 );

            SetTerminalCommands();
        }

        /// <summary>
        /// Sets the world commands in the terminal.
        /// </summary>
        private void SetTerminalCommands()
        {
            // none yet
        }

        /// <summary>
        /// Creates a chunk with the given indices.
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

            /**
             * We're going to ASSUME that the calling thread already has an exclusive lock
             * on the chunk collection.
             */

            // make sure we don't already have that chunk created
            if ( !_chunks.ContainsKey( index ) )
            {
                // create the chunk and store it
                Chunk chunk = new Chunk( this, IndexToPosition( index ) );
                _chunks.Add( index, chunk );
                _indices.Add( index );
            }
        }



        private Thread _chunkManagementThread;
        private Thread _chunkCreationThread;
        private Thread _chunkUnloadingThread;

        /// <summary>
        /// gets the distance between two 3D indices.
        /// </summary>
        /// <param name="a">The first index.</param>
        /// <param name="b">The second index.</param>
        /// <returns></returns>
        private float GetDistanceBetween( Index3D a, Index3D b )
        {
            Vector3 av = IndexToPosition( a );
            Vector3 bv = IndexToPosition( b );
            return Vector3.Distance( av, bv );
        }

        /// <summary>
        /// The callback for the chunk management thread.
        /// </summary>
        private void ChunkManagementCallback()
        {
            HashSet<Index3D> indices = new HashSet<Index3D>();
            Index3D index;
            int maxDist;
            float viewDistance;
            int maxHeight = (int)Math.Ceiling( _terrain.HighestPoint / Chunk.Size );
            float Root2 = (float)Math.Sqrt( 2 );

            while ( !_isDisposed )
            {
                index = PositionToIndex( _currentViewPosition );
                viewDistance = _currentViewDistance * Root2;
                maxDist = (int)( viewDistance / Chunk.Size );
                
                // create a list of all of the indices to check
                indices.Clear();
                for ( int y = maxHeight; y >= 0; --y )
                {
                    for ( int x = 0; x < maxDist; ++x )
                    {
                        for ( int z = 0; z < maxDist; ++z )
                        {
                            // check [+x,y,+z]
                            Index3D temp = new Index3D( index.X + x, y, index.Z + z );
                            if ( GetDistanceBetween( index, temp ) <= viewDistance )
                            {
                                indices.Add( temp );
                            }

                            // check [+x,y,-z]
                            temp = new Index3D( index.X + x, y, index.Z - z );
                            if ( GetDistanceBetween( index, temp ) <= viewDistance )
                            {
                                indices.Add( temp );
                            }

                            // check [-x,y,+z]
                            temp = new Index3D( index.X - x, y, index.Z + z );
                            if ( GetDistanceBetween( index, temp ) <= viewDistance )
                            {
                                indices.Add( temp );
                            }

                            // check [-x,y,-z]
                            temp = new Index3D( index.X - x, y, index.Z - z );
                            if ( GetDistanceBetween( index, temp ) <= viewDistance )
                            {
                                indices.Add( temp );
                            }
                        }
                    }
                }

                // put ALL chunks on the chopping block
                _unloadList.AddRange( _chunks.Keys );
                for ( int i = 0; i < _unloadList.Count; ++i )
                {
                    // check if we can still see the chunk
                    Vector3 pos = IndexToPosition( _unloadList[ i ] );
                    if ( Vector3.Distance( _currentViewPosition, pos ) <= viewDistance )
                    {
                        // if we can, remove it from the unload list
                        _unloadList.RemoveAt( i );
                        --i;
                    }
                    else
                    {
                        // if we can't see it, we need to make sure it's not slated to be created
                        if ( indices.Contains( _unloadList[ i ] ) )
                        {
                            indices.Remove( _unloadList[ i ] );
                        }
                    }
                }


                // whatever's left in indices is what needs to be created
                _createList.AddRange( indices );


                ChunkUnloadingCallback();
                ChunkCreationCallback();


                // update the render queue
                lock ( _renderQueue )
                {
                    _renderQueue.Clear();
                    _renderQueue.AddRange( _chunks.Values );
                }
            }
        }

        /// <summary>
        /// Callback for the chunk creaton thread.
        /// </summary>
        private void ChunkCreationCallback()
        {
            Index3D current = PositionToIndex( _currentViewPosition );
            int count = 0;

            for ( int i = 0; i < _createList.Count; ++i )
            {
                Index3D idx = _createList[ i ];
                if ( GetDistanceBetween( current, idx ) > _currentViewDistance )
                {
                    continue;
                }

                // create the chunk
                lock ( _chunks )
                {
                    CreateChunk( idx );
                }

                ++count;
                if ( count % 16 == 0 )
                {
                    lock ( _renderQueue )
                    {
                        _renderQueue.Clear();
                        _renderQueue.AddRange( _chunks.Values );
                    }
                }
            }
            _createList.Clear();
        }

        /// <summary>
        /// Callback for the chunk unloading thread.
        /// </summary>
        private void ChunkUnloadingCallback()
        {
            for ( int i = 0; i < _unloadList.Count; ++i )
            {
                Index3D idx = _unloadList[ i ];
                _chunks[ idx ].Unload();
                _chunks.Remove( idx );
            }
            _unloadList.Clear();
        }




        /// <summary>
        /// Disposes of this world, including all of the chunks in it.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;

            // join the threads
            _chunkManagementThread.Join( 100 );
            // _chunkCreationThread.Join( 100 );
            // _chunkUnloadingThread.Join( 100 );

            // dispose of all chunks
            lock ( _chunks )
            {
                foreach ( Chunk chunk in _chunks.Values )
                {
                    chunk.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts the chunk management threads.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="distance">The view distance.</param>
        public void StartChunkManagement( Vector3 position, float distance )
        {
            _currentViewPosition = position;
            _currentViewDistance = distance;

            // create the chunk creation thread
            _chunkCreationThread = new Thread( new ThreadStart( ChunkCreationCallback ) );
            _chunkCreationThread.Name = "kyoob - Chunk Creation";
            _chunkCreationThread.IsBackground = true;
            // _chunkCreationThread.Start(); // DON'T FORGET TO JOIN

            // create the chunk unloading thread
            _chunkUnloadingThread = new Thread( new ThreadStart( ChunkUnloadingCallback ) );
            _chunkUnloadingThread.Name = "kyoob - Chunk Creation";
            _chunkUnloadingThread.IsBackground = true;
            // _chunkUnloadingThread.Start(); // DON'T FORGET TO JOIN

            // start the chunk management thread
            _chunkManagementThread = new Thread( new ThreadStart( ChunkManagementCallback ) );
            _chunkManagementThread.Name = "kyoob - Chunk Management";
            _chunkManagementThread.IsBackground = true;
            _chunkManagementThread.Start();
        }

        /// <summary>
        /// Gets the block at the given location.
        /// </summary>
        /// <param name="loc">The world location.</param>
        /// <returns></returns>
        public BlockType GetBlockType( Vector3 loc )
        {
            loc.X = (float)Math.Round( loc.X );
            loc.Y = (float)Math.Round( loc.Y );
            loc.Z = (float)Math.Round( loc.Z );

            Index3D index;
            Vector3 local = WorldToLocal( loc, out index );

            // make sure we're in the bounds (X)
            while ( local.X >= Chunk.Size )
            {
                local.X -= Chunk.Size;
                ++index.X;
            }
            while ( local.X < 0 )
            {
                local.X += Chunk.Size;
                --index.X;
            }

            // make sure we're in the bounds (Y)
            while ( local.Y >= Chunk.Size )
            {
                local.Y -= Chunk.Size;
                ++index.Y;
            }
            while ( local.Y < 0 )
            {
                local.Y += Chunk.Size;
                --index.Y;
            }

            // make sure we're in the bounds (Z)
            while ( local.Z >= Chunk.Size )
            {
                local.Z -= Chunk.Size;
                ++index.Z;
            }
            while ( local.Z < 0 )
            {
                local.Z += Chunk.Size;
                --index.Z;
            }

            // get the block type
            Chunk chunk = GetChunk( index );
            if ( chunk == null )
            {
                return _terrain.GetBlockType( loc );
            }
            else
            {
                return chunk.Blocks[ (int)local.X, (int)local.Y, (int)local.Z ].Type;
            }
        }

        /// <summary>
        /// Converts a chunk's local coordinates to world coordinates.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="x">The local X index.</param>
        /// <param name="y">The local Y index.</param>
        /// <param name="z">The local Z index.</param>
        /// <returns></returns>
        public Vector3 LocalToWorld( Vector3 center, int x, int y, int z )
        {
            return new Vector3(
                center.X + x - Chunk.Size / 2.0f,
                center.Y + y - Chunk.Size / 2.0f,
                center.Z + z - Chunk.Size / 2.0f
            );
        }

        /// <summary>
        /// Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="center">The center coordinates a chunk.</param>
        /// <param name="world">The world coordinates.</param>
        public Vector3 WorldToLocal( Vector3 center, Vector3 world )
        {
            return new Vector3(
                world.X - center.X + Chunk.Size / 2.0f,
                world.Y - center.Y + Chunk.Size / 2.0f,
                world.Z - center.Z + Chunk.Size / 2.0f
            );
        }

        /// <summary>
        /// [EXPERIMENTAL] Converts world coordinates to local coordinates.
        /// </summary>
        /// <param name="world">The world coordinates.</param>
        /// <param name="index">The index variable to populate.</param>
        public Vector3 WorldToLocal( Vector3 world, out Index3D index )
        {
            index = PositionToIndex( world );
            Vector3 center = IndexToPosition( index );
            return WorldToLocal( center, world );
        }

        /// <summary>
        /// Converts a position in the world to an index for a chunk.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Index3D PositionToIndex( Vector3 position )
        {
            return new Index3D(
                (int)Math.Round( position.X / Chunk.Size ),
                (int)Math.Round( position.Y / Chunk.Size ),
                (int)Math.Round( position.Z / Chunk.Size )
            );
        }

        /// <summary>
        /// Converts a chunk index to a position in the world.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <returns></returns>
        public Vector3 IndexToPosition( Index3D index )
        {
            return new Vector3(
                index.X * Chunk.Size,
                index.Y * Chunk.Size,
                index.Z * Chunk.Size
            );
        }

        /// <summary>
        /// Gets the chunk that contains the given world position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Chunk GetChunk( Vector3 position )
        {
            return GetChunk( PositionToIndex( position ) );
        }

        /// <summary>
        /// Gets the chunk at the given index.
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
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="camera">The current camera.</param>
        public void Update( GameTime gameTime, Camera camera )
        {
            _currentViewDistance = camera.GetSettings().ClipFar;
            _currentViewPosition = camera.Position;
        }

        /// <summary>
        /// Draws the world.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="camera">The current camera to use for getting visible tiles.</param>
        public void Draw( GameTime gameTime, Camera camera )
        {
            lock ( _renderQueue )
            {
                foreach ( Chunk chunk in _renderQueue )
                {
                    // if the chunk is non-existant, skip it
                    if ( chunk == null )
                    {
                        continue;
                    }

                    // only draw the chunk if we can see it
                    if ( !camera.CanSee( chunk.Bounds ) )
                    {
                        continue;
                    }

                    chunk.Draw( _renderer );
                }
            }

            _renderer.Render( camera );
        }

        /// <summary>
        /// Saves this world to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void SaveTo( Stream stream )
        {
            // create helper writer and write the magic number
            BinaryWriter bin = new BinaryWriter( stream );
            bin.Write( MagicNumber );

            // save the noise seed and number of chunks and then each chunk
            bin.Write( _terrain.Seed );
            bin.Write( _chunks.Count );
            foreach ( Index3D key in _chunks.Keys )
            {
                // write the index
                bin.Write( key.X );
                bin.Write( key.Y );
                bin.Write( key.Z );

                // write the chunk
                _chunks[ key ].SaveTo( stream );
            }
        }

        /// <summary>
        /// Reads a world's data from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="settings">The settings to use.</param>
        public static World ReadFrom( Stream stream, KyoobSettings settings )
        {
            // create our helper reader and make sure we find the world's magic number
            BinaryReader bin = new BinaryReader( stream );
            if ( bin.ReadInt32() != MagicNumber )
            {
                Terminal.WriteError( "Encountered invalid world in stream." );
                return null;
            }

            // now try to read the world
            try
            {
                World world = new World( bin, settings );
                return world;
            }
            catch ( Exception ex )
            {
                Terminal.WriteError( "Failed to load world." );
                Terminal.WriteError( "-- {0}", ex.Message );
                Terminal.WriteError( ex.StackTrace );

                return null;
            }
        }
    }
}