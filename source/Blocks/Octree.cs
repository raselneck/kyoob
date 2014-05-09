using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob.Blocks
{
    /// <summary>
    /// A generic octree implementation.
    /// </summary>
    /// <typeparam name="T">The type contained within the octree.</typeparam>
    public sealed class Octree<T>
        : IBoundable
          where T : IBoundable
    {
        /// <summary>
        /// The maximum number of elements we can have before we need to subdivide.
        /// </summary>
        private const int MaxItemCount = 64;

        private Vector3 _center;
        private Vector3 _dimensions;
        private List<T> _objects;
        private Octree<T>[] _children;
        private BoundingBox _bounds;

        /// <summary>
        /// Gets the octree's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Checks to see if this octree has divided.
        /// </summary>
        public bool HasDivided
        {
            get
            {
                return _children[ 0 ] != null;
            }
        }

        /// <summary>
        /// Creates a new octree.
        /// </summary>
        /// <param name="bounds">The octree's bounds.</param>
        public Octree( BoundingBox bounds )
        {
            _objects = new List<T>();
            _children = new Octree<T>[ 8 ];

            // set bounds and calculate dimensions and center
            _bounds = bounds;
            float width  = Math.Abs( _bounds.Max.X - _bounds.Min.X );
            float height = Math.Abs( _bounds.Max.Y - _bounds.Min.Y );
            float depth  = Math.Abs( _bounds.Max.Z - _bounds.Min.Z );
            _dimensions = new Vector3( width, height, depth );
            _center = new Vector3( _bounds.Min.X + width / 2, _bounds.Min.Y + height / 2, _bounds.Min.Z + depth / 2 );
        }

        /// <summary>
        /// Makes a bounding box from a center and dimensions.
        /// </summary>
        /// <param name="center">The center of the bounding box.</param>
        /// <param name="dim">The demensions of the bounding box.</param>
        private BoundingBox MakeBoundingBox( Vector3 center, Vector3 dim )
        {
            return new BoundingBox(
                new Vector3(
                    center.X - dim.X / 2,
                    center.Y - dim.Y / 2,
                    center.Z - dim.Z / 2
                ),
                new Vector3(
                    center.X + dim.X / 2,
                    center.Y + dim.Y / 2,
                    center.Z + dim.Z / 2
                )
            );
        }

        /// <summary>
        /// Divides this octree into its eight children.
        /// </summary>
        private void Divide()
        {
            // make sure we haven't divided already
            if ( HasDivided )
            {
                throw new InvalidOperationException( "This octree has already divided." );
            }

            // get half dimensions
            Vector3 hdim = new Vector3( _dimensions.X / 2, _dimensions.Y / 2, _dimensions.Z / 2 );

            // get child centers
            Vector3 tlb = new Vector3( _center.X - hdim.X, _center.Y + hdim.Y, _center.Z + hdim.Z );
            Vector3 tlf = new Vector3( _center.X - hdim.X, _center.Y + hdim.Y, _center.Z - hdim.Z );
            Vector3 trb = new Vector3( _center.X + hdim.X, _center.Y + hdim.Y, _center.Z + hdim.Z );
            Vector3 trf = new Vector3( _center.X + hdim.X, _center.Y + hdim.Y, _center.Z - hdim.Z );
            Vector3 blb = new Vector3( _center.X - hdim.X, _center.Y - hdim.Y, _center.Z + hdim.Z );
            Vector3 blf = new Vector3( _center.X - hdim.X, _center.Y - hdim.Y, _center.Z - hdim.Z );
            Vector3 brb = new Vector3( _center.X + hdim.X, _center.Y - hdim.Y, _center.Z + hdim.Z );
            Vector3 brf = new Vector3( _center.X + hdim.X, _center.Y - hdim.Y, _center.Z - hdim.Z );

            // create children
            _children[ 0 ] = new Octree<T>( MakeBoundingBox( tlb, hdim ) ); // top left back
            _children[ 1 ] = new Octree<T>( MakeBoundingBox( tlf, hdim ) ); // top left front
            _children[ 2 ] = new Octree<T>( MakeBoundingBox( trb, hdim ) ); // top right back
            _children[ 3 ] = new Octree<T>( MakeBoundingBox( trf, hdim ) ); // top right front
            _children[ 4 ] = new Octree<T>( MakeBoundingBox( blb, hdim ) ); // bottom left back
            _children[ 5 ] = new Octree<T>( MakeBoundingBox( blf, hdim ) ); // bottom left front
            _children[ 6 ] = new Octree<T>( MakeBoundingBox( brb, hdim ) ); // bottom right back
            _children[ 7 ] = new Octree<T>( MakeBoundingBox( brf, hdim ) ); // bottom right front

            // go through our items and try to move them into children
            for ( int i = 0; i < _objects.Count; ++i )
            {
                T obj = _objects[i];
                foreach ( Octree<T> child in _children )
                {
                    if ( child.Contains( obj.Bounds ) )
                    {
                        // move the object from this tree to the child
                        child.Add( obj );
                        _objects.RemoveAt( i );
                        --i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if this octree contains another bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public bool Contains( BoundingBox box )
        {
            ContainmentType type = _bounds.Contains( box );
            return type == ContainmentType.Contains
                ; // || type == ContainmentType.Intersects;
        }

        /// <summary>
        /// Adds an object to this octree.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <returns>True if the object was added, false if not.</returns>
        public bool Add( T obj )
        {
            // make sure we contain the object
            if ( !Contains( obj.Bounds ) ) // might need to change
            {
                return false;
            }

            // make sure we can add the object to our children
            if ( _objects.Count < MaxItemCount && !HasDivided )
            {
                _objects.Add( obj );
                return true;
            }
            else
            {
                // check if we need to divide
                if ( !HasDivided )
                {
                    Divide();
                }

                // try to get the child octree that contains the object
                Octree<T> tree = null;
                for ( int i = 0; i < 8; ++i )
                {
                    if ( _children[ i ].Contains( obj.Bounds ) )
                    {
                        tree = _children[ i ];
                        break;
                    }
                }

                // check if we need to add to the child or this
                if ( tree == null )
                {
                    // maybe don't ???
                    _objects.Add( obj );
                    return true;
                }
                else
                {
                    return tree.Add( obj );
                }
            }
        }

        /// <summary>
        /// Removes the given object from the octree.
        /// </summary>
        /// <param name="obj">The object.</param>
        public bool Remove( T obj )
        {
            // make sure we contain the object
            if ( !Contains( obj.Bounds ) )
            {
                return false;
            }

            // check if any children contain it first
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    if ( _children[ i ].Remove( obj ) )
                    {
                        return true;
                    }
                }
            }

            // now we need to check all of our objects
            int index = _objects.IndexOf( obj );
            if ( index == -1 )
            {
                return false;
            }
            _objects.RemoveAt( index );
            return true;
        }

        /// <summary>
        /// Clears this octree. Subdivisions, if any, will remain intact for speed.
        /// </summary>
        public void Clear()
        {
            _objects.Clear();
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    _children[ i ].Clear();
                }
            }
        }


        /// <summary>
        /// Draws the octree's bounds.
        /// </summary>
        /// <param name="device">The graphics device to draw to.</param>
        /// <param name="effect">The effect to draw with.</param>
        public void Draw( GraphicsDevice device, BaseEffect effect )
        {
            /*
            // draw bounds of octrees
            _bounds.Draw( device, effect );
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    _children[ i ].Draw( device, effect );
                }
            }
            */

            // draw bounds of all objects
            foreach ( T obj in _objects )
            {
                obj.Bounds.Draw( device, effect );
            }
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    _children[ i ].Draw( device, effect );
                }
            }
        }


    }
}