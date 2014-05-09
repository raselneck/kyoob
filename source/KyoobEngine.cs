using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Effects;

#warning TODO : Try being able to draw individual octrees

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

            // create the camera
            CameraSettings settings = new CameraSettings( _device );
            _camera = new Camera( settings );

            // load our effect
            _effect = new PointLightEffect( Content.Load<Effect>( "fx/camlight" ) );
            ( (PointLightEffect)_effect ).LightAttenuation = 32.0f;

            // load the textures
            _spriteSheet = new SpriteSheet( Content.Load<Texture2D>( "tex/spritesheet" ) );

            _world = new World( _device, _effect, _spriteSheet );
        }

        /// <summary>
        /// Unloads all non-XNA managed Kyoob content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Updates the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Update( GameTime gameTime )
        {
            if ( Keyboard.GetState().IsKeyDown( Keys.Escape ) )
                this.Exit();

            // update the camera and effect
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
            _device.Clear( new Color( ( (PointLightEffect)_effect ).AmbientColor ) );

            // draw the world
            _effect.Projection = _camera.Projection;
            _effect.View = _camera.View;
            _world.Draw( _camera );

            // draw FPS
            _spriteBatch.Begin();
            _spriteBatch.DrawString( _font, _fps + " FPS", Vector2.One * 10.0f, Color.White );
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