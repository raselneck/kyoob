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

        private KyoobSettings _settings;
        private ChunkManager _chunkManager;
        private List<Chunk> _renderList;

        /// <summary>
        /// Gets the chunk manager used by this world.
        /// </summary>
        public ChunkManager ChunkManager
        {
            get
            {
                return _chunkManager;
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
        /// Performs common initialization logic for the world.
        /// </summary>
        /// <param name="settings">The settings to use.</param>
        private void CommonInitialization( KyoobSettings settings )
        {
            // set variables
            _settings = settings;
            _chunkManager = new ChunkManager( _settings, this );
            _renderList = new List<Chunk>();

            SetTerminalCommands();
        }

        /// <summary>
        /// Sets the world commands in the terminal.
        /// </summary>
        private void SetTerminalCommands()
        {
            // world.seed to get the seed
            Terminal.AddCommand( "world", "seed", ( string[] param ) =>
            {
                Terminal.WriteInfo( "seed: {0}", _settings.TerrainGenerator.Seed );
            } );
        }


        /// <summary>
        /// Gets the world distance between two indices.
        /// </summary>
        /// <param name="a">The first index.</param>
        /// <param name="b">The second index.</param>
        /// <returns></returns>
        public static float Distance( Index3D a, Index3D b )
        {
            return Vector3.Distance( IndexToPosition( a ), IndexToPosition( b ) );
        }

        /// <summary>
        /// Converts a chunk's local coordinates to world coordinates.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="x">The local X index.</param>
        /// <param name="y">The local Y index.</param>
        /// <param name="z">The local Z index.</param>
        /// <returns></returns>
        public static Vector3 LocalToWorld( Vector3 center, int x, int y, int z )
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
        public static Vector3 WorldToLocal( Vector3 center, Vector3 world )
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
        public static Vector3 WorldToLocal( Vector3 world, out Index3D index )
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
        public static Index3D PositionToIndex( Vector3 position )
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
        public static Vector3 IndexToPosition( Index3D index )
        {
            return new Vector3(
                index.X * Chunk.Size,
                index.Y * Chunk.Size,
                index.Z * Chunk.Size
            );
        }


        /// <summary>
        /// Disposes of this world, including all of the chunks in it.
        /// </summary>
        public void Dispose()
        {
            _chunkManager.Dispose();
        }

        /// <summary>
        /// Starts the chunk management threads.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="distance">The view distance.</param>
        public void StartChunkManagement( Vector3 position, float distance )
        {
            _chunkManager.Start( position, distance );
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
                return _settings.TerrainGenerator.GetBlockType( loc );
            }
            else
            {
                return chunk.Blocks[ (int)local.X, (int)local.Y, (int)local.Z ].Type;
            }
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
            return _chunkManager.GetChunk( index );
        }

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="camera">The current camera.</param>
        public void Update( GameTime gameTime, Camera camera )
        {
            _chunkManager.ViewDistance = _settings.GameSettings.ViewDistance;
            _chunkManager.ViewPosition = camera.Position;

            if ( !_chunkManager.IsBusy )
            {
                _renderList = _chunkManager.GetRenderList();
            }
        }

        /// <summary>
        /// Draws the world.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="camera">The current camera to use for getting visible tiles.</param>
        public void Draw( GameTime gameTime, Camera camera )
        {
            foreach ( Chunk chunk in _renderList )
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

                chunk.Draw( _settings.EffectRenderer );
            }

            _settings.EffectRenderer.Render( camera );
        }
    }
}