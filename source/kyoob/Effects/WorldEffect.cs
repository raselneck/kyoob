using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// The main world effect.
    /// </summary>
    public class WorldEffect : PointLightEffect
    {
        // as of right now this is just here in case I add anything later

        /// <summary>
        /// Creates a new world effect.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public WorldEffect( Effect effect )
            : base( effect )
        {
        }
    }
}