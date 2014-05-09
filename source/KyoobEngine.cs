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

#warning TODO : Clean up code.
#warning TODO : Finish terminal and convert Console.* to Terminal.*
#warning TODO : Input manager. (?)
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
        private DepthStencilState _depthState;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private SpriteSheet _spriteSheet;
        private Camera _camera;
        private BaseEffect _effect;
        private World _world;

        private int _frameCount;
        private int _fps;
        private double _tickCount;

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
            _depthState = _device.DepthStencilState;
            _spriteBatch = new SpriteBatch( _device );

            // load the font
            _font = Content.Load<SpriteFont>( "font/arial" );
            Terminal.Font = _font;

            // create the camera
            CameraSettings settings = new CameraSettings( _device );
            _camera = new Camera( settings );

            // load the textures
            _spriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );

            // load our effect
            _effect = new TextureEffect( Content.Load<Effect>( "fx/texture" ) );
            // ( (PointLightEffect)_effect ).LightAttenuation = 128.0f;
            ( (TextureEffect)_effect ).Texture = _spriteSheet.Texture;

            // create the world if we can't find the file
            if ( File.Exists( "./worlds/test.dat" ) )
            {
                using ( Stream stream = File.OpenRead( "./worlds/test.dat" ) )
                {
                    _world = World.ReadFrom( stream, _device, _effect, _spriteSheet );
                }
                if ( _world != null )
                {
                    Console.WriteLine( "Loaded world from file." );
                }
            }
            if ( _world == null )
            {
                _world = new World( _device, _effect, _spriteSheet );
                Console.WriteLine( "Created new world." );
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


            // draw the world
            _effect.Projection = _camera.Projection;
            _effect.View = _camera.View;
            _world.Draw( _camera );


            // draw FPS
            _spriteBatch.Begin();
            _spriteBatch.DrawString( _font, _fps + " FPS", Vector2.One * 10.0f, Color.White );
            Terminal.Draw( gameTime, _spriteBatch );
            _spriteBatch.End();
            _device.DepthStencilState = _depthState;


            // update FPS data
            ++_frameCount;
            _tickCount += gameTime.ElapsedGameTime.TotalSeconds;
            if ( _tickCount >= 1.0 )
            {
                _tickCount -= 1.0;
                _fps = _frameCount;
                _frameCount = 0;
            }


            base.Draw( gameTime );
        }
    }
}