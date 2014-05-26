using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob
{
    /// <summary>
    /// Internal class for extensions.
    /// </summary>
    internal static class Extensions
    {
        private static BasicEffect _effect;

        /// <summary>
        /// Static initialization of extensions.
        /// </summary>
        static Extensions()
        {
        }

        /// <summary>
        /// Gets the center coordinates of this bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public static Vector3 GetCenter( this BoundingBox box )
        {
            float width = box.Max.X - box.Min.X;
            float height = box.Max.Y - box.Min.Y;
            float depth = box.Max.Z - box.Min.Z;
            return new Vector3(
                box.Min.X + width / 2.0f,
                box.Min.Y + height / 2.0f,
                box.Min.Z + depth / 2.0f
            );
        }

#if DEBUG

        /// <summary>
        /// Draws the ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="device">The graphics device to draw to.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="proj">The current projection matrix.</param>
        /// <param name="color">The color to draw with.</param>
        public static void Draw( this Ray ray, GraphicsDevice device, Matrix view, Matrix proj, Color color )
        {
            // create the basic effect if we need to
            if ( _effect == null )
            {
                _effect = new BasicEffect( device );
                _effect.TextureEnabled = false;
                _effect.VertexColorEnabled = true;
            }

            VertexPositionColor[] points = {
                new VertexPositionColor( ray.Position, color ),
                new VertexPositionColor( ray.Position + ray.Direction, color )
            };
            short[] indices = { 0, 1, 1, 0 };

            // set the effect parameters and draw the box
            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = proj;
            foreach ( EffectPass pass in _effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives( PrimitiveType.LineList, points, 0, 4, indices, 0, 2 );
            }
        }

        /// <summary>
        /// Draws the bounds of a bounding box.
        /// </summary>
        /// <param name="box">The bounding box to draw.</param>
        /// <param name="device">The graphics device to draw to.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="proj">The current projection matrix.</param>
        /// <param name="color">The color to draw with.</param>
        public static void Draw( this BoundingBox box, GraphicsDevice device, Matrix view, Matrix proj, Color color )
        {
            // create the basic effect if we need to
            if ( _effect == null )
            {
                _effect = new BasicEffect( device );
                _effect.TextureEnabled = false;
                _effect.VertexColorEnabled = true;
            }

            short[] indices = {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };

            Vector3[] corners = box.GetCorners();
            VertexPositionColor[] primitiveList = new VertexPositionColor[ corners.Length ];

            // assign the 8 box vertices
            for ( int i = 0; i < corners.Length; i++ )
            {
                primitiveList[ i ] = new VertexPositionColor( corners[ i ], color );
            }

            // set the effect parameters and draw the box
            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = proj;
            foreach ( EffectPass pass in _effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.LineList, primitiveList, 0, 8,
                    indices, 0, 12 );
            }
        }

#endif
    }
}