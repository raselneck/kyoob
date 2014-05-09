using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// Base class for any effect that uses lights.
    /// </summary>
    public abstract class LightedEffect : TextureEffect
    {
        private Vector3 _ambientColor;
        private Vector3 _diffuseColor;

        /// <summary>
        /// Gets or sets the world ambient color.
        /// </summary>
        public Vector3 AmbientColor
        {
            get
            {
                return _ambientColor;
            }
            set
            {
                _ambientColor = value;
                Effect.Parameters[ "_ambientColor" ].SetValue( _ambientColor );
            }
        }

        /// <summary>
        /// Gets or sets the world diffuse color.
        /// </summary>
        public Vector3 DiffuseColor
        {
            get
            {
                return _diffuseColor;
            }
            set
            {
                _diffuseColor = value;
                Effect.Parameters[ "_diffuseColor" ].SetValue( _diffuseColor );
            }
        }

        /// <summary>
        /// Creates a new lighted effect wrapper.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public LightedEffect( Effect effect )
            : base( effect )
        {
            AmbientColor = new Vector3( 0.1f, 0.1f, 0.1f );
            DiffuseColor = new Vector3( 0.85f, 0.85f, 0.85f );
        }
    }
}