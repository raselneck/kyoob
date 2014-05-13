using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Blocks;

namespace Kyoob.Effects
{
    /// <summary>
    /// A simple effect renderer / manager.
    /// </summary>
    public class EffectRenderer
    {
        private GraphicsDevice _device;
        private BaseEffect _effect;
        private Camera _camera;
        private List<VoxelBuffer> _solidQueue;
        private List<VoxelBuffer> _alphaQueue;
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
        /// <param name="effect">The effect to render with.</param>
        /// <param name="camera">The camera to use.</param>
        public EffectRenderer( GraphicsDevice device, BaseEffect effect, Camera camera )
        {
            _device = device;
            _effect = effect;
            _camera = camera;
            _solidQueue = new List<VoxelBuffer>();
            _alphaQueue = new List<VoxelBuffer>();
            _clearColor = Color.CornflowerBlue;
        }

        /// <summary>
        /// Queues a voxel buffer to be drawn with the main rendering effect.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void QueueSolid( VoxelBuffer buffer )
        {
            _solidQueue.Add( buffer );
        }

        /// <summary>
        /// Queues a voxel buffer to be drawn with the alpha blending effect.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void QueueAlpha( VoxelBuffer buffer )
        {
            _alphaQueue.Add( buffer );
        }

        /// <summary>
        /// Renders everything to the screen.
        /// </summary>
        public void Render()
        {
            // draw solid shit
            _effect.SetTechnique( "MainTechnique" );
            for ( int i = 0; i < _solidQueue.Count; ++i )
            {
                VoxelBuffer buffer = _solidQueue[ i ];
                lock ( buffer )
                {
                    buffer.Draw( _device, _effect );
                }
            }
            _solidQueue.Clear();

            // draw transparent shit
            _effect.SetTechnique( "AlphaTechnique" );
            for ( int i = 0; i < _alphaQueue.Count; ++i )
            {
                VoxelBuffer buffer = _alphaQueue[ i ];
                lock ( buffer )
                {
                    buffer.Draw( _device, _effect );
                }
            }
            _alphaQueue.Clear();
        }
    }
}