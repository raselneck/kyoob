using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Blocks
{
    /// <summary>
    /// Contains block sprite sheet data.
    /// </summary>
    public sealed class SpriteSheet
    {
        /// <summary>
        /// The number of expected sprites in the X direction.
        /// </summary>
        private const int NumberOfSpritesX = 3;

        /// <summary>
        /// The number of expected sprites in the Y direction.
        /// </summary>
        private const int NumberOfSpritesY = 2;

        private Dictionary<BlockType, Vector2> _coords;
        private Texture2D _texture;

        /// <summary>
        /// Gets the texture.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
        }

        /// <summary>
        /// Gets the sprite sheet's total width.
        /// </summary>
        public int SheetWidth
        {
            get
            {
                return _texture.Width;
            }
        }

        /// <summary>
        /// Gets the sprite sheet's total height.
        /// </summary>
        public int SheetHeight
        {
            get
            {
                return _texture.Height;
            }
        }

        /// <summary>
        /// Gets the width of each sprite.
        /// </summary>
        public int SpriteWidth
        {
            get
            {
                return _texture.Width / NumberOfSpritesX;
            }
        }

        /// <summary>
        /// Gets the height of each sprite.
        /// </summary>
        public int SpriteHeight
        {
            get
            {
                return _texture.Height / NumberOfSpritesY;
            }
        }

        /// <summary>
        /// Gets the width in texture coordinates of each sprite.
        /// </summary>
        public float TexCoordWidth
        {
            get
            {
                return (float)SpriteWidth / SheetWidth;
            }
        }

        /// <summary>
        /// Gets the height in texture coordinates of each sprite.
        /// </summary>
        public float TexCoordHeight
        {
            get
            {
                return (float)SpriteHeight / SheetHeight;
            }
        }

        /// <summary>
        /// Creates a new sprite sheet.
        /// </summary>
        /// <param name="texture">The sprite sheet texture.</param>
        public SpriteSheet( Texture2D texture )
        {
            _texture = texture;

            _coords = new Dictionary<BlockType, Vector2>();
            _coords.Add( BlockType.Air, new Vector2( 0.0f, 0.0f ) );
            _coords.Add( BlockType.Dirt, new Vector2( TexCoordWidth, 0.0f ) );
            _coords.Add( BlockType.Stone, new Vector2( 0.0f, TexCoordHeight ) );
            _coords.Add( BlockType.Sand, new Vector2( TexCoordWidth, TexCoordHeight ) );
            _coords.Add( BlockType.Water, new Vector2( TexCoordWidth * 2, 0.0f ) );
        }

        /// <summary>
        /// Gets the texture coordinates of the given block type.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <returns></returns>
        public Vector2 GetTexCoords( BlockType type )
        {
            if ( _coords.ContainsKey( type ) )
            {
                return _coords[ type ];
            }
            return _coords[ BlockType.Air ]; // default to a transparent texture sprite
        }
    }
}