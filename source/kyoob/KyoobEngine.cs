using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Effects;
using Kyoob.Terrain;

#warning TODO : Everything is super fucking disorganized right now.
#warning TODO : There's really no need for chunk->world and world->chunk coordinate conversions for the terrain generator.
#warning TODO : Input manager. (?)
#warning TODO : Add input/commands to terminal.
#warning TODO : Add some more lighting stuff, maybe shadows.
#warning TODO : Implement effect manager.
#warning TODO : Motion blur. (?)
#warning TODO : Physics.

namespace Kyoob
{
    /// <summary>
    /// The main Kyoob engine.
    /// </summary>
    public class KyoobEngine : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _device;

        private SpriteSheet _spriteSheet;
        private Camera _camera;
        private SkySphere _skySphere;
        private EffectRenderer _renderer;
        private BaseEffect _effect;
        private World _world;

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
            _graphics.PreferMultiSampling = true;
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


            // create a perlin terrain generator (needs work)
            PerlinTerrain terrain = new PerlinTerrain( 0 );
            terrain.Seed = 114;
            terrain.HorizontalBias = 1 / 53.0f;
            terrain.VerticalBias = 24.0f;
            terrain.Levels.WaterLevel = 6.0f;
            terrain.Levels.AddLevel( 4.00f, BlockType.Stone );
            terrain.Levels.AddLevel( 9.00f, BlockType.Sand );
            terrain.Levels.AddLevel( 24.0f, BlockType.Dirt );


            // create the camera
            CameraSettings settings = new CameraSettings( _device );
            settings.InitialPosition = new Vector3( 0.0f, 16.0f, 0.0f );
            _camera = new Camera( settings );


            // load the sky sphere
            _skySphere = new SkySphere(
                Content.Load<Model>( "model/skysphere" ),
                _device,
                new SkySphereEffect( Content.Load<Effect>( "fx/skysphere" ) ),
                Content.Load<TextureCube>( "tex/skysphere" )
            );


            // load the textures
            _spriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );


            // load our effects
            PointLightEffect world = new PointLightEffect( Content.Load<Effect>( "fx/world" ) );
            world.Texture = _spriteSheet.Texture;
            world.LightAttenuation = 96.0f;
            AlphaEffect alpha = new AlphaEffect( Content.Load<Effect>( "fx/alpha" ) );


            // create the renderer
            _renderer = new EffectRenderer( _device, world, alpha );
            _effect = world;


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
                    Terminal.WriteLine( Color.Cyan, 3.0, "Loaded world from file." );
                }
            }
            if ( _world == null )
            {
                _world = new World( _renderer, _spriteSheet, terrain );
                Terminal.WriteLine( Color.Cyan, 3.0, "Creating new world..." );
            }
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
            _camera.Update( gameTime, _world );
            if ( _effect is PointLightEffect )
            {
                ( (PointLightEffect)_effect ).LightPosition = _camera.Position;
            }


            base.Update( gameTime );
        }

        /// <summary>
        /// Renders the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Draw( GameTime gameTime )
        {
            // clear based on ambient color if we can
            if ( _effect is LightedEffect )
            {
                // _renderer.ClearColor = new Color(  )
            }
            else
            {
                _renderer.ClearColor = Color.CornflowerBlue;
            }


            // draw the world and the terminal
            _effect.Projection = _camera.Projection;
            _effect.View = _camera.View;
            _world.Draw( gameTime, _camera, _skySphere );
            Terminal.Draw( gameTime );


            base.Draw( gameTime );
        }
    }
}