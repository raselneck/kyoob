using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// The alpha blending effect.
    /// </summary>
    public class AlphaEffect : BaseEffect
    {
        private RenderTarget2D _target;

        /// <summary>
        /// Gets the effect's render target.
        /// </summary>
        public RenderTarget2D RenderTarget
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
                Effect.Parameters[ "_alphaTarget" ].SetValue( _target );
            }
        }

        /// <summary>
        /// Creates a new alpha blending effect.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public AlphaEffect( Effect effect )
            : base( effect )
        {
            PresentationParameters pp = effect.GraphicsDevice.PresentationParameters;
            RenderTarget = new RenderTarget2D( effect.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight );
        }
    }
}