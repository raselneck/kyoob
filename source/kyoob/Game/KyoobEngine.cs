using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Debug;
using Kyoob.Effects;
using Kyoob.Game.Entities;
using Kyoob.Graphics;
using Kyoob.Terrain;

using XnaGame = Microsoft.Xna.Framework.Game;

namespace Kyoob.Game
{
    /// <summary>
    /// The main Kyoob engine.
    /// </summary>
    public class KyoobEngine : XnaGame
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _device;

        private SpriteSheet _spriteSheet;
        private EffectRenderer _renderer;
        private PointLightEffect _effect;
        private World _world;
        private Player _player;

        /// <summary>
        /// Creates a new Kyoob engine.
        /// </summary>
        public KyoobEngine()
        {
            _graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "content";
        }

        /// <summary>
        /// Initializes the Kyoob engine.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.PreferMultiSampling = false;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            // IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// Loads all XNA-compiled content for the Kyoob engine.
        /// </summary>
        protected override void LoadContent()
        {
            _device = GraphicsDevice;

            // initialize the terminal
            Terminal.Initialize( this );
            Terminal.Font = Content.Load<SpriteFont>( "font/arial" );
            Terminal.RequestControl += ( sender, args ) =>
            {
                IsMouseVisible = true;
            };
            Terminal.ReleaseControl += ( sender, args ) =>
            {
                IsMouseVisible = false;
            };


            // create a perlin terrain generator (needs work)
            // PerlinTerrain terrain  = new PerlinTerrain( (int)DateTime.Now.Ticks );
            PerlinTerrain terrain  = new PerlinTerrain( 103695625 );
            terrain.VerticalBias   = 1.0f / 49;
            terrain.HorizontalBias = 1.0f / 79;
            terrain.Levels.WaterLevel = 0.500f;
            terrain.Levels.SetBounds( BlockType.Stone, 0.000f, 0.250f );
            terrain.Levels.SetBounds( BlockType.Sand,  0.250f, 0.625f );
            terrain.Levels.SetBounds( BlockType.Dirt,  0.625f, 1.000f );


            // create the player
            CameraSettings settings = new CameraSettings( _device );
            settings.InitialPosition = new Vector3( 0.0f, 1.0f / terrain.VerticalBias, 0.0f );
            _player = new Player( _device, settings );


            // load the sprite sheet and set the bounding box effect
            _spriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );


            // load our effects
            _effect = new PointLightEffect( Content.Load<Effect>( "fx/world" ) );
            _effect.Texture = _spriteSheet.Texture;
            _effect.LightAttenuation = 96.0f;


            // create the renderer
            _renderer = new EffectRenderer( _device, _effect, _player.Camera );
            _renderer.SkyBox = new SkyBox(
                Content.Load<Model>( "model/skybox" ),
                _device,
                new SkyBoxEffect( Content.Load<Effect>( "fx/skybox" ) ),
                Content.Load<TextureCube>( "tex/skybox-512" )
            );


            // create the world if we can't find the file
            const string WorldFile = "./worlds/test.dat";
            if ( File.Exists( WorldFile ) )
            {
                using ( Stream stream = File.OpenRead( WorldFile ) )
                {
                    _world = World.ReadFrom( stream, _renderer, _spriteSheet, terrain );
                }
                if ( _world != null )
                {
                    Terminal.WriteInfo( "Loaded world from file." );
                }
            }
            if ( _world == null )
            {
                _world = new World( _renderer, _spriteSheet, terrain );
                Terminal.WriteInfo( "Creating new world..." );
            }

            // set the player's world
            _player.World = _world;
        }

        /// <summary>
        /// Unloads all non-XNA managed Kyoob content.
        /// </summary>
        protected override void UnloadContent()
        {
            // create world directory
            if ( !Directory.Exists( "./worlds/" ) )
            {
                Directory.CreateDirectory( "./worlds/" );
            }


            /*
            // save the world (figuratively, of course)
            using ( Stream stream = File.Create( "./worlds/test.dat" ) )
            {
                _world.SaveTo( stream );
            }
            */

            _world.Dispose();
        }

        /// <summary>
        /// Updates the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Update( GameTime gameTime )
        {
            if ( Keyboard.GetState().IsKeyDown( Keys.Escape ) )
                this.Exit();


            Terminal.Update( gameTime );
            _player.Update( gameTime );
            _effect.LightPosition = _player.Camera.Position;
            _world.Update( gameTime, _player.Camera );


            base.Update( gameTime );
        }

        /// <summary>
        /// Renders the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Draw( GameTime gameTime )
        {
            // clear based on ambient color if we can
            _renderer.ClearColor = new Color( _effect.AmbientColor );


            // draw the world and the terminal
            _effect.Projection = _player.Camera.Projection;
            _effect.View = _player.Camera.View;
            _renderer.GraphicsDevice.Clear( _renderer.ClearColor );
            _world.Draw( gameTime, _player.Camera );
            _player.Draw( gameTime, _effect );
            Terminal.Draw( gameTime );


            base.Draw( gameTime );
        }
    }
}