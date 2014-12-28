using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Kyoob.Entities;
using Kyoob.Graphics;
using Kyoob.VoxelData;

// TODO : Input manager class to easily support keyboard+mouse and controllers?

namespace Kyoob
{
    /// <summary>
    /// The game class for Kyoob.
    /// </summary>
    public sealed class Game : Microsoft.Xna.Framework.Game
    {
        private static Game _instance;
        private GraphicsDeviceManager _graphics;
        private PresentationParameters _pp;
        private SceneRenderer _renderer;
        private KeyboardState _oldKeys;
        private KeyboardState _newKeys;
        private FrameCounter _frameCounter;

        /// <summary>
        /// Gets the instance of the Kyoob game.
        /// </summary>
        public static Game Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new Game();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Creates a new Kyoob game.
        /// </summary>
        private Game()
            : base()
        {
            _graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "content";

            // add our frame counter component
            _frameCounter = new FrameCounter( this );
            Components.Add( _frameCounter );
        }

        /// <summary>
        /// Centers the mouse on this game's window.
        /// </summary>
        public void CenterMouse()
        {
            Mouse.SetPosition( _pp.BackBufferWidth / 2, _pp.BackBufferHeight / 2 );
        }

        /// <summary>
        /// Checks to see if the given key was pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool WasKeyPressed( Keys key )
        {
            return _oldKeys.IsKeyDown( key ) && _newKeys.IsKeyUp( key );
        }

        /// <summary>
        /// Initializes the Kyoob game.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth  = Settings.Instance.WindowWidth;
            _graphics.PreferredBackBufferHeight = Settings.Instance.WindowHeight;
            _graphics.PreferMultiSampling = false;
            if ( !Settings.Instance.VSync )
            {
                IsFixedTimeStep = false;
                _graphics.SynchronizeWithVerticalRetrace = false;
            }
            _graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// Loads game-specific content.
        /// </summary>
        protected override void LoadContent()
        {
            // ensure HiDef is supported
            if ( !GraphicsDevice.Adapter.IsProfileSupported( GraphicsProfile.HiDef ) )
            {
                throw new NotSupportedException( "ERROR: The current system does not support the HiDef graphics profile." );
            }

            // load the sprite sheet texture
            SpriteSheet.Instance.Texture = Content.Load<Texture2D>( "textures/spritesheet" );

            _pp = GraphicsDevice.PresentationParameters;
            Mouse.WindowHandle = Window.Handle;

            // create the world and renderer
            World.CreateInstance();
            _renderer = SceneRenderer.Instance;
            _renderer.LoadContent( Content );

#if DEBUG
            // this is just because I want to
            GraphicsInfo.Dump( "graphics.txt" );
#endif
        }

        /// <summary>
        /// Disposes of this game.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose( bool disposing )
        {
            World.Instance.Dispose();
            base.Dispose( disposing );
        }

        /// <summary>
        /// Updates the Kyoob game.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            // if escape is being pressed, exit
            _newKeys = Keyboard.GetState();
            if ( _newKeys.IsKeyDown( Keys.Escape ) )
            {
                this.Exit();
            }


            // check for keypresses
            if ( WasKeyPressed( Keys.D1 ) )
            {
                _renderer.RenderMode = RenderMode.Normal;
            }
            if ( WasKeyPressed( Keys.D2 ) )
            {
                _renderer.RenderMode = RenderMode.LightMap;
            }
            if ( WasKeyPressed( Keys.D3 ) )
            {
                _renderer.RenderMode = RenderMode.NormalMap;
            }
            if ( WasKeyPressed( Keys.P ) )
            {
                var handle = Player.Instance.HandleInput;
                IsMouseVisible = handle;
                if ( !handle )
                {
                    CenterMouse();
                }
                Player.Instance.HandleInput = !handle;
            }


            // update the world
            _renderer.Update( gameTime );

            // update old key state and update base game
            _oldKeys = _newKeys;
            base.Update( gameTime );
        }

        /// <summary>
        /// Draws the Kyoob game.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
        {
            // draw the world (handles clearing)
            _renderer.Draw( gameTime );

            // draw our GUI items
            GUI.Begin();
            GUI.DrawText( 10, 10, "{0} FPS", _frameCounter.FPS );
#if DEBUG
            GUI.DrawText( 10, 24, "Position: {0}", Player.Instance.Position );
            GUI.DrawText( 10, 38, "World Seed: {0}", TerrainGenerator.Instance.Seed );
#endif
            GUI.End();

            base.Draw( gameTime );
        }
    }
}