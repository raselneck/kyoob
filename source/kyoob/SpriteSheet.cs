using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Blocks;

namespace Kyoob
{
    /// <summary>
    /// Contains block sprite sheet data.
    /// </summary>
    public sealed class SpriteSheet
    {
        /// <summary>
        /// The number of expected sprites in the X direction.
        /// </summary>
        private const int NumberOfSpritesX = 4;

        /// <summary>
        /// The number of expected sprites in the Y direction.
        /// </summary>
        private const int NumberOfSpritesY = 4;

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
        }

        /// <summary>
        /// Gets the texture coordinates for the sprite at the given X and Y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns></returns>
        private Vector2 GetCoords( int x, int y )
        {
            return new Vector2( x * TexCoordWidth, y * TexCoordHeight );
        }

        /// <summary>
        /// Gets the texture coordinates for a grass block on the given face.
        /// </summary>
        /// <param name="face">The block face.</param>
        /// <returns></returns>
        private Vector2 GetGrassCoords( CubeFace face )
        {
            switch ( face )
            {
                case CubeFace.Left:
                case CubeFace.Right:
                case CubeFace.Front:
                case CubeFace.Back:
                    return GetCoords( 2, 2 );
                case CubeFace.Top:
                    return GetCoords( 2, 1 );
                case CubeFace.Bottom:
                default:
                    return GetCoords( 1, 0 );
            }
        }

        /// <summary>
        /// Gets the texture coordinates of the given block type.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <param name="face">The cube face.</param>
        public Vector2 GetTexCoords( BlockType type, CubeFace face )
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
    }
}