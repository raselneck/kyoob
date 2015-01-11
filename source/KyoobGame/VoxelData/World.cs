using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;
using Kyoob.Graphics;
using MyDirectionalLight = Kyoob.Graphics.DirectionalLight;

// TODO : Move day/night cycle + sun info into a Nature class (not Environment because of System.Environment)
// TODO : Move terrain seed to be a part of the world and have terrain use that seed

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains information about the Kyoob world.
    /// </summary>
    public sealed class World : IDisposable
    {
        /// <summary>
        /// The duration of the day/night cycle.
        /// </summary>
        private const double DayNightCycleDuration = 200.0;

        /// <summary>
        /// The time offset to use in calculating the position of the sun for the day/night cycle.
        /// </summary>
        private const double DayNightCycleOffset = ( 20 * Math.PI / 180.0 + Math.PI ) * DayNightCycleDuration;

        private static World _instance;
        private TerrainGenerator _terrain;
        private WorldManager _manager;
        private Player _player;

        /// <summary>
        /// Gets the singleton world instance.
        /// </summary>
        public static World Instance
        {
            get
            {
                return CreateInstance();
            }
        }

        /// <summary>
        /// Creates the world instance if it does not already exist.
        /// </summary>
        /// <returns></returns>
        public static World CreateInstance()
        {
            if ( _instance == null )
            {
                _instance = new World();
            }
            return _instance;
        }

        /// <summary>
        /// The directional light representing the sun.
        /// </summary>
        public MyDirectionalLight SunLight
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new world.
        /// </summary>
        private World()
        {
            // setup the terrain
            _terrain = TerrainGenerator.Instance;
            //_terrain.Seed = 597176279; // pretty awesome starting seed
            _terrain.Seed = (int)DateTime.Now.Ticks;
            _terrain.Invert = true;
            _terrain.OctaveCount    = 5;
            _terrain.VerticalBias   = 1.0f / 42;
            _terrain.HorizontalBias = 1.0f / 167;
            _terrain.Levels.SetBounds( BlockType.Stone, 0.000f, 0.250f );
            _terrain.Levels.SetBounds( BlockType.Sand,  0.250f, 0.420f );
            _terrain.Levels.SetBounds( BlockType.Dirt,  0.420f, 1.000f );

            // create the player
            //Vector3 playerPosition = new Vector3( 400.0f, _terrain.HighestPoint + 2.0f, 164.0f );
            Vector3 playerPosition = new Vector3( 0.0f, _terrain.HighestPoint + 2.0f, 0.0f );
            _player = Player.CreateInstance( playerPosition );

            // create the world manager (it will begin running all by itself)
            _manager = new WorldManager( _player.Position );
        }

        /// <summary>
        /// Ensures this world is disposed of.
        /// </summary>
        ~World()
        {
            Dispose( false );
        }

        /// <summary>
        /// Loads content for the world.
        /// </summary>
        /// <param name="content">The content manager to use.</param>
        public void LoadContent( ContentManager content )
        {
            SunLight = SceneRenderer.Instance.AddDirectionalLight();
            SunLight.Color = Color.LightYellow;
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
            
            // calculate the sun position (simulate day/night cycle)
            var lightDir = new Vector3();
            lightDir.X = (float)( Math.Cos( ( gameTime.TotalGameTime.TotalSeconds + DayNightCycleOffset ) / DayNightCycleDuration ) );
            lightDir.Y = (float)( Math.Sin( ( gameTime.TotalGameTime.TotalSeconds + DayNightCycleOffset ) / DayNightCycleDuration ) );
            lightDir.Z = -lightDir.X;
            SunLight.Direction = lightDir;

            // TODO : Limit light direction or something so that the sun doesn't affect anything
            // when it's night time (aka when the sun is underneath the world)
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
        /// Disposes of this world.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose( bool disposing )
        {
            _manager.Dispose();
        }
    }
}