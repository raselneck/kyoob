using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains a way to save and restore the state of the graphics device.
    /// </summary>
    public sealed class GraphicsState
    {
        private static Stack<GraphicsState> _states;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private SamplerState _samplerState;

        /// <summary>
        /// Gets the blend state.
        /// </summary>
        public BlendState BlendState
        {
            get
            {
                return _blendState;
            }
            private set
            {
                if ( value != null )
                {
                    _blendState = value;
                }
            }
        }

        /// <summary>
        /// Gets the depth-stencil state.
        /// </summary>
        public DepthStencilState DepthStencilState
        {
            get
            {
                return _depthStencilState;
            }
            private set
            {
                if ( value != null )
                {
                    _depthStencilState = value;
                }
            }
        }

        /// <summary>
        /// Gets the sampler state.
        /// </summary>
        public SamplerState SamplerState
        {
            get
            {
                return _samplerState;
            }
            private set
            {
                if ( value != null )
                {
                    _samplerState = value;
                }
            }
        }

        /// <summary>
        /// Statically initializes the graphics state interface.
        /// </summary>
        static GraphicsState()
        {
            _states = new Stack<GraphicsState>();
        }

        /// <summary>
        /// Pushes the current graphics state to an internal stack.
        /// </summary>
        public static void Push()
        {
            _states.Push( new GraphicsState() );
        }

        /// <summary>
        /// Removes the state from the top of the stack and restores it.
        /// </summary>
        public static void Pop()
        {
            if ( _states.Count > 0 )
            {
                var state = _states.Pop();
                state.Restore();
            }
        }

        /// <summary>
        /// Peeks at the last pushed state.
        /// </summary>
        /// <returns></returns>
        public static GraphicsState Peek()
        {
            return _states.Peek();
        }

        /// <summary>
        /// Creates a graphics state representing the current state of the graphics device.
        /// </summary>
        private GraphicsState()
        {
            var device = Game.Instance.GraphicsDevice;
            BlendState = device.BlendState ?? BlendState.Opaque;
            DepthStencilState = device.DepthStencilState ?? DepthStencilState.Default;
            SamplerState = device.SamplerStates[ 0 ] ?? SamplerState.PointClamp;
        }

        /// <summary>
        /// Restores this state's information to the graphics device.
        /// </summary>
        private void Restore()
        {
            var device = Game.Instance.GraphicsDevice;
            device.BlendState = BlendState;
            device.DepthStencilState = DepthStencilState;
            device.SamplerStates[ 0 ] = SamplerState;
        }
    }
}