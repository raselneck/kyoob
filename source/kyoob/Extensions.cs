using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob
{
    /// <summary>
    /// Internal class for generics collections extensions.
    /// </summary>
    internal static class GenericsExtensions
    {
        /// <summary>
        /// Merges the given dictionary into this dictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="that">This dictionary.</param>
        /// <param name="dict">The dictionary to merge in.</param>
        public static void Merge<TKey, TValue>( this Dictionary<TKey, TValue> that, Dictionary<TKey, TValue> dict )
        {
            foreach ( var pair in dict )
            {
                that.Add( pair.Key, pair.Value );
            }
        }
    }

    internal static class RayExtensions
    {
#if DEBUG
        private static BasicEffect _effect;

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

            // set the effect parameters and draw the box
            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = proj;
            foreach ( EffectPass pass in _effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserPrimitives( PrimitiveType.LineList, points, 0, 1 );
            }
        }

#endif
    }

    /// <summary>
    /// Internal class for bounding box extensions.
    /// </summary>
    internal static class BoundingBoxExtensions
    {
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

        /// <summary>
        /// Gets the bounding box's width.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns></returns>
        public static float GetWidth( this BoundingBox box )
        {
            return Math.Abs( box.Max.X - box.Min.X );
        }

        /// <summary>
        /// Gets the bounding box's height.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns></returns>
        public static float GetHeight( this BoundingBox box )
        {
            return Math.Abs( box.Max.Y - box.Min.Y );
        }

        /// <summary>
        /// Gets the bounding box's depth.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns></returns>
        public static float GetDepth( this BoundingBox box )
        {
            return Math.Abs( box.Max.Z - box.Min.Z );
        }

        /// <summary>
        /// Gets the amount that this bounding box overlaps another.
        /// </summary>
        /// <param name="bb">The bounding box.</param>
        /// <param name="box">The bounding box to check for overlap.</param>
        /// <param name="overlap">The overlap vector to populate. (-1.0f values for no overlap.)</param>
        /// <returns></returns>
        public static bool GetOverlap( this BoundingBox bb, BoundingBox box, out Vector3 overlap )
        {
            // code adapted from http://jsfiddle.net/uthyZ/ which is an answer to
            // http://math.stackexchange.com/questions/99565/simplest-way-to-calculate-the-intersect-area-of-two-rectangles

            Vector3 calc = new Vector3(
                Math.Min( bb.Max.X, box.Max.X ) - Math.Max( bb.Min.X, box.Min.X ),
                Math.Min( bb.Max.Y, box.Max.Y ) - Math.Max( bb.Min.Y, box.Min.Y ),
                Math.Min( bb.Max.Z, box.Max.Z ) - Math.Max( bb.Min.Z, box.Min.Z )
            );

            overlap.X = ( calc.X < 0.0f ) ? -1.0f : calc.X;
            overlap.Y = ( calc.Y < 0.0f ) ? -1.0f : calc.Y;
            overlap.Z = ( calc.Z < 0.0f ) ? -1.0f : calc.Z;

            return overlap != -Vector3.One;
        }

#if DEBUG
        private static BasicEffect _effect;

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