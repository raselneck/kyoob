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
        private static ushort[] BoundingBoxIndexData;
        private static IndexBuffer BoundingBoxIndices;
        private static VertexBuffer BoundingBoxBuffer;

        /// <summary>
        /// Static initialization of extensions.
        /// </summary>
        static Extensions()
        {
            BoundingBoxBuffer = null;
            BoundingBoxIndices = null;
            BoundingBoxIndexData = new ushort[] {
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
        /// <param name="effect">The effect to draw with.</param>
        public static void Draw( this BoundingBox box, GraphicsDevice device, BaseEffect effect )
        {
            // create the buffers
            if ( BoundingBoxBuffer == null || BoundingBoxIndices == null )
            {
                BoundingBoxIndices = new IndexBuffer( device, IndexElementSize.SixteenBits, BoundingBoxIndexData.Length, BufferUsage.None );
                BoundingBoxIndices.SetData<ushort>( BoundingBoxIndexData );
                BoundingBoxBuffer = new VertexBuffer( device, VertexPositionNormalTexture.VertexDeclaration, 8, BufferUsage.None );
            }

            // create the buffer data
            Vector3[] corners = box.GetCorners();
            VertexPositionNormalTexture[] primitives = new VertexPositionNormalTexture[ corners.Length ];
            for ( int i = 0; i < corners.Length; ++i )
            {
                primitives[ i ] = new VertexPositionNormalTexture( corners[ i ], Vector3.Zero, Vector2.Zero );
            }
            BoundingBoxBuffer.SetData<VertexPositionNormalTexture>( primitives );

            // set world matrix and draw lines connecting corners
            effect.World = Matrix.Identity;
            device.Indices = BoundingBoxIndices;
            device.SetVertexBuffer( BoundingBoxBuffer );
            foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, 12 );
            }
            device.SetVertexBuffer( null );
        }

#endif
    }
}