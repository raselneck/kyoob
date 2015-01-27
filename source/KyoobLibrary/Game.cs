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
// TODO : Make the world (or something) a component so we don't need to manually call everything
// TODO : Move most of the main code over to a separate library to facilitate plugins later
// TODO : Get rid of heavy JSON dependency and write custom INI parser

namespace Kyoob
{
    /// <summary>
    /// The game class for Kyoob.
    /// </summary>
    public sealed class Game : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// The path to the settings file.
        /// </summary>
        private const string SettingsFile = "settings.json";

        private static Game _instance;
        private GraphicsDeviceManager _graphics;
        private PresentationParameters _pp;
        private WorldRenderer _renderer;
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
        /// Gets the settings for this game.
        /// </summary>
        public GameSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new Kyoob game.
        /// </summary>
        private Game()
        {
            // handle when this game exits
            Exiting += OnGameExit;

            // create graphics device manager
            _graphics = new GraphicsDeviceManager( this );

            // set content directory
            Content.RootDirectory = "content";

            // load settings
            Settings = new GameSettings( this );
            Settings.Load( SettingsFile );

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
        /// Handles when this game is exiting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnGameExit( object sender, EventArgs args )
        {
            Settings.Save( SettingsFile );
        }

        /// <summary>
        /// Initializes the Kyoob game.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth  = Settings.WindowWidth;
            _graphics.PreferredBackBufferHeight = Settings.WindowHeight;
            _graphics.PreferMultiSampling = false;
            if ( !Settings.VSync )
            {
                IsFixedTimeStep = false;
                _graphics.SynchronizeWithVerticalRetrace = false;
            }
            _graphics.ApplyChanges();
            Window.AllowUserResizing = Settings.WindowResizable;

            base.Initialize();
        }

        /// <summary>
        /// Loads game-specific content.
        /// </summary>
        protected override void LoadContent()
        {
            // load the sprite sheet texture
            SpriteSheet.Instance.Texture = Content.Load<Texture2D>( "textures/spritesheet" );

            _pp = GraphicsDevice.PresentationParameters;
            Mouse.WindowHandle = Window.Handle;

            // create the world and renderer
            World.CreateInstance( 279186598 );
            //World.CreateInstance( (int)DateTime.Now.Ticks );
            _renderer = new WorldRenderer( World.Instance );
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
            if ( WasKeyPressed( Keys.P ) )
            {
                var handleInput = Player.Instance.HandleInput;
                IsMouseVisible = handleInput;
                if ( !handleInput )
                {
                    CenterMouse();
                }
                Player.Instance.HandleInput = !handleInput;
            }
            if ( WasKeyPressed( Keys.N ) )
            {
                var canNoClip = Player.Instance.CanNoClip;
                Player.Instance.CanNoClip = !canNoClip;
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
            GUI.DrawText( 10, 24, "Eye Position: {0}", Player.Instance.EyePosition );
            GUI.DrawText( 10, 38, "World Seed: {0}", Terrain.Instance.Seed );
#endif
            GUI.End();

            base.Draw( gameTime );
        }
    }
}