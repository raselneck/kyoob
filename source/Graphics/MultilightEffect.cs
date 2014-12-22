using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The custom effect wrapper for the multilight effect.
    /// </summary>
    public sealed class MultilightEffect : LightedEffect
    {
        /// <summary>
        /// Gets or sets the light map to use.
        /// </summary>
        public Texture2D LightMap
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new multilight effect.
        /// </summary>
        /// <param name="lightMap">The light map to use.</param>
        /// <param name="shadowMap">The shadow map to use.</param>
        public MultilightEffect( Texture2D lightMap )
            : base( Game.Instance.Content.Load<Effect>( "shaders/multilight" ) )
        {
            LightMap = lightMap;
            World = Matrix.Identity;
        }

        /// <summary>
        /// Applies parameters to the underlying XNA effect.
        /// </summary>
        public override void ApplyToEffect()
        {
            var viewport = Game.Instance.GraphicsDevice.Viewport;
            SetParameter( "viewportHeight", (float)viewport.Height );
            SetParameter( "viewportWidth", (float)viewport.Width );
            SetParameter( "spriteTexture", SpriteSheet.Instance.Texture );
            SetParameter( "lightTexture", LightMap );


            base.ApplyToEffect();
        }
    }
}