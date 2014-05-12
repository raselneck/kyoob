using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;
using Kyoob.Terrain;

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
        private TerrainGenerator _terrain;

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
            _terrain = terrain;

            // set our debuggin variables
            _drawCount = 0;
            _frameCount = 0;
            _timeCount = 0.0;
            _tickCount = 0.0;

            // start the chunk creation thread
            _creationThread = new Thread( new ThreadStart( ChunkCreationCallback ) );
            _creationThread.Name = "Test Chunk Creation";
            _creationThread.Start();
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
            _terrain = terrain;

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
        }

        /// <summary>
        /// Creates a chunk with the given indices.
        /// </summary>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <param name="z">The Z index.</param>
        private void CreateChunk( int x, int y, int z )
        {
            lock ( _chunks ) // gain a thread-exclusive lock
            {
                // create the chunk index
                Index3D index = new Index3D( x, y, z );

                // make sure we don't already have that chunk created
                if ( !_chunks.ContainsKey( index ) )
                {
                    // create the chunk and store it
                    Chunk chunk = new Chunk( this, new Vector3(
                        x * Chunk.Size,
                        y * Chunk.Size,
                        z * Chunk.Size
                    ) );
                    _chunks.Add( index, chunk );
                }
            }
        }



        private Thread _creationThread;

        /// <summary>
        /// The callback for the chunk creation thread.
        /// </summary>
        private void ChunkCreationCallback()
        {
            // just create some chunks
            for ( int x = 0; x <= 6; ++x )
            {
                for ( int y = 0; y <= 2; ++y )
                {
                    for ( int z = 0; z <= 6; ++z )
                    {
                        // locking takes place in CreateChunk
                        CreateChunk( x, y, z );
                        CreateChunk( x, y, -z );
                        CreateChunk( -x, y, z );
                        CreateChunk( -x, y, -z );
                    }
                }
            }

            // join the thread and write that we're done creating the world
            _creationThread.Join( 1000 );
            Terminal.WriteLine( Color.Cyan, 3.0, "World creation complete." );
            Terminal.WriteLine( Color.Cyan, 3.0, "Created {0} chunks.", _chunks.Count );
        }



        /// <summary>
        /// Disposes of this world, including all of the chunks in it.
        /// </summary>
        public void Dispose()
        {
            // join the thread
            _creationThread.Join( 100 );

            // dispose of all chunks
            foreach ( Chunk chunk in _chunks.Values )
            {
                chunk.Dispose();
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
                center.X + x - Chunk.Size / 2,
                center.Y + y - Chunk.Size / 2,
                center.Z + z - Chunk.Size / 2
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
                world.X - center.X + Chunk.Size / 2,
                world.Y - center.Y + Chunk.Size / 2,
                world.Z - center.Z + Chunk.Size / 2
            );
        }

        /// <summary>
        /// Gets the chunk at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Chunk GetChunk( Index3D index )
        {
            lock ( _chunks )
            {
                if ( _chunks.ContainsKey( index ) )
                {
                    return _chunks[ index ];
                }
            }
            return null;
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
            lock ( _chunks ) // gain thread-exclusive lock
            {
                // don't include locking time (even though it's only ~50ns)
                _watch = Stopwatch.StartNew();

                foreach ( Chunk chunk in _chunks.Values )
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

                _renderer.GraphicsDevice.Clear( _renderer.ClearColor );
                skySphere.Draw( camera );
                _renderer.Render();
            }


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