using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains point light information.
    /// </summary>
    public sealed class PointLight : Light
    {
        private string _positionName;
        private string _attenName;
        private string _falloffName;

        /// <summary>
        /// Gets or sets this light's attenuation.
        /// </summary>
        public float Attenuation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this light's falloff.
        /// </summary>
        public float Falloff
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this light's position.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new point light.
        /// </summary>
        /// <param name="positionName">The name of the variable for this light's position.</param>
        /// <param name="colorName">The name of the variable for this light's color.</param>
        /// <param name="attenName">The name of the variable for this light's attenuation.</param>
        /// <param name="falloffName">The name of the variable for this light's falloff.</param>
        public PointLight( string positionName, string colorName, string attenName, string falloffName )
            : base( colorName )
        {
            _positionName = positionName;
            _attenName = attenName;
            _falloffName = falloffName;

            Attenuation = 32.0f;
            Falloff = 2.0f;
            Position = Vector3.Zero;
        }

        /// <summary>
        /// Applies this point light to the given effect.
        /// </summary>
        /// <param name="effect">The effect.</param>
        public override void Apply( CustomEffect effect )
        {
            effect.SetParameter( _attenName, Attenuation );
            effect.SetParameter( _falloffName, Falloff );
            effect.SetParameter( _positionName, Position );

            base.Apply( effect );
        }
    }
}