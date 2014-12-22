using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The base class for lighted effects.
    /// </summary>
    public abstract class LightedEffect : CustomEffect
    {
        /// <summary>
        /// Gets or sets this effect's ambient color.
        /// </summary>
        public Color AmbientColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this effect's diffuse color.
        /// </summary>
        public Color DiffuseColor
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new lighted effect.
        /// </summary>
        /// <param name="effect">The XNA effect to use.</param>
        public LightedEffect( Effect effect )
            : base( effect )
        {
            AmbientColor = new Color( new Vector3( 0.05f ) );
            DiffuseColor = new Color( Vector3.One ); // Color.White...
        }

        /// <summary>
        /// Applies parameters to the underlying XNA effect.
        /// </summary>
        public override void ApplyToEffect()
        {
            SetParameter( "ambientColor", AmbientColor.ToVector3() );
            SetParameter( "diffuseColor", DiffuseColor.ToVector3() );

            base.ApplyToEffect();
        }
    }
}