using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// The effect used for rendering the sky sphere.
    /// </summary>
    public class SkySphereEffect : BaseEffect
    {
        private TextureCube _cubeMap;

        /// <summary>
        /// Gets or sets the effect's cube map.
        /// </summary>
        public TextureCube CubeMap
        {
            get
            {
                return _cubeMap;
            }
            set
            {
                _cubeMap = value;
                Effect.Parameters[ "_cubeMap" ].SetValue( _cubeMap );
            }
        }

        /// <summary>
        /// Creates a new sky sphere effect wrapper.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public SkySphereEffect( Effect effect )
            : base( effect )
        {
        }
    }
}