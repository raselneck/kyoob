using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;

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
        /// Gets or sets the fog's starting distance.
        /// </summary>
        public float FogStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fog's ending distance.
        /// </summary>
        public float FogEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fog's color.
        /// </summary>
        public Color FogColor
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

            FogColor = Color.CornflowerBlue;
            SetDefaultFogRange();
        }

        /// <summary>
        /// Sets the default fog range.
        /// </summary>
        /// <param name="viewDistance">The view distance to use.</param>
        public void SetDefaultFogRange()
        {
            FogEnd = Settings.Instance.ViewDistance * 0.95f;
            FogStart = FogEnd * 0.875f;
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
            SetParameter( "playerPosition", Player.Instance.EyePosition );
            SetParameter( "fogStart", FogStart );
            SetParameter( "fogEnd", FogEnd );
            SetParameter( "fogColor", FogColor.ToVector3() );

            base.ApplyToEffect();
        }
    }
}