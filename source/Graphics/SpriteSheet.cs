using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.VoxelData;

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
        /// Gets the texture coordinates for the given block information.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <param name="face">The block face.</param>
        public Vector2 GetTextureCoords( BlockType type, BlockFace face )
        {
            // check the block type
            switch ( type )
            {
                case BlockType.Grass:
                    return GetGrassCoords( face );
                case BlockType.Bedrock:
                    return GetCoords( 0, 2 );
                case BlockType.Dirt:
                    return GetCoords( 1, 0 );
                case BlockType.Sand:
                    return GetCoords( 1, 1 );
                case BlockType.Stone:
                    return GetCoords( 0, 1 );
                case BlockType.Water:
                    return GetCoords( 2, 0 );
                default:
                    return GetCoords( 0, 0 );
            }
        }


        /// <summary>
        /// Creates the sprite sheet.
        /// </summary>
        private SpriteSheet()
        {
        }

        /// <summary>
        /// Gets the texture coordinates for the sprite at the given X and Y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns></returns>
        private Vector2 GetCoords( int x, int y )
        {
            return new Vector2( x * _texCoordSize.X, y * _texCoordSize.Y );
        }

        /// <summary>
        /// Gets the texture coordinates for a grass block on the given face.
        /// </summary>
        /// <param name="face">The block face.</param>
        /// <returns></returns>
        private Vector2 GetGrassCoords( BlockFace face )
        {
            switch ( face )
            {
                case BlockFace.Left:
                case BlockFace.Right:
                case BlockFace.Front:
                case BlockFace.Back:
                    return GetCoords( 2, 2 );
                case BlockFace.Top:
                    return GetCoords( 2, 1 );
                case BlockFace.Bottom:
                default:
                    return GetCoords( 1, 0 );
            }
        }
    }
}