using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;
using Kyoob.Graphics;

// TODO : Make some kind of environment class for weather and such

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains information about the Kyoob world.
    /// </summary>
    public sealed class World : IDisposable
    {
        private static World _instance;
        private WorldManager _manager;
        private Terrain _terrain;
        private Player _player;

        /// <summary>
        /// Gets the singleton world instance.
        /// </summary>
        public static World Instance
        {
            get
            {
                return CreateInstance( (int)DateTime.Now.Ticks );
            }
        }

        /// <summary>
        /// Creates the world instance if it does not already exist.
        /// </summary>
        /// <returns></returns>
        public static World CreateInstance( int seed )
        {
            if ( _instance == null )
            {
                _instance = new World( seed );
            }
            return _instance;
        }

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="seed">The world seed.</param>
        private World( int seed )
        {
            // setup the terrain
            //_terrain = Terrain.CreateInstance( (int)DateTime.Now.Ticks );
            _terrain = Terrain.CreateInstance( seed );

            // create the player
            Vector3 playerPosition = new Vector3( 0.0f, Terrain.MaximumHeight + 2.0f, 0.0f );
            _player = Player.CreateInstance( playerPosition );

            // create the world manager (it will begin running all by itself)
            _manager = new WorldManager( this, playerPosition );
        }

        /// <summary>
        /// Ensures this world is disposed of.
        /// </summary>
        ~World()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of this world.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update( GameTime gameTime )
        {
            // update the player
            _player.Update( gameTime );
        }

        /// <summary>
        /// Draws the world's chunks.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw( GameTime gameTime )
        {
            _manager.DrawChunks( _player.Camera );
        }

        /// <summary>
        /// Finds the chunk that contains the given world position.
        /// </summary>
        /// <param name="world">The world position.</param>
        /// <returns></returns>
        public Chunk FindChunk( Vector3 world )
        {
            Vector3 index, center;
            Position.WorldToLocal( world, out index, out center );
            return _manager.GetChunkFromCenter( center );
        }

        /// <summary>
        /// Gets the block at the given world coordinates.
        /// </summary>
        /// <param name="x">The requested block's X coordinate.</param>
        /// <param name="y">The requested block's Y coordinate.</param>
        /// <param name="z">The requested block's Z coordinate.</param>
        /// <returns></returns>
        public Block GetBlock( int x, int y, int z )
        {
            return GetBlock( new Vector3( x, y, z ) );
        }

        /// <summary>
        /// Gets the block at the given world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates.</param>
        /// <returns></returns>
        public Block GetBlock( Vector3 world )
        {
            // get local positions
            Vector3 chunkIndex, localIndex;
            Position.WorldToLocal( world, out localIndex, out chunkIndex );
            
            // get the block
            var chunk = _manager.GetChunkFromCenter( chunkIndex );
            var block = chunk.Data[ localIndex ];

            return block;
        }

        /// <summary>
        /// Disposes of this world.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose( bool disposing )
        {
            _manager.Dispose();
        }
    }
}