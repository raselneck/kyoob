using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

/**
 * I separated the Block and Cube classes in case I ever
 * needed to use the Cube in some way that wasn't a block.
 * 
 * Later, blocks will have their own types.
 */

namespace Kyoob.Blocks
{
    /// <summary>
    /// A class containing block data.
    /// </summary>
    public sealed class Block
    {
        private Cube _cube;
        private BlockType _type;

        /// <summary>
        /// Gets this block's position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _cube.Position;
            }
        }

        /// <summary>
        /// Gets this block's bounding box.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _cube.Bounds;
            }
        }

        /// <summary>
        /// Gets this cube's world matrix.
        /// </summary>
        public Matrix World
        {
            get
            {
                return _cube.World;
            }
        }

        /// <summary>
        /// Checks to see if this block is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _type == BlockType.Air;
            }
        }

        /// <summary>
        /// Creates a new block.
        /// </summary>
        /// <param name="device">The graphics device this block belongs on.</param>
        /// <param name="position">The block's position.</param>
        public Block( GraphicsDevice device, Vector3 position, BlockType type )
        {
            _cube = new Cube( device, position );
            _type = type;
        }

        /// <summary>
        /// Draws this block.
        /// </summary>
        /// <param name="device">The device to draw to.</param>
        /// <param name="effect">The effect to use to draw.</param>
        public void Draw( GraphicsDevice device, BaseEffect effect )
        {
            // no need to draw if the block is empty
            if ( IsEmpty )
            {
                return;
            }

            ( (TextureEffect)effect ).Texture = BlockTextures.GetInstance()[ _type ];
            _cube.Draw( device, effect );
        }
    }
}