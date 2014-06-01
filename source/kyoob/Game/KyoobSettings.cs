using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Graphics;
using Kyoob.Terrain;

using XnaGame = Microsoft.Xna.Framework.Game;

namespace Kyoob.Game
{
    /// <summary>
    /// Contains global Kyoob engine settings.
    /// </summary>
    public sealed class KyoobSettings
    {
        private XnaGame _game;
        private SpriteSheet _spriteSheet;
        private GameSettings _gameSettings;
        private CameraSettings _cameraSettings;
        private EffectRenderer _renderer;
        private TerrainGenerator _terrain;
        
        /// <summary>
        /// Gets the global graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _game.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets the global content manager.
        /// </summary>
        public ContentManager ContentManager
        {
            get
            {
                return _game.Content;
            }
        }

        /// <summary>
        /// Gets or sets the global camera settings.
        /// </summary>
        public CameraSettings CameraSettings
        {
            get
            {
                return _cameraSettings;
            }
            set
            {
                _cameraSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the global game settings.
        /// </summary>
        public GameSettings GameSettings
        {
            get
            {
                return _gameSettings;
            }
            set
            {
                _gameSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the global sprite sheet to use.
        /// </summary>
        public SpriteSheet SpriteSheet
        {
            get
            {
                return _spriteSheet;
            }
            set
            {
                _spriteSheet = value;
            }
        }

        /// <summary>
        /// Gets or sets the global terrain generator.
        /// </summary>
        public TerrainGenerator TerrainGenerator
        {
            get
            {
                return _terrain;
            }
            set
            {
                _terrain = value;
            }
        }

        /// <summary>
        /// Gets or sets the global effect renderer.
        /// </summary>
        public EffectRenderer EffectRenderer
        {
            get
            {
                return _renderer;
            }
            set
            {
                _renderer = value;
            }
        }

        /// <summary>
        /// Creates a new Kyoob settings object.
        /// </summary>
        /// <param name="game">The XNA game object to be created from.</param>
        public KyoobSettings( XnaGame game )
        {
            _game = game;
            _terrain = null;
            _renderer = null;
            _spriteSheet = null;
            _gameSettings = new GameSettings();
            _cameraSettings = new CameraSettings( _game.GraphicsDevice );
        }
    }
}