using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        private const int MaxItemCount = 48;

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
            _objects  = new List<T>( MaxItemCount );
            _children = new Octree<T>[ 8 ];

            // set bounds and calculate dimensions and center
            _bounds = bounds;
            float width  = Math.Abs( _bounds.Max.X - _bounds.Min.X );
            float height = Math.Abs( _bounds.Max.Y - _bounds.Min.Y );
            float depth  = Math.Abs( _bounds.Max.Z - _bounds.Min.Z );
            _dimensions  = new Vector3(
                width,
                height,
                depth
            );
            _center      = new Vector3(
                _bounds.Min.X + width / 2,
                _bounds.Min.Y + height / 2,
                _bounds.Min.Z + depth / 2
            );
        }

        /// <summary>
        /// Makes a bounding box from a center and dimensions.
        /// </summary>
        /// <param name="center">The center of the bounding box.</param>
        /// <param name="dim">The demensions of the bounding box.</param>
        private BoundingBox MakeBoundingBox( Vector3 center, Vector3 dim )
        {
            return new BoundingBox(
                center - dim / 2.0f,
                center + dim / 2.0f              
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
            Vector3 hdim = _dimensions / 2.0f;

            // get child centers
            Vector3 trb = new Vector3( _center.X + hdim.X / 2.0f, _center.Y + hdim.Y / 2.0f, _center.Z + hdim.Z / 2.0f );
            Vector3 trf = new Vector3( _center.X + hdim.X / 2.0f, _center.Y + hdim.Y / 2.0f, _center.Z - hdim.Z / 2.0f );
            Vector3 brb = new Vector3( _center.X + hdim.X / 2.0f, _center.Y - hdim.Y / 2.0f, _center.Z + hdim.Z / 2.0f );
            Vector3 brf = new Vector3( _center.X + hdim.X / 2.0f, _center.Y - hdim.Y / 2.0f, _center.Z - hdim.Z / 2.0f );
            Vector3 tlb = new Vector3( _center.X - hdim.X / 2.0f, _center.Y + hdim.Y / 2.0f, _center.Z + hdim.Z / 2.0f );
            Vector3 tlf = new Vector3( _center.X - hdim.X / 2.0f, _center.Y + hdim.Y / 2.0f, _center.Z - hdim.Z / 2.0f );
            Vector3 blb = new Vector3( _center.X - hdim.X / 2.0f, _center.Y - hdim.Y / 2.0f, _center.Z + hdim.Z / 2.0f );
            Vector3 blf = new Vector3( _center.X - hdim.X / 2.0f, _center.Y - hdim.Y / 2.0f, _center.Z - hdim.Z / 2.0f );

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
                foreach ( Octree<T> child in _children )
                {
                    if ( child.Add( _objects[ i ] ) )
                    {
                        // move the object from this tree to the child
                        _objects.RemoveAt( i );
                        --i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if this octree's bounds contains or intersects another bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public bool Contains( BoundingBox box )
        {
            ContainmentType type = _bounds.Contains( box );
            return type == ContainmentType.Contains
                || type == ContainmentType.Intersects;
        }

        /// <summary>
        /// Checks to see if this octree's bounds contains or intersects a point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public bool Contains( Vector3 point )
        {
            ContainmentType type = _bounds.Contains( point );
            return type == ContainmentType.Contains
                || type == ContainmentType.Intersects;
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
                for ( int i = 0; i < 8; ++i )
                {
                    if ( _children[ i ].Add( obj ) )
                    {
                        return true;
                    }
                }

                // honestly, we shouldn't get here
                _objects.Add( obj );
                return true;
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
        /// Clears this octree. Subdivisions, if any, will remain intact.
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
        /// Checks to see if the given bounding box collides with this octree.
        /// </summary>
        /// <param name="box">The bounds to check.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box )
        {
            // make sure we at least contain the given bounding box.
            //if ( !Contains( box ) )
            //{
            //    return false;
            //}

            // check children
            if ( HasDivided )
            {
                foreach ( Octree<T> child in _children )
                {
                    if ( child.Collides( box ) )
                    {
                        return true;
                    }
                }
            }

            // check our blocks
            foreach ( T obj in _objects )
            {
                ContainmentType type = obj.Bounds.Contains(box);
                if ( type == ContainmentType.Contains || type == ContainmentType.Intersects )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this octree.
        /// </summary>
        /// <param name="box">The bounds to check.</param>
        /// <param name="collisions">The list of bounding boxes that the given box collides with.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box, out List<BoundingBox> collisions )
        {
            collisions = new List<BoundingBox>();

            // make sure we at least contain the given bounding box.
            if ( !Contains( box ) )
            {
                return false;
            }

            // check children
            if ( HasDivided )
            {
                List<BoundingBox> other = new List<BoundingBox>();
                foreach ( Octree<T> child in _children )
                {
                    if ( child.Collides( box, out other ) )
                    {
                        collisions.AddRange( other );
                    }
                }
            }

            // check our blocks
            foreach ( T obj in _objects )
            {
                ContainmentType type = obj.Bounds.Contains( box );
                if ( type == ContainmentType.Contains || type == ContainmentType.Intersects )
                {
                    collisions.Add( obj.Bounds );
                }
            }

            return collisions.Count > 0;
        }

        /// <summary>
        /// Gets the list of objects that the given bounding box collides with.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public List<T> GetCollisions( BoundingBox box )
        {
            List<T> objects = new List<T>();

            // check children first
            if ( HasDivided )
            {
                foreach ( Octree<T> child in _children )
                {
                    objects.AddRange( child.GetCollisions( box ) );
                }
            }

            // now check our objects
            foreach ( T obj in _objects )
            {
                ContainmentType type = obj.Bounds.Contains( box );
                if ( type == ContainmentType.Contains || type == ContainmentType.Intersects )
                {
                    objects.Add( obj );
                }
            }

            return objects;
        }

        /// <summary>
        /// Gets the list of blocks that a ray intersects in this octree.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns></returns>
        public Dictionary<T, float?> GetIntersections( Ray ray )
        {
            Dictionary<T, float?> intersections = new Dictionary<T, float?>();

            // if we've divided, check our children first
            if ( HasDivided )
            {
                foreach ( Octree<T> child in _children )
                {
                    intersections.Merge( child.GetIntersections( ray ) );
                }
            }

            // now check our objects
            foreach ( T obj in _objects )
            {
                float? value = obj.Bounds.Intersects( ray );
                if ( value.HasValue )
                {
                    intersections.Add( obj, value );
                }
            }

            return intersections;
        }

#if DEBUG

        /// <summary>
        /// Draws the bounds of every object within the octree for debugging purposes.
        /// </summary>
        /// <param name="device">The graphics device to draw to.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="proj">The current projection matrix.</param>
        /// <param name="color">The color to draw.</param>
        public void Draw( GraphicsDevice device, Matrix view, Matrix proj, Color color )
        {
            // draw bounds of all objects
            foreach ( T obj in _objects )
            {
                obj.Bounds.Draw( device, view, proj, color );
            }
            //_bounds.Draw( device, view, proj, color );
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    _children[ i ].Draw( device, view, proj, color );
                }
            }
        }

#endif
    }
}