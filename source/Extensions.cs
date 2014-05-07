using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob
{
    /// <summary>
    /// Internal class for extensions.
    /// </summary>
    internal static class Extensions
    {
        private static short[] BoundingBoxIndices;

        /// <summary>
        /// Static initialization of extensions.
        /// </summary>
        static Extensions()
        {
            BoundingBoxIndices = new short[] {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };
        }

#if DEBUG

        /// <summary>
        /// Draws the bounds of a bounding box.
        /// </summary>
        /// <param name="box">The bounding box to draw.</param>
        /// <param name="device">The graphics device to draw to.</param>
        public static void Draw( this BoundingBox box, GraphicsDevice device, BaseEffect effect )
        {
            Vector3[] corners = box.GetCorners();
            VertexPositionColorTexture[] primitives = new VertexPositionColorTexture[ corners.Length ];

            // copy over corner data
            for ( int i = 0; i < corners.Length; ++i )
            {
                primitives[ i ] = new VertexPositionColorTexture( corners[ i ], Color.White, Vector2.Zero );
            }

            // set world matrix and draw lines
            effect.World = Matrix.Identity;
            foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives( PrimitiveType.LineList, primitives, 0, 8, BoundingBoxIndices, 0, 12 );
            }
        }

#endif
    }
}