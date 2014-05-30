using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Blocks;
using Kyoob.Effects;

#warning TODO : Consolidate some code in ApplyPhysics

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// The base class for entities.
    /// </summary>
    public abstract class Entity
    {
        private const float VelocityDueToGravity = -0.50f;
        private const float TerminalVelocity = -1.50f;
        private const float CollisionBuffer = 0.0001f;

        private GraphicsDevice _device;
        private World _world;
        private Vector3 _position;
        private Vector3 _translation;
        private float _velocityY;
        private bool _isOnGround;

        /// <summary>
        /// Gets or sets the translation vector.
        /// </summary>
        protected Vector3 Translation
        {
            get
            {
                return _translation;
            }
            set
            {
                _translation = value;
            }
        }

        /// <summary>
        /// Gets the graphics device this entity is on.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
        }

        /// <summary>
        /// Gets the position of the entity.
        /// </summary>
        public virtual Vector3 Position
        {
            get
            {
                // position is also the center of the size
                return _position;
            }
        }

        /// <summary>
        /// Gets the entity's world matrix.
        /// </summary>
        public virtual Matrix WorldMatrix
        {
            get
            {
                return Matrix.CreateTranslation( _position );
            }
        }

        /// <summary>
        /// Gets this entity's size.
        /// </summary>
        public virtual Vector3 Size
        {
            get
            {
                return new Vector3( 1.0f, 2.0f, 1.0f );
            }
        }

        /// <summary>
        /// Gets or sets the world the entity is in.
        /// </summary>
        public World World
        {
            get
            {
                return _world;
            }
            set
            {
                _world = value;
            }
        }

        /// <summary>
        /// Gets this entity's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return GetBounds( 0.0f, 0.0f, 0.0f );
            }
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="device">The graphics device to create the entity on.</param>
        public Entity( GraphicsDevice device )
        {
            _device = device;
            _world = null;
            _position = new Vector3();
            _translation = new Vector3();
            _velocityY = 0.0f;
            _isOnGround = false;
        }

        /// <summary>
        /// Gets the bounding box for this entity.
        /// </summary>
        /// <param name="xmod">The position X modifier.</param>
        /// <param name="ymod">The position Y modifier.</param>
        /// <param name="zmod">The position Z modifier.</param>
        protected virtual BoundingBox GetBounds( float xmod, float ymod, float zmod )
        {
            Vector3 min = new Vector3(
                ( Position.X + xmod ) - ( Size.X / 2.0f ),
                ( Position.Y + ymod ) - ( Size.Y / 2.0f ),
                ( Position.Z + zmod ) - ( Size.Z / 2.0f )
            );
            Vector3 max = new Vector3(
                ( Position.X + xmod ) + ( Size.X / 2.0f ),
                ( Position.Y + ymod ) + ( Size.Y / 2.0f ),
                ( Position.Z + zmod ) + ( Size.Z / 2.0f )
            );
            return new BoundingBox( min, max );
        }


        /// <summary>
        /// Gets the surrounding chunks.
        /// </summary>
        /// <returns></returns>
        private List<Chunk> GetSurroundingChunks()
        {
            List<Chunk> chunks = new List<Chunk>();

            // get the 9 surrounding chunks
            for ( int x = -1; x <= 1; ++x )
            {
                for ( int y = -1; y <= 1; ++y )
                {
                    for ( int z = -1; z <= 1; ++z )
                    {
                        chunks.Add( World.GetChunk( new Vector3(
                            Position.X + x * Chunk.Size,
                            Position.Y + y * Chunk.Size,
                            Position.Z + z * Chunk.Size
                        ) ) );
                    }
                }
            }

            // remove null chunks
            for ( int i = 0; i < chunks.Count; ++i )
            {
                if ( chunks[ i ] == null )
                {
                    chunks.RemoveAt( i );
                    --i;
                }
            }

            return chunks;
        }

        /// <summary>
        /// Gets the minimum value in a collection of nullable floats.
        /// </summary>
        /// <param name="values">The values to check.</param>
        /// <param name="defValue">The default value.</param>
        /// <returns></returns>
        private float GetMinimumValue( IEnumerable<float?> values, float defValue )
        {
            float val = defValue;
            foreach ( float? value in values )
            {
                if ( value.HasValue && value.Value < val )
                {
                    val = value.Value;
                }
            }
            return val;
        }

        /// <summary>
        /// Gets the shortest collision distance in the XZ direction.
        /// </summary>
        /// <param name="chunk">The chunk to check for collisions</param>
        /// <param name="rayDir">The ray direction.</param>
        /// <param name="offs">The position offset to use.</param>
        /// <returns></returns>
        private float GetShortestCollisionDistanceXZ( Chunk chunk, Vector3 rayDir, Vector3 offs )
        {
            float startY = -Size.Y / 2.0f;
            float endY = Size.Y / 2.0f;
            int countY = (int)( Math.Ceiling( Size.Y ) / Cube.Size ) + 1;
            float incY = Math.Abs( endY - startY ) / (float)( countY - 1 );

            float dist = Chunk.Size;
            for ( float y = startY; y <= endY; y += incY )
            {
                Ray ray = new Ray(
                    new Vector3(
                        Position.X + offs.X,
                        Position.Y + offs.Y + y,
                        Position.Z + offs.Z
                    ),
                    rayDir
                );

                dist = Math.Min( dist, GetMinimumValue( chunk.GetIntersections( ray ).Values, Chunk.Size ) );
            }

            return dist;
        }

        /// <summary>
        /// Gets the shortest collision distance in the Y direction.
        /// </summary>
        /// <param name="chunk">The chunk to check.</param>
        /// <param name="dir">The direction.</param>
        /// <returns></returns>
        private float GetShortestCollisionDistanceY( Chunk chunk, Vector3 dir )
        {
            // create our rays
            Ray ray0 = new Ray( new Vector3( Position.X + Size.X / 2.0f, Position.Y + dir.Y * Size.Y / 2.0f, Position.Z + Size.Z / 2.0f ), dir );
            Ray ray1 = new Ray( new Vector3( Position.X + Size.X / 2.0f, Position.Y + dir.Y * Size.Y / 2.0f, Position.Z - Size.Z / 2.0f ), dir );
            Ray ray2 = new Ray( new Vector3( Position.X - Size.X / 2.0f, Position.Y + dir.Y * Size.Y / 2.0f, Position.Z + Size.Z / 2.0f ), dir );
            Ray ray3 = new Ray( new Vector3( Position.X - Size.X / 2.0f, Position.Y + dir.Y * Size.Y / 2.0f, Position.Z - Size.Z / 2.0f ), dir );

            // go through each chunk
            float dist0 = GetMinimumValue( chunk.GetIntersections( ray0 ).Values, Chunk.Size );
            float dist1 = GetMinimumValue( chunk.GetIntersections( ray1 ).Values, Chunk.Size );
            float dist2 = GetMinimumValue( chunk.GetIntersections( ray2 ).Values, Chunk.Size );
            float dist3 = GetMinimumValue( chunk.GetIntersections( ray3 ).Values, Chunk.Size );

            float dist = Math.Min( Math.Min( dist0, dist1 ), Math.Min( dist2, dist3 ) );
            return dist;
        }

        /// <summary>
        /// Applies collision and gravity physics to the player.
        /// </summary>
        /// <param name="time">The time since the last frame update.</param>
        protected void ApplyPhysics( float time )
        {
            // apply some falling physics
            _velocityY += VelocityDueToGravity * time;
            if ( _velocityY < TerminalVelocity )
            {
                _velocityY = TerminalVelocity;
            }
            _translation.Y = _velocityY;

            // get the surrounding chunks
            Chunk[] surrounding = GetSurroundingChunks().ToArray();


            // create the XZ offset vectors
            Vector3 offsX = new Vector3( Size.X / 2.0f, 0.0f, 0.0f );
            Vector3 offsZ = new Vector3( 0.0f, 0.0f, Size.Z / 2.0f );


            // X physics
            BoundingBox boxx = GetBounds( _translation.X, 0.0f, 0.0f );
            if ( _translation.X > 0.0f )
            {
                foreach ( Chunk chunk in surrounding )
                {
                    if ( chunk.Collides( boxx ) )
                    {
                        float dist = Math.Min(
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Right, offsX + offsZ ),
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Right, offsX - offsZ )
                        ) - CollisionBuffer;
                        if ( dist < _translation.X )
                        {
                            _translation.X = dist;
                        }
                    }
                }
            }
            else if ( _translation.X < 0.0f )
            {
                foreach ( Chunk chunk in surrounding )
                {
                    if ( chunk.Collides( boxx ) )
                    {
                        float dist = Math.Min(
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Left, -offsX + offsZ ),
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Left, -offsX - offsZ )
                        ) - CollisionBuffer;
                        if ( dist < -_translation.X )
                        {
                            _translation.X = -dist;
                        }
                    }
                }
            }

            // Z physics
            BoundingBox boxz = GetBounds( 0.0f, 0.0f, _translation.Z );
            if ( _translation.Z > 0.0f )
            {
                foreach ( Chunk chunk in surrounding )
                {
                    if ( chunk.Collides( boxz ) )
                    {
                        float dist = Math.Min(
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Backward, offsZ + offsX ),
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Backward, offsZ - offsX )
                        ) - CollisionBuffer;
                        if ( dist < _translation.Z )
                        {
                            _translation.Z = dist;
                        }
                    }
                }
            }
            else if ( _translation.Z < 0.0f )
            {
                foreach ( Chunk chunk in surrounding )
                {
                    if ( chunk.Collides( boxz ) )
                    {
                        float dist = Math.Min(
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Forward, -offsZ + offsX ),
                            GetShortestCollisionDistanceXZ( chunk, Vector3.Forward, -offsZ - offsX )
                        ) - CollisionBuffer;
                        if ( dist < -_translation.Z )
                        {
                            _translation.Z = -dist;
                        }
                    }
                }
            }


            // Y physics (less complex than XZ physics)
            if ( _translation.Y != 0.0f )
            {
                _isOnGround = false;
                Vector3 dir = ( _translation.Y < 0.0f ) ? Vector3.Down : Vector3.Up;

                // check each surrounding chunk
                foreach ( Chunk chunk in surrounding )
                {
                    // get the absolute minimum collision distance
                    float dist = GetShortestCollisionDistanceY( chunk, dir ) - CollisionBuffer;
                    if ( dist < Math.Abs( _translation.Y ) )
                    {
                        // if we've hit a block going down, then we're on "ground"
                        if ( _translation.Y < 0.0f )
                        {
                            _isOnGround = true;
                        }

                        _translation.Y = dist * Math.Sign( _translation.Y );
                        _velocityY = 0.0f;
                    }
                }
            }
        }

        /// <summary>
        /// Causes the entity to jump.
        /// </summary>
        /// <param name="time">The amount of time (in seconds) since the last update.</param>
        /// <param name="velocity">The jump velocity.</param>
        public void Jump( float time, float velocity )
        {
            if ( _isOnGround )
            {
                _velocityY += velocity * time;
                _isOnGround = false;
            }
        }


        /// <summary>
        /// Moves the entity.
        /// </summary>
        /// <param name="x">The X amount to move.</param>
        /// <param name="y">The Y amount to move.</param>
        /// <param name="z">The Z amount to move.</param>
        public void Move( float x, float y, float z )
        {
            Move( new Vector3( x, y, z ) );
        }

        /// <summary>
        /// Moves the entity.
        /// </summary>
        /// <param name="amount">The amount to move.</param>
        public void Move( Vector3 amount )
        {
            _position += amount;
        }

        /// <summary>
        /// Moves the entity to the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate to move to.</param>
        /// <param name="y">The Y coordinate to move to.</param>
        /// <param name="z">The Z coordinate to move to.</param>
        public void MoveTo( float x, float y, float z )
        {
            MoveTo( new Vector3( x, y, z ) );
        }

        /// <summary>
        /// Moves the entity to the given coordinates.
        /// </summary>
        /// <param name="pos">The coordinates to move to.</param>
        public void MoveTo( Vector3 pos )
        {
            _position = pos;
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public abstract void Update( GameTime gameTime );

        /// <summary>
        /// Draws the entity.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="effect">The effect to use to draw the entity.</param>
        public abstract void Draw( GameTime gameTime, BaseEffect effect );
    }
}