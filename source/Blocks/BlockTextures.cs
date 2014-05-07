using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Blocks
{
    /// <summary>
    /// Contains a set of block textures.
    /// </summary>
    public sealed class BlockTextures
    {
        private static BlockTextures _instance;
        private Dictionary<BlockType, Texture2D> _textures;

        /// <summary>
        /// Gets or sets the texture for the given block type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Texture2D this[ BlockType type ]
        {
            get
            {
                if ( _textures.ContainsKey( type ) )
                {
                    return _textures[ type ];
                }
                return null;
            }
            set
            {
                _textures[ type ] = value;
            }
        }

        /// <summary>
        /// Creates a new block texture set.
        /// </summary>
        private BlockTextures()
        {
            _textures = new Dictionary<BlockType, Texture2D>();
        }

        /// <summary>
        /// Gets the singleton block texture set instance.
        /// </summary>
        /// <returns></returns>
        public static BlockTextures GetInstance()
        {
            if ( _instance == null )
            {
                _instance = new BlockTextures();
            }
            return _instance;
        }
    }
}