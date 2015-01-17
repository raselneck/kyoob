using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;
using Kyoob.VoxelData;

// TODO : Make this a component?

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains an easy way to render a world.
    /// </summary>
    public sealed class WorldRenderer : IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        private BlockRenderEffect _blockRenderEffect;
        private World _world;

        /// <summary>
        /// Gets or sets the clear color to use.
        /// </summary>
        public Color ClearColor
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new world renderer.
        /// </summary>
        /// <param name="world">The world to render.</param>
        public WorldRenderer( World world )
        {
            _graphicsDevice = Game.Instance.GraphicsDevice;
            _graphicsDevice.SamplerStates[ 0 ] = SamplerState.PointClamp;
            _graphicsDevice.BlendState = BlendState.Opaque;

            _world = world;

            _blockRenderEffect = new BlockRenderEffect();

            ClearColor = Color.Gray;
        }

        /// <summary>
        /// Ensures this world renderer is disposed.
        /// </summary>
        ~WorldRenderer()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of this world renderer.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Updates the world renderer.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update( GameTime gameTime )
        {
            Profiler.Start( "Scene Update" );


            // update the world
            _world.Update( gameTime );

            // set effect parameters
            var camera = Player.Instance.Camera;
            _blockRenderEffect.View = camera.View;
            _blockRenderEffect.Projection = camera.Projection;
            _blockRenderEffect.FogColor = ClearColor;
            _blockRenderEffect.ApplyToEffect();


            Profiler.Stop( "Scene Update" );
        }

        /// <summary>
        /// Draws the world renderer.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw( GameTime gameTime )
        {
            Profiler.Start( "Scene Draw" );

            
            // clear buffer
            _graphicsDevice.Clear( ClearColor );

            // draw the world
            _blockRenderEffect.CurrentTechnique.Passes[ 0 ].Apply();
            _world.Draw( gameTime );


            Profiler.Stop( "Scene Draw" );
        }

        /// <summary>
        /// Disposes of this world renderer.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose( bool disposing )
        {
            _world.Dispose();
        }
    }
}