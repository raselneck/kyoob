using System;
using Microsoft.Xna.Framework;

namespace Kyoob.Effects
{
    /// <summary>
    /// The interface to be implemented by all effects that contain fog.
    /// </summary>
    public interface IFogEffect
    {
        /// <summary>
        /// Gets or sets the fog starting distance.
        /// </summary>
        float FogStart { get; set; }

        /// <summary>
        /// Gets or sets the fog ending distance.
        /// </summary>
        float FogEnd { get; set; }

        /// <summary>
        /// Gets or sets the fog's color.
        /// </summary>
        Vector3 FogColor { get; set; }
    }
}