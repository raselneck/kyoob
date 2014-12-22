using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The custom effect wrapper for the depth-normal effect.
    /// </summary>
    public sealed class DepthNormalEffect : CustomEffect
    {
        /// <summary>
        /// Creates a new depth-normal effect.
        /// </summary>
        public DepthNormalEffect()
            : base( Game.Instance.Content.Load<Effect>( "shaders/depth_normal" ) )
        {
            World = Matrix.Identity;
        }
    }
}