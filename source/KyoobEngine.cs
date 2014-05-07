using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Effects;

namespace Kyoob
{
    /// <summary>
    /// The main Kyoob engine.
    /// </summary>
    public class KyoobEngine : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;

        private Camera _camera;
        private PointLightEffect _effect;
        private List<Chunk> _chunks;

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
            _spriteBatch = new SpriteBatch( _device );

            _camera = new Camera( _device, Vector3.Zero, 0.0f, 0.0f );

            _effect = new PointLightEffect( Content.Load<Effect>( "fx/camlight" ) );

            // load the textures
            BlockTextures textures = BlockTextures.GetInstance();
            textures[ BlockType.Dirt ] = Content.Load<Texture2D>( "tex/dirt" );
            textures[ BlockType.Stone ] = Content.Load<Texture2D>( "tex/stone" );

            // create some chunks
            _chunks = new List<Chunk>();
            _chunks.Add( new Chunk( _device, new Vector3( 0.0f, 0.0f, 0.0f ) ) );
            _chunks.Add( new Chunk( _device, new Vector3( 0.0f, 0.0f, 8.0f ) ) );
            _chunks.Add( new Chunk( _device, new Vector3( 8.0f, 0.0f, 0.0f ) ) );
            _chunks.Add( new Chunk( _device, new Vector3( 8.0f, 0.0f, 8.0f ) ) );
            _chunks.Add( new Chunk( _device, new Vector3( 0.0f, 0.0f, -8.0f ) ) );
            _chunks.Add( new Chunk( _device, new Vector3( -8.0f, 0.0f, 0.0f ) ) );
            _chunks.Add( new Chunk( _device, new Vector3( -8.0f, 0.0f, -8.0f ) ) );
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

            _camera.Update( gameTime );
            _effect.LightPosition = _camera.Position;

            base.Update( gameTime );
        }

        /// <summary>
        /// Renders the Kyoob engine.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        protected override void Draw( GameTime gameTime )
        {
            _device.Clear( new Color( _effect.AmbientColor ) );

            _effect.Projection = _camera.Projection;
            _effect.View = _camera.View;

            // draw the chunks and time it
            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach ( Chunk chunk in _chunks )
            {
                chunk.Draw( _device, _effect, _camera );
            }
            watch.Stop();
            Console.WriteLine( "Chunk draw time: {0:0.00}ms", watch.Elapsed.TotalMilliseconds );


            base.Draw( gameTime );
        }
    }
}