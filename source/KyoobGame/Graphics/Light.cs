using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The base class for all lights.
    /// </summary>
    public abstract class Light
    {
        protected string _colorName;

        /// <summary>
        /// Gets or sets the color of this light.
        /// </summary>
        public Color Color
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new light.
        /// </summary>
        /// <param name="effect">The custom effect this light is associated with.</param>
        /// <param name="colorName">The name of the variable for this light's color.</param>
        public Light( string colorName )
        {
            _colorName = colorName;
            Color = Color.White;
        }

        /// <summary>
        /// Applies this light to the given effect.
        /// </summary>
        /// <param name="effect">The effect.</param>
        public virtual void Apply( CustomEffect effect )
        {
            effect.SetParameter( _colorName, Color.ToVector3() );
        }
    }
}