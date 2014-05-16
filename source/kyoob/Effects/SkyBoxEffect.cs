using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// The effect used for rendering the sky box.
    /// </summary>
    public class SkyBoxEffect : BaseEffect
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
        /// Creates a new sky box effect wrapper.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public SkyBoxEffect( Effect effect )
            : base( effect )
        {
        }
    }
}