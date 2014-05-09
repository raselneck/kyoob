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

#warning TODO : Add wireframe vertex buffer for chunks. (?)
#warning TODO : Input manager. (?)
#warning TODO : Add commands to terminal.
#warning TODO : Add some more lighting stuff, maybe shadows.
#warning TODO : Implement effect manager.
#warning TODO : Create base terrain generator and implementations for worlds.
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


            // create the camera
            CameraSettings settings = new CameraSettings( _device );
            _camera = new Camera( settings );


            // load the textures
            _spriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );


            // load our effect
            _effect = new PointLightEffect( Content.Load<Effect>( "fx/camlight" ) );
            // ( (PointLightEffect)_effect ).LightAttenuation = 128.0f;
            ( (PointLightEffect)_effect ).Texture = _spriteSheet.Texture;


            // create the world if we can't find the file
            if ( File.Exists( "./worlds/test.dat" ) )
            {
                using ( Stream stream = File.OpenRead( "./worlds/test.dat" ) )
                {
                    _world = World.ReadFrom( stream, _device, _effect, _spriteSheet );
                }
                if ( _world != null )
                {
                    Terminal.WriteLine( "Loaded world from file." );
                }
            }
            if ( _world == null )
            {
                _world = new World( _device, _effect, _spriteSheet );
                Terminal.WriteLine( "Created new world." );
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


            // only save the file if it doesn't exist
            if ( !File.Exists( "./worlds/test.dat" ) )
            {
                // save the world
                using ( Stream stream = File.Create( "./worlds/test.dat" ) )
                {
                    _world.SaveTo( stream );
                }
            }
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
            _camera.Update( gameTime );
            ( (PointLightEffect)_effect ).LightPosition = _camera.Position;


            base.Update( gameTime );
        }

        /// <summary>
        /// Renders the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Draw( GameTime gameTime )
        {
            // _device.Clear( Color.CornflowerBlue );
            _device.Clear( new Color( ( (LightedEffect)_effect ).AmbientColor ) );


            // draw the world and the terminal
            _effect.Projection = _camera.Projection;
            _effect.View = _camera.View;
            _world.Draw( gameTime, _camera );
            Terminal.Draw( gameTime );


            base.Draw( gameTime );
        }
    }
}