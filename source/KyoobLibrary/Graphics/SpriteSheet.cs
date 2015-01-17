using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.VoxelData;

// TODO : Rebuild chunk voxel buffers when the texture is changed
// TODO : Can this be redone or incorporated into world renderer?
// TODO : Look into using separate textures for all blocks

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains an easy way to interact with a sprite sheet.
    /// </summary>
    public sealed class SpriteSheet
    {
        private const int NumSpritesX = 4;
        private const int NumSpritesY = 4;
        private static SpriteSheet _instance;
        private Texture2D _texture;
        private Point _spriteSize;
        private Vector2 _texCoordSize;

        /// <summary>
        /// Gets the sprite sheet instance.
        /// </summary>
        public static SpriteSheet Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new SpriteSheet();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets this sprite sheet's texture.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                // ensure texture isn't null
                if ( value == null )
                {
                    throw new ArgumentNullException( "value" );
                }

                // set texture and sprite/tex coord sizes
                _texture = value;
                _spriteSize.X = _texture.Width / NumSpritesX;
                _spriteSize.Y = _texture.Height / NumSpritesY;
                _texCoordSize.X = (float)_spriteSize.X / _texture.Width;
                _texCoordSize.Y = (float)_spriteSize.Y / _texture.Height;

                // now trigger the event
                var handler = TextureChanged;
                if ( handler != null )
                {
                    handler( this, EventArgs.Empty );
                }
            }
        }

        /// <summary>
        /// The event that is fired whenever this sprite sheet's texture is changed.
        /// </summary>
        public event EventHandler<EventArgs> TextureChanged;

        /// <summary>
        /// Gets the size of each individual sprite.
        /// </summary>
        public Point SpriteSize
        {
            get
            {
                return _spriteSize;
            }
        }

        /// <summary>
        /// Gets the size of each sprite in normalized texture coordinates.
        /// </summary>
        public Vector2 TexCoordSize
        {
            get
            {
                return _texCoordSize;
            }
        }

        /// <summary>
        /// Creates the sprite sheet.
        /// </summary>
        private SpriteSheet()
        {
        }
    }
}