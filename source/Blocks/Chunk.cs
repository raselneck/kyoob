using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#warning TODO : Add chunk interoperability for Chunk.IsEmpty(int,int,int)

namespace Kyoob.Blocks
{
    /// <summary>
    /// A data structure containing a chunk of blocks.
    /// </summary>
    public sealed class Chunk
    {
        private LibNoise.Perlin _noise;
        private Block[ , , ] _blocks;
        private List<Block> _visibleBlocks;
        private Vector3 _position;
        private BoundingBox _bounds;

        /// <summary>
        /// Gets this chunk's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="device">The graphics device to be created on.</param>
        /// <param name="position">The chunk's position (the center of the chunk).</param>
        public Chunk( GraphicsDevice device, Vector3 position )
        {
            _noise = new LibNoise.Perlin();
            _blocks = new Block[ 16, 16, 16 ];
            _position = position;

            // set our bounding box (for some reason this one doesn't work)
            //_bounds = new BoundingBox(
            //    new Vector3( position.X - 8.5f, position.Y - 8.5f, position.Z - 8.5f ),
            //    new Vector3( position.X + 8.5f, position.Y + 8.5f, position.Z + 8.5f )
            //);

            for ( int x = 0, xx = (int)position.X - 8; x < 16; ++x, ++xx )
            {
                for ( int y = 0, yy = (int)position.Y - 8; y < 16; ++y, ++yy )
                {
                    for ( int z = 0, zz = (int)position.Z - 8; z < 16; ++z, ++zz )
                    {
                        // create the block
                        _blocks[ x, y, z ] = new Block( device, new Vector3(
                            _position.X + xx,
                            _position.Y + yy,
                            _position.Z + zz
                        ), GetBlockType( xx, yy, zz ) );

                        // add to our bounding box (or create it)
                        if ( _bounds == null )
                        {
                            _bounds = _blocks[ 0, 0, 0 ].Bounds;
                        }
                        else
                        {
                            _bounds = BoundingBox.CreateMerged( _bounds, _blocks[ x, y, z ].Bounds );
                        }
                    }
                }
            }

            BuildVisibleBlockList();
        }

        /// <summary>
        /// Builds the visible block list.
        /// </summary>
        private void BuildVisibleBlockList()
        {
            _visibleBlocks = new List<Block>();
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        // if this block is empty, then exit immediately
                        if ( IsEmpty( x, y, z ) )
                        {
                            continue;
                        }

                        // check if there are blocks in certain directions
                        bool above = !IsEmpty( x, y - 1, z );
                        bool below = !IsEmpty( x, y + 1, z );
                        bool left  = !IsEmpty( x - 1, y, z );
                        bool right = !IsEmpty( x + 1, y, z );
                        bool front = !IsEmpty( x, y, z - 1 );
                        bool back  = !IsEmpty( x, y, z + 1 );

                        // if we're surrounded by blocks, then this one isn't visible
                        if ( above && below && left && right && front && back )
                        {
                            continue;
                        }

                        _visibleBlocks.Add( _blocks[ x, y, z ] );
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if a block is empty.
        /// </summary>
        /// <param name="x">The X index of the block to check.</param>
        /// <param name="y">The Y index of the block to check.</param>
        /// <param name="z">The Z index of the block to check.</param>
        private bool IsEmpty( int x, int y, int z )
        {
            // make sure the bounds are legit for this chunk
            if ( ( x < 0 || x > 15 ) ||
                 ( y < 0 || y > 15 ) ||
                 ( z < 0 || z > 15 ) )
            {
                return GetBlockType( x, y, z ) == BlockType.Air;
            }

            return _blocks[ x, y, z ].IsEmpty;
        }

        /// <summary>
        /// Gets a block type.
        /// </summary>
        /// <param name="x">The global X coordinate.</param>
        /// <param name="y">The global Y coordinate.</param>
        /// <param name="z">The global Z coordinate.</param>
        /// <returns></returns>
        private BlockType GetBlockType( int x, int y, int z )
        {
            double value = _noise.GetValue( ( x - 8 ) / 16.0, ( y - 8 ) / 16.0, ( z - 8 ) / 16.0 );
            
            // just some arbitrary values
            BlockType type = BlockType.Air;
            if ( Math.Abs( value ) >= 0.4 )
            {
                type = BlockType.Dirt;
            }
            else if ( Math.Abs( value ) >= 0.15 && Math.Abs( value ) < 0.4 )
            {
                type = BlockType.Stone;
            }

            return type;
        }

        /// <summary>
        /// Draws this chunk.
        /// </summary>
        /// <param name="device">The device to draw to.</param>
        /// <param name="effect">The effect to use to draw.</param>
        /// <param name="camera">The camera to use for culling.</param>
        public void Draw( GraphicsDevice device, BaseEffect effect, Camera camera )
        {
            // don't even attempt to draw this if the camera can't see it
            if ( !camera.CanSee( _bounds ) )
            {
                return;
            }

            // now draw each block only if it's visible
            /*
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    for ( int z = 0; z < 16; ++z )
                    {
                        //if ( camera.CanSee( _blocks[ x, y, z ].Bounds ) )
                        //{
                        //}
                        _blocks[ x, y, z ].Draw( device, effect );
                    }
                }
            }
            */
            foreach ( Block block in _visibleBlocks )
            {
                block.Draw( device, effect );
            }
        }
    }
}