using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Debug;
using Kyoob.Effects;
using Kyoob.Terrain;

#warning TODO : When chunks are unloaded, their data should be saved to a file somehow so that block data can be retrieved in case a chunk was modified.

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

        private Stopwatch _watch;
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
        private bool _isDisposed;

        private int _drawCount;
        private int _frameCount;
        private double _timeCount;
        private double _tickCount;

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
        /// <param name="renderer">The renderer to use.</param>
        /// <param name="spriteSheet">The sprite sheet to use with each cube.</param>
        /// <param name="terrain">The terrain generator to use.</param>
        public World( EffectRenderer renderer, SpriteSheet spriteSheet, TerrainGenerator terrain )
        {
            // set variables
            _renderer = renderer;
            _spriteSheet = spriteSheet;
            _chunks = new Dictionary<Index3D, Chunk>();
            _renderQueue = new List<Chunk>();
            _indices = new HashSet<Index3D>();
            _terrain = terrain;
            _isDisposed = false;

            // set our debuggin variables
            _drawCount = 0;
            _frameCount = 0;
            _timeCount = 0.0;
            _tickCount = 0.0;

            // create our management lists
            _createList = new List<Index3D>();
            _unloadList = new List<Index3D>();

            StartChunkManagement();
            SetTerminalCommands();
        }

        /// <summary>
        /// Creates a new world by loading it from a stream.
        /// </summary>
        /// <param name="bin">The binary reader to use when reading the world.</param>
        /// <param name="renderer">The renderer to use.</param>
        /// <param name="spriteSheet">The sprite sheet to use with each cube.</param>
        /// <param name="terrain">The terrain generator to use.</param>
        private World( BinaryReader bin, EffectRenderer renderer, SpriteSheet spriteSheet, TerrainGenerator terrain )
        {
            // set variables
            _renderer = renderer;
            _spriteSheet = spriteSheet;
            _chunks = new Dictionary<Index3D, Chunk>();
            _renderQueue = new List<Chunk>();
            _indices = new HashSet<Index3D>();
            _terrain = terrain;
            _isDisposed = false;

            // load the seed (and terrain) and chunks
            _terrain.Seed = bin.ReadInt32();
            int count = bin.ReadInt32();
            for ( int i = 0; i < count; ++i )
            {
                // read the index, then the chunk, then record both
                Index3D index = new Index3D( bin.ReadInt32(), bin.ReadInt32(), bin.ReadInt32() );
                Chunk chunk = Chunk.ReadFrom( bin.BaseStream, this );
                if ( chunk != null )
                {
                    _chunks.Add( index, chunk );
                }
            }

            // create our management lists
            _createList = new List<Index3D>();
            _unloadList = new List<Index3D>();

            StartChunkManagement();
            SetTerminalCommands();
        }

        /// <summary>
        /// Starts the chunk management thread.
        /// </summary>
        private void StartChunkManagement()
        {
            // start the chunk creation thread
            _chunkManagementThread = new Thread( new ThreadStart( ChunkManagementCallback ) );
            _chunkManagementThread.Name = "Chunk Management";
            _chunkManagementThread.IsBackground = true;
            _chunkManagementThread.Start();
        }

        /// <summary>
        /// Sets the world commands in the terminal.
        /// </summary>
        private void SetTerminalCommands()
        {
            // world.reload
            Terminal.AddCommand( "world", "reload", ( string[] param ) =>
            {
                Terminal.WriteLine( Color.Red, 5.0, "world.reload is not implemented." );
            } );

            // terrain.vbias
            Terminal.AddCommand( "terrain", "vbias", ( string[] param ) =>
            {
                if ( param.Length < 1 )
                {
                    Terminal.WriteLine( Color.Red, 3.0, "Not enough parameters." );
                    return;
                }

                if ( _terrain is PerlinTerrain )
                {
                    float vbias = float.Parse( param[ 0 ] );
                    ( (PerlinTerrain)_terrain ).VerticalBias = vbias;
                }
            } );

            // terrain.hbias
            Terminal.AddCommand( "terrain", "hbias", ( string[] param ) =>
            {
                if ( param.Length < 1 )
                {
                    Terminal.WriteLine( Color.Red, 3.0, "Not enough parameters." );
                    return;
                }

                if ( _terrain is PerlinTerrain )
                {
                    float hbias = float.Parse( param[ 0 ] );
                    ( (PerlinTerrain)_terrain ).HorizontalBias = hbias;
                }
            } );
        }

        /// <summary>
        /// Creates a chunk with the given indices.
        /// </summary>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        private void CreateChunk( int x, int y, int z )
        {
            // if we're disposed, then there's no sense in creating a chunk
            if ( _isDisposed )
            {
                return;
            }

            // if the y is out of bounds, no need to create an empty chunk.
            if ( y < 0 || y > _terrain.Levels.GetLevel( BlockType.Air ) )
            {
                return;
            }

            /**
             * We're going to ASSUME that the calling thread already has an exclusive lock
             * on the chunk collection.
             */

            // create the chunk index
            Index3D index = new Index3D( x, y, z );

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

        /// <summary>
        /// The callback for the chunk management thread.
        /// </summary>
        private void ChunkManagementCallback()
        {
            Index3D temp, index;
            HashSet<Index3D> indices = new HashSet<Index3D>();
            int maxHeight = (int)Math.Ceiling( _terrain.Levels.GetHighestLevel() / Chunk.Size );

            while ( !_isDisposed )
            {
                index = PositionToIndex( _currentViewPosition );
                int maxDist = (int)( _currentViewDistance / Chunk.Size );
                
                // create a list of all of the indices to check
                for ( int x = 0; x < maxDist; ++x )
                {
                    for ( int z = 0; z < maxDist; ++z )
                    {
                        for ( int y = maxHeight; y >= 0; --y )
                        {
                            indices.Add( new Index3D( index.X + x, y, index.Z + z ) );
                            indices.Add( new Index3D( index.X + x, y, index.Z - z ) );
                            indices.Add( new Index3D( index.X - x, y, index.Z + z ) );
                            indices.Add( new Index3D( index.X - x, y, index.Z - z ) );
                        }
                    }
                }

                // create a copy of the current vertices
                HashSet<Index3D> copy = new HashSet<Index3D>( _indices );

                // now check all of the "local" indices
                foreach ( Index3D idx in indices )
                {
                    // if we have a chunk at the index, it doesn't need to be unloaded
                    if ( _chunks.ContainsKey( idx ) )
                    {
                        copy.Remove( idx );
                    }
                    // create the chunk if we need to
                    else
                    {
                        _createList.Add( idx );
                    }
                }
                indices.Clear();

                // check if which chunks we need to remove
                foreach ( Index3D idx in copy )
                {
                    if ( _chunks.ContainsKey( idx ) )
                    {
                        _unloadList.Add( idx );
                    }
                }

                lock ( _chunks )
                {
                    UnloadChunks();
                    CreateChunks();
                }

                lock ( _renderQueue )
                {
                    _renderQueue.Clear();
                    _renderQueue.AddRange( _chunks.Values );
                }

                // tell the garbage collector to collect shit
                Thread.Sleep( 256 );
            }
        }

        /// <summary>
        /// Creates all of the chunks in the create list.
        /// </summary>
        private void CreateChunks()
        {
            lock ( _createList )
            {
                if ( _createList.Count > 0 )
                {
                    Terminal.WriteLine( Color.White, 3.0, "Creating {0} chunks...", _createList.Count );
                }
                for ( int i = 0; i < _createList.Count; ++i )
                {
                    Index3D idx = _createList[ i ];
                    CreateChunk( idx.X, idx.Y, idx.Z );
                }
                _createList.Clear();
            }
        }

        /// <summary>
        /// Unloads all of the chunks in the unload list.
        /// </summary>
        private void UnloadChunks()
        {
            lock ( _unloadList )
            {
                if ( _unloadList.Count > 0 )
                {
                    Terminal.WriteLine( Color.White, 3.0, "Unloading {0} chunks...", _unloadList.Count );
                }
                for ( int i = 0; i < _unloadList.Count; ++i )
                {
                    Index3D idx = _unloadList[ i ];
                    _chunks[ idx ].Unload();
                    _chunks.Remove( idx );
                }
                _unloadList.Clear();
            }
        }




        /// <summary>
        /// Disposes of this world, including all of the chunks in it.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;

            // join the thread
            _chunkManagementThread.Join( 100 );

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
        /// Converts a position in the world to an index for a chunk.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Index3D PositionToIndex( Vector3 position )
        {
            return new Index3D(
                (int)position.X / Chunk.Size,
                (int)position.Y / Chunk.Size,
                (int)position.Z / Chunk.Size
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
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="camera">The current camera.</param>
        public void Update( GameTime gameTime, Camera camera )
        {
            _currentViewDistance = camera.ViewDistance;
            _currentViewPosition = camera.Position;
        }

        /// <summary>
        /// Draws the world.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="camera">The current camera to use for getting visible tiles.</param>
        /// <param name="skySphere">The sky sphere to draw.</param>
        public void Draw( GameTime gameTime, Camera camera, SkySphere skySphere )
        {
            // time how long it takes to draw our chunks
            int count = 0;
            lock ( _renderQueue )
            {
                _watch = Stopwatch.StartNew();

                foreach ( Chunk chunk in _renderQueue )
                {
                    // only draw the chunk if we can see it
                    if ( !camera.CanSee( chunk.Bounds ) )
                    {
                        continue;
                    }

                    chunk.Draw( _renderer );
                    ++count;
                }

                _watch.Stop();
            }

            _renderer.GraphicsDevice.Clear( _renderer.ClearColor );
            skySphere.Draw( camera );
            _renderer.Render();


            // update our average chunk drawing information
            ++_frameCount;
            _drawCount += count;
            _tickCount += gameTime.ElapsedGameTime.TotalSeconds;
            _timeCount += _watch.Elapsed.TotalMilliseconds;
            if ( _tickCount >= 1.0 )
            {
                Terminal.WriteLine( Color.Yellow, 0.95,
                    "{0:0.00} chunks in {1:0.00}ms",
                    (float)_drawCount / _frameCount,
                           _timeCount / _frameCount
                );

                _frameCount = 0;
                _tickCount -= 1.0;
                _timeCount = 0.0;
                _drawCount = 0;
            }
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
        /// <param name="renderer">The renderer to use.</param>
        /// <param name="spriteSheet">The world's sprite sheet.</param>
        /// <param name="terrain">The terrain generator to use.</param>
        public static World ReadFrom( Stream stream, EffectRenderer renderer, SpriteSheet spriteSheet, TerrainGenerator terrain )
        {
            // create our helper reader and make sure we find the world's magic number
            BinaryReader bin = new BinaryReader( stream );
            if ( bin.ReadInt32() != MagicNumber )
            {
                Terminal.WriteLine( Color.Red, 3.0, "Encountered invalid world in stream." );
                return null;
            }

            // now try to read the world
            try
            {
                World world = new World( bin, renderer, spriteSheet, terrain );
                return world;
            }
            catch ( Exception ex )
            {
                Terminal.WriteLine( Color.Red, 3.0, "Failed to load world." );
                Terminal.WriteLine( Color.Red, 3.0, "-- {0}", ex.Message );
                // Terminal.WriteLine( ex.StackTrace );

                return null;
            }
        }
    }
}