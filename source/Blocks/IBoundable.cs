using System;
using Microsoft.Xna.Framework;

namespace Kyoob.Blocks
{
    /// <summary>
    /// An interface for boundable objects.
    /// </summary>
    public interface IBoundable
    {
        /// <summary>
        /// Gets the object's bounds.
        /// </summary>
        BoundingBox Bounds { get; }
    }
}