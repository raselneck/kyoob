using System.IO;
using Kyoob.Blocks;
using Kyoob.Debug;
using Kyoob.Effects;
using Kyoob.Game.Entities;
using Kyoob.Graphics;
using Kyoob.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using XnaGame = Microsoft.Xna.Framework.Game;

#warning TODO : Create input manager for key/mouse and controller
#warning TODO : Create Renderer3D to facilitate 3D rendering like Renderer2D
#warning TODO : Look into Lindgren.Network

namespace Kyoob.Game
{
    /// <summary>
    /// The main Kyoob engine.
    /// </summary>
    public class KyoobEngine : XnaGame
    {
        private const string SettingsFile = "./settings.json";

        private World _world;
        private Player _player;
        private WorldEffect _effect;
        private KyoobSettings _settings;
        private GraphicsDevice _device;
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
            _device = GraphicsDevice;
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
            PerlinTerrain terrain  = new PerlinTerrain( 103695625 );
            terrain.Invert            = true;
            terrain.Octave            = 5;
            terrain.VerticalBias      = 1.0f / 64;
            terrain.HorizontalBias    = 1.0f / 113;
            terrain.Levels.WaterLevel = 0.450f;
            terrain.Levels.SetBounds( BlockType.Stone, 0.000f, 0.375f );
            terrain.Levels.SetBounds( BlockType.Sand,  0.375f, 0.500f );
            terrain.Levels.SetBounds( BlockType.Dirt,  0.500f, 1.000f );
            _settings.TerrainGenerator = terrain;


            // create the player
            CameraSettings camSettings = new CameraSettings( _device );
            camSettings.InitialPosition = new Vector3( -48.0f, 1.0f / terrain.VerticalBias + 4.0f, -80.0f );
            camSettings.ClipFar = _settings.GameSettings.ViewDistance * 2.0f;
            _settings.CameraSettings = camSettings;
            _player = new Player( _settings );


            // load the sprite sheet and set the bounding box effect
            _settings.SpriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );


            // load our effects
            _effect = new WorldEffect( Content.Load<Effect>( "fx/world" ) );
            _effect.Texture = _settings.SpriteSheet.Texture;
            _effect.LightAttenuation = 96.0f;


            // create the renderer
            _settings.EffectRenderer = new EffectRenderer( _device, _effect, _player.Camera );
            _settings.EffectRenderer.SkyBox = new SkyBox(
                Content.Load<Model>( "model/skybox" ),
                _device,
                new SkyBoxEffect( Content.Load<Effect>( "fx/skybox" ) ),
                Content.Load<TextureCube>( "tex/skybox-512" )
            );


            // create the world
            _world = new World( _settings );
            _player.World = _world;
            _world.StartChunkManagement( _player.Position, _settings.GameSettings.ViewDistance );
        }

        /// <summary>
        /// Unloads all non-XNA managed Kyoob content.
        /// </summary>
        protected override void UnloadContent()
        {
            // dispose of the world
            _world.Dispose();

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


            Terminal.Update( gameTime );
            _player.Update( gameTime );
            _effect.LightPosition = _player.Camera.EyePosition;
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
            _settings.EffectRenderer.ClearColor = new Color( _effect.AmbientColor );
            

            // draw the world and the terminal
            _effect.Projection = _player.Camera.Projection;
            _effect.View = _player.Camera.View;
            _world.Draw( gameTime, _player.Camera );
            _player.Draw( gameTime, _effect );
            Terminal.Draw( gameTime );


            base.Draw( gameTime );
        }
    }
}