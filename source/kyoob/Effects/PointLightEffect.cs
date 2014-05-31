using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// A wrapper for a simple point light effect.
    /// </summary>
    public class PointLightEffect : LightedEffect
    {
        private Vector3 _lightColor;
        private Vector3 _lightPosition;
        private float _lightAttenuation;
        private float _lightFalloff;

        /// <summary>
        /// Gets or sets the light's color.
        /// </summary>
        public Vector3 LightColor
        {
            get
            {
                return _lightColor;
            }
            set
            {
                _lightColor = value;
                Effect.Parameters[ "_lightColor" ].SetValue( _lightColor );
            }
        }

        /// <summary>
        /// Gets or sets the light's position.
        /// </summary>
        public Vector3 LightPosition
        {
            get
            {
                return _lightPosition;
            }
            set
            {
                _lightPosition = value;
                Effect.Parameters[ "_lightPosition" ].SetValue( _lightPosition );
            }
        }

        /// <summary>
        /// Gets or sets the light's attenuation level.
        /// </summary>
        public float LightAttenuation
        {
            get
            {
                return _lightAttenuation;
            }
            set
            {
                _lightAttenuation = value;
                Effect.Parameters[ "_lightAttenuation" ].SetValue( _lightAttenuation );
            }
        }

        /// <summary>
        /// Gets or sets the light's falloff level.
        /// </summary>
        public float LightFalloff
        {
            get
            {
                return _lightFalloff;
            }
            set
            {
                _lightFalloff = value;
                Effect.Parameters[ "_lightFalloff" ].SetValue( _lightFalloff );
            }
        }

        /// <summary>
        /// Creates a new point light effect wrapper.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public PointLightEffect( Effect effect )
            : base( effect )
        {
            LightColor = new Vector3( 1.0f, 1.0f, 1.0f );
            LightPosition = new Vector3( 0.0f, 0.0f, 0.0f );
            LightAttenuation = 10.0f;
            LightFalloff = 4.0f;
        }
    }
}