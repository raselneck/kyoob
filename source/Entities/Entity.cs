using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Kyoob.VoxelData;

// TODO : Use velocity variable instead of translation

namespace Kyoob.Entities
{
    /// <summary>
    /// The base class for all entities.
    /// </summary>
    public abstract class Entity
    {
        protected const float JumpVelocity     =  1.00f;
        protected const float JumpForce        =  0.16f;
        protected const float MoveVelocity     =  2.40f;
        protected const float SprintModifier   =  1.50f;
        protected const float GravityVelocity  = -0.50f;
        protected const float TerminalVelocity = -1.50f;
        protected const float CollisionBuffer  =  0.00005f;
        private const int NumberOfSurroundingChunks = 27; // 3 * 3 * 3

        protected Vector3 _translation;
        private Chunk[] _surroundingChunks;
        private float _yVelocity;

        /// <summary>
        /// Gets this entity's position.
        /// </summary>
        public virtual Vector3 Position
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets this entity's size.
        /// </summary>
        public virtual Vector3 Size
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets this entity's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                BoundingBox bounds;
                GetBounds( out bounds );
                return bounds;
            }
        }

        /// <summary>
        /// Checks to see if this entity is on the ground.
        /// </summary>
        protected bool IsOnGround
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        public Entity()
        {
            Position = new Vector3();
            Size = new Vector3();
            IsOnGround = false;

            _translation = new Vector3();
            _surroundingChunks = new Chunk[ NumberOfSurroundingChunks ];
            _yVelocity = 0.0f;
        }

        /// <summary>
        /// Updates this entity.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update( GameTime gameTime )
        {
            Position += _translation;
            _translation *= 0.5f;
            // TODO : Incorporate time for translation damping
        }

        /// <summary>
        /// Moves the entity.
        /// </summary>
        /// <param name="x">The X amount to move.</param>
        /// <param name="y">The Y amount to move.</param>
        /// <param name="z">The Z amount to move.</param>
        public virtual void Move( float x, float y, float z )
        {
            if ( !float.IsNaN( x ) )
            {
                _translation.X += x;
            }
            if ( !float.IsNaN( y ) )
            {
                _translation.Y += y;
            }
            if ( !float.IsNaN( z ) )
            {
                _translation.Z += z;
            }
        }

        /// <summary>
        /// Moves the entity.
        /// </summary>
        /// <param name="amount">The amount to move.</param>
        public void Move( Vector3 amount )
        {
            Move( amount.X, amount.Y, amount.Z );
        }

        /// <summary>
        /// Moves the entity to the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate to move to.</param>
        /// <param name="y">The Y coordinate to move to.</param>
        /// <param name="z">The Z coordinate to move to.</param>
        public void MoveTo( float x, float y, float z )
        {
            // move
            float dx = x - Position.X;
            float dy = y - Position.Y;
            float dz = z - Position.Z;
            Move( dx, dy, dz );

            // clear the translation
            _translation.X = _translation.Y = _translation.Z = 0.0f;
        }

        /// <summary>
        /// Moves the entity to the given coordinates.
        /// </summary>
        /// <param name="pos">The coordinates to move to.</param>
        public void MoveTo( Vector3 pos )
        {
            MoveTo( pos.X, pos.Y, pos.Z );
        }

        /// <summary>
        /// Causes this entity to jump if they are on the ground.
        /// </summary>
        public void Jump()
        {
            if ( IsOnGround && Position.Y < TerrainGenerator.MaxEntityHeight )
            {
                _yVelocity += JumpForce * JumpVelocity;
            }
        }

        /// <summary>
        /// Applies physics to this entity.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected void ApplyPhysics( GameTime gameTime )
        {
            // get the time value that we're going to use
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // add some falling physics
            _yVelocity += GravityVelocity * time;
            _yVelocity = Math.Max( _yVelocity, TerminalVelocity );
            _translation.Y = _yVelocity;

            // find the surrounding chunks
            FindSurroundingChunks();



            // create variables for Y physics
            IsOnGround = false;
            Vector3 yDir = ( _translation.Y < 0.0f ) ? Vector3.Down : Vector3.Up;

            // go through and perform Y physics calculations. we need to do these BEFORE
            // XZ physics so that the offset bounding boxes will work properly
            for ( int i = 0; i < NumberOfSurroundingChunks; ++i )
            {
                // get the current chunk
                var chunk = _surroundingChunks[ i ];
                if ( chunk == null )
                {
                    continue;
                }

                // check collision distance
                float dist = GetShortestCollisionDistanceY( chunk, ref yDir ) - CollisionBuffer;
                if ( dist < Math.Abs( _translation.Y ) )
                {
                    // if we've hit a block going down, then we're on something we can stand on
                    if ( _translation.Y < 0.0f )
                    {
                        IsOnGround = true;
                    }

                    _translation.Y = dist * Math.Sign( _translation.Y );
                    _yVelocity = 0.0f;
                }
            }



            // create variables for XZ physics
            Vector3 offsX = new Vector3( Size.X * 0.5f, 0.0f, 0.0f );
            Vector3 offsY = new Vector3( 0.0f, _translation.Y, 0.0f );
            Vector3 offsZ = new Vector3( 0.0f, 0.0f, Size.Z * 0.5f );
            BoundingBox boxX, boxZ;
            GetBounds( _translation.X, _translation.Y, 0.0f, out boxX );
            GetBounds( 0.0f, _translation.Y, _translation.Z, out boxZ );
            Vector3 vecX = ( _translation.X > 0.0f ) ? Vector3.Right    : Vector3.Left;
            Vector3 vecZ = ( _translation.Z > 0.0f ) ? Vector3.Backward : Vector3.Forward;
            float signX = Math.Sign( _translation.X );
            float signZ = Math.Sign( _translation.Z );

            // now go through all of the surrounding chunks
            for ( int i = 0; i < _surroundingChunks.Length; ++i )
            {
                // get the current chunk
                var chunk = _surroundingChunks[ i ];
                if ( chunk == null )
                {
                    continue;
                }

                // check for X direction collisions
                if ( chunk.Collides( boxX ) )
                {
                    float distX = Math.Min(
                        GetShortestCollisionDistanceXZ( chunk, vecX, signX * offsX + offsZ + offsY ),
                        GetShortestCollisionDistanceXZ( chunk, vecX, signX * offsX - offsZ + offsY )
                    ) - CollisionBuffer;
                    if ( distX < Math.Abs( _translation.X ) )
                    {
                        _translation.X = distX * signX;
                    }
                }

                // check for Z direction collisions
                if ( chunk.Collides( boxZ ) )
                {
                    float distZ = Math.Min(
                        GetShortestCollisionDistanceXZ( chunk, vecZ, signZ * offsZ + offsX + offsY ),
                        GetShortestCollisionDistanceXZ( chunk, vecZ, signZ * offsZ - offsX + offsY )
                    ) - CollisionBuffer;
                    if ( distZ < Math.Abs( _translation.Z ) )
                    {
                        _translation.Z = distZ * signZ;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the bounding box for this entity with no offset.
        /// </summary>
        /// <param name="bounds">The bounding box to populate.</param>
        protected virtual void GetBounds( out BoundingBox bounds )
        {
            GetBounds( 0.0f, 0.0f, 0.0f, out bounds );
        }

        /// <summary>
        /// Gets the bounding box for this entity with an offset.
        /// </summary>
        /// <param name="xOffs">The X offset.</param>
        /// <param name="yOffs">The Y offset.</param>
        /// <param name="zOffs">The Z offset.</param>
        /// <param name="bounds">The bounding box to populate.</param>
        protected virtual void GetBounds( float xOffs, float yOffs, float zOffs, out BoundingBox bounds )
        {
            bounds.Min.X = ( Position.X + xOffs ) - Size.X * 0.5f;
            bounds.Min.Y = ( Position.Y + yOffs ) - Size.Y * 0.5f;
            bounds.Min.Z = ( Position.Z + zOffs ) - Size.Z * 0.5f;
            bounds.Max.X = ( Position.X + xOffs ) + Size.X * 0.5f;
            bounds.Max.Y = ( Position.Y + yOffs ) + Size.Y * 0.5f;
            bounds.Max.Z = ( Position.Z + zOffs ) + Size.Z * 0.5f;
        }

        /// <summary>
        /// Gets the minimum value from a list of floats.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private float GetMinimumFromList( List<float> list )
        {
            if ( list.Count == 0 )
            {
                return float.MaxValue;
            }

            var min = list[ 0 ];
            for ( int i = 1; i < list.Count; ++i )
            {
                min = Math.Min( list[ i ], min );
            }
            return min;
        }

        /// <summary>
        /// Gets the shortest collision distance in the XZ plane for this entity.
        /// </summary>
        /// <param name="chunk">The current chunk.</param>
        /// <param name="rayDir">The direction of the ray.</param>
        /// <param name="offs">The position offset.</param>
        /// <returns></returns>
        private float GetShortestCollisionDistanceXZ( Chunk chunk, Vector3 rayDir, Vector3 offs )
        {
            // loop helpers
            float startY = -Size.Y * 0.5f;
            float endY = Size.Y * 0.5f;
            int countY = (int)Math.Ceiling( Size.Y ) + 1;
            float yStep = Math.Abs( endY - startY ) / (float)( countY - 1 );

            // create the ray
            Ray ray = new Ray();
            ray.Position.X = Position.X + offs.X;
            ray.Position.Z = Position.Z + offs.Z;
            ray.Direction = rayDir;

            // get the nearest collision distance
            List<float> intersections = new List<float>();
            float dist = float.MaxValue;
            for ( float y = startY; y <= endY; y += yStep )
            {
                ray.Position.Y = Position.Y + offs.Y + y;
                chunk.GetIntersectionDistances( ray, ref intersections );
                dist = Math.Min( dist, GetMinimumFromList( intersections ) );
            }

            return dist;
        }

        /// <summary>
        /// Gets the shortest collision distance in the Y direction for this entity.
        /// </summary>
        /// <param name="chunk">The current chunk.</param>
        /// <param name="rayDir">The ray direction.</param>
        /// <returns></returns>
        private float GetShortestCollisionDistanceY( Chunk chunk, ref Vector3 rayDir )
        {
            // create our rays
            Ray ray0 = new Ray( new Vector3( Position.X + Size.X * 0.5f, Position.Y + rayDir.Y * Size.Y * 0.5f, Position.Z + Size.Z * 0.5f ), rayDir );
            Ray ray1 = new Ray( new Vector3( Position.X + Size.X * 0.5f, Position.Y + rayDir.Y * Size.Y * 0.5f, Position.Z - Size.Z * 0.5f ), rayDir );
            Ray ray2 = new Ray( new Vector3( Position.X - Size.X * 0.5f, Position.Y + rayDir.Y * Size.Y * 0.5f, Position.Z + Size.Z * 0.5f ), rayDir );
            Ray ray3 = new Ray( new Vector3( Position.X - Size.X * 0.5f, Position.Y + rayDir.Y * Size.Y * 0.5f, Position.Z - Size.Z * 0.5f ), rayDir );

            // get each collision distance
            List<float> intersections = new List<float>();
            chunk.GetIntersectionDistances( ray0, ref intersections );
            float dist0 = GetMinimumFromList( intersections );
            chunk.GetIntersectionDistances( ray1, ref intersections );
            float dist1 = GetMinimumFromList( intersections );
            chunk.GetIntersectionDistances( ray2, ref intersections );
            float dist2 = GetMinimumFromList( intersections );
            chunk.GetIntersectionDistances( ray3, ref intersections );
            float dist3 = GetMinimumFromList( intersections );

            // now return the absolute minimum
            return Math.Min( Math.Min( dist0, dist1 ), Math.Min( dist2, dist3 ) );
        }

        /// <summary>
        /// Finds the surrounding chunks.
        /// </summary>
        private void FindSurroundingChunks()
        {
            Vector3 center = new Vector3();
            int index = 0;
            for ( int x = -1; x <= 1; ++x )
            {
                for ( int y = -1; y <= 1; ++y )
                {
                    for ( int z = -1; z <= 1; ++z )
                    {
                        center.X = Position.X + x * ChunkData.Size * 0.5f;
                        center.Y = Position.Y + y * ChunkData.Size * 0.5f;
                        center.Z = Position.Z + z * ChunkData.Size * 0.5f;

                        var chunk = World.Instance.FindChunk( center );
                        _surroundingChunks[ index ] = chunk;
                        index++;
                    }
                }
            }
        }
    }
}