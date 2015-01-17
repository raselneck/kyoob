using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The wrapper for block rendering.
    /// </summary>
    public sealed class BlockRenderEffect : CustomEffect
    {
        private Vector3 _fogColor;

        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Color FogColor
        {
            get
            {
                return new Color( _fogColor );
            }
            set
            {
                _fogColor = value.ToVector3();
            }
        }

        /// <summary>
        /// Gets the fog starting distance.
        /// </summary>
        public float FogStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the fog ending distance.
        /// </summary>
        public float FogEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new block render effect.
        /// </summary>
        public BlockRenderEffect()
            : base( Game.Instance.Content.Load<Effect>( "shaders/block_render" ) )
        {
            FogColor = Color.CornflowerBlue;
            FogEnd = Game.Instance.Settings.ViewDistance * 0.95f;
            FogStart = FogEnd * 0.875f;
        }

        /// <summary>
        /// Applies this effect's values to the underlying effect.
        /// </summary>
        public override void ApplyToEffect()
        {
            SetParameter( "spriteTexture", SpriteSheet.Instance.Texture );
            SetParameter( "playerPosition", Player.Instance.Position );
            SetParameter( "fogColor", _fogColor );
            SetParameter( "fogStart", FogStart );
            SetParameter( "fogEnd", FogEnd );
            base.ApplyToEffect();
        }
    }
}