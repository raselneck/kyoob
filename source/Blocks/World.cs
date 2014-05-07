using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

#warning TODO : Octree would REALLY help speed this up

namespace Kyoob.Blocks
{
    /// <summary>
    /// Creates a new world.
    /// </summary>
    public class World
    {
        private Stopwatch _watch;
        private GraphicsDevice _device;
        private BaseEffect _effect;
        private List<Chunk> _chunks;

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="effect">The base effect.</param>
        public World( GraphicsDevice device, BaseEffect effect )
        {
            _watch = new Stopwatch();
            _device = device;
            _effect = effect;

            // add some arbitrary chunks
            _chunks = new List<Chunk>();
            for ( int x = -2; x <= 2; ++x )
            {
                for ( int z = -2; z <= 2; ++z )
                {
                    _chunks.Add( new Chunk( _device, new Vector3( x * 8.0f, 0.0f, z * 8.0f ) ) );
                }
            }
        }

        /// <summary>
        /// Draws only the visible blocks.
        /// </summary>
        /// <param name="camera">The camera to use for position testing.</param>
        private int DrawVisibleBlocks( Camera camera )
        {
            // TODO : Use octree to query "visibility sphere" instead of all chunks

            int count = 0;
            Vector3 forward = Vector3.Normalize( Vector3.Transform( Vector3.Forward, camera.Rotation ) );
            foreach ( Chunk chunk in _chunks )
            {
                // skip the chunk if we can't see it
                if ( !camera.CanSee( chunk.Bounds ) )
                {
                    continue;
                }

                foreach ( Block block in chunk.GetExposedBlocks() )
                {
                    // get cos(THETA) of block position and direction we're facing
                    Vector3 heading = Vector3.Normalize( block.Position - camera.Position );
                    float dot = Vector3.Dot( heading, forward );

                    // if cos(THETA) >= 7/16 (somewhat arbitrary value), draw the block
                    // basically if the block is in front of us
                    if ( dot >= 0.4375f )
                    {
                        ++count;
                        block.Draw( _device, _effect );
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Draws the world.
        /// </summary>
        /// <param name="camera">The current camera to use for getting visible tiles.</param>
        public void Draw( Camera camera )
        {
            _watch = new Stopwatch();
            _watch.Start();


            int count = DrawVisibleBlocks( camera );


            _watch.Stop();
            Console.WriteLine( "Draw {0} blocks: {1:0.00}ms", count, _watch.Elapsed.TotalMilliseconds );
        }
    }
}