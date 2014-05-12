using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Blocks;

#warning TODO : This doesn't work at all. And it's going to be super fucking disorganized.

namespace Kyoob.Effects
{
    /// <summary>
    /// A simple effect renderer / manager.
    /// </summary>
    public class EffectRenderer
    {
        private GraphicsDevice _device;
        private BaseEffect _mainEffect;
        private AlphaEffect _blendEffect;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Queue<VoxelBuffer> _solidQueue;
        private Queue<VoxelBuffer> _alphaQueue;
        private Color _clearColor;

        /// <summary>
        /// Gets the graphics device this renderer uses.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
        }

        /// <summary>
        /// Gets or sets the renderer's clear color.
        /// </summary>
        public Color ClearColor
        {
            get
            {
                return _clearColor;
            }
            set
            {
                _clearColor = value;
            }
        }

        /// <summary>
        /// Creates a new effect renderer.
        /// </summary>
        /// <param name="device">The graphics device to render to.</param>
        /// <param name="main">The main rendering effect to use.</param>
        /// <param name="blend">The alpha blending effect to use.</param>
        public EffectRenderer( GraphicsDevice device, BaseEffect main, AlphaEffect blend )
        {
            _device = device;
            _mainEffect = main;
            _blendEffect = blend;
            _solidQueue = new Queue<VoxelBuffer>();
            _alphaQueue = new Queue<VoxelBuffer>();
            _spriteBatch = new SpriteBatch( _device );

            // set our render target to mimic the back buffer
            PresentationParameters pp = device.PresentationParameters;
            _renderTarget = new RenderTarget2D(
                _device, pp.BackBufferWidth, pp.BackBufferHeight,
                false, _device.DisplayMode.Format,
                pp.DepthStencilFormat, pp.MultiSampleCount, pp.RenderTargetUsage
            );

            _clearColor = Color.CornflowerBlue;
        }

        /// <summary>
        /// Queues a voxel buffer to be drawn with the main rendering effect.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void QueueSolid( VoxelBuffer buffer )
        {
            _solidQueue.Enqueue( buffer );
        }

        /// <summary>
        /// Queues a voxel buffer to be drawn with the alpha blending effect.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void QueueAlpha( VoxelBuffer buffer )
        {
            _alphaQueue.Enqueue( buffer );
        }

        /// <summary>
        /// Renders everything to the screen.
        /// </summary>
        /// <param name="skySphere">The sky sphere to draw.</param>
        public void Render()
        {
            while ( _solidQueue.Count > 0 )
            {
                VoxelBuffer buff = _solidQueue.Dequeue();
                buff.Draw( _device, _mainEffect );
            }
        }
    }
}