using System;
using System.IO;
using Kyoob.Blocks;
using Kyoob.Debug;
using Kyoob.Effects;
using Kyoob.Game.Entities;
using Kyoob.Game.Management;
using Kyoob.Graphics;
using Kyoob.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using XnaGame = Microsoft.Xna.Framework.Game;

namespace Kyoob.Game
{
    /// <summary>
    /// The main Kyoob engine.
    /// </summary>
    public class KyoobEngine : XnaGame
    {
        private const string SettingsFile = "./settings.json";

        private StateSystem _stateSystem;
        private KyoobSettings _settings;
        private GraphicsDeviceManager _graphics;

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
            _settings = new KyoobSettings( this );
            Renderer2D.Initialize( this );


            // try to load the settings file
            _settings.GameSettings.Import( SettingsFile );


            // initialize the terminal
            Terminal.Initialize( _settings );
            Terminal.Font = Content.Load<SpriteFont>( "font/consolas" );
            Terminal.RequestControl += ( sender, args ) =>
            {
                IsMouseVisible = true;
            };
            Terminal.ReleaseControl += ( sender, args ) =>
            {
                IsMouseVisible = false;
            };


            // create a perlin terrain generator
            //PerlinTerrain terrain  = new PerlinTerrain( 103695625 );
            PerlinTerrain terrain = new PerlinTerrain( (int)DateTime.Now.Ticks );
            terrain.Invert            = true;
            terrain.Octave            = 5;
            terrain.VerticalBias      = 1.0f / 36;
            terrain.HorizontalBias    = 1.0f / 113;
            terrain.Levels.WaterLevel = 0.450f;
            terrain.Levels.SetBounds( BlockType.Stone, 0.000f, 0.375f );
            terrain.Levels.SetBounds( BlockType.Sand,  0.375f, 0.500f );
            terrain.Levels.SetBounds( BlockType.Dirt,  0.500f, 1.000f );
            _settings.TerrainGenerator = terrain;


            // create the player
            CameraSettings camSettings = new CameraSettings( GraphicsDevice );
            camSettings.InitialPosition = new Vector3( 0.0f, 1.0f / terrain.VerticalBias + 4.0f, 0.0f );
            camSettings.ClipFar = _settings.GameSettings.ViewDistance * 2.0f;
            _settings.CameraSettings = camSettings;


            // load the sprite sheet and set the bounding box effect
            _settings.SpriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );


            // load our effects
            WorldEffect effect = new WorldEffect( Content.Load<Effect>( "fx/world" ) );
            effect.Texture = _settings.SpriteSheet.Texture;
            effect.LightAttenuation = 96.0f;


            // create the renderer
            _settings.EffectRenderer = new EffectRenderer( GraphicsDevice, effect );
            _settings.EffectRenderer.SkyBox = new SkyBox(
                Content.Load<Model>( "model/skybox" ),
                GraphicsDevice,
                new SkyBoxEffect( Content.Load<Effect>( "fx/skybox" ) ),
                Content.Load<TextureCube>( "tex/skybox-512" )
            );


            // create the world
            World world = new World( _settings );
            
            // create the game state system
            _stateSystem = new StateSystem( this, _settings, world );
            _stateSystem.AddState( "load", new LoadState( _stateSystem ) );
            _stateSystem.AddState( "play", new PlayState( _stateSystem, effect ) );
            _stateSystem.ChangeState( "load" );
        }

        /// <summary>
        /// Unloads all non-XNA managed Kyoob content.
        /// </summary>
        protected override void UnloadContent()
        {
            // dispose of the state system
            _stateSystem.Dispose();

            // export our settings
            _settings.GameSettings.Export( SettingsFile );
        }

        /// <summary>
        /// Updates the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Update( GameTime gameTime )
        {
            if ( Keyboard.GetState().IsKeyDown( Keys.Escape ) )
                this.Exit();

            _stateSystem.Update( gameTime );

            base.Update( gameTime );
        }

        /// <summary>
        /// Renders the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Draw( GameTime gameTime )
        {
            _stateSystem.Draw( gameTime );

            base.Draw( gameTime );
        }
    }
}