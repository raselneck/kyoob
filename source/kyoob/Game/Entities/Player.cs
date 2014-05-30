using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Debug;
using Kyoob.Effects;

#warning TODO : Move physics / jump stuff to Entity

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// An implementation for the player.
    /// </summary>
    public class Player : Entity
    {
        private const float VelocityDueToGravity = -0.50f;
        private const float TerminalVelocity     = -1.50f;
        private const float CollisionBuffer      =  0.0001f;

        private World _world;
        private PlayerCamera _camera;
        private Vector3 _translation;
        private KeyboardState _currKeys;
        private KeyboardState _prevKeys;
        private float _velocityY;

        /// <summary>
        /// Gets the player's camera.
        /// </summary>
        public PlayerCamera Camera
        {
            get
            {
                return _camera;
            }
        }

        /// <summary>
        /// Gets or sets the world the player is in.
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
        /// Gets the player's size.
        /// </summary>
        public override Vector3 Size
        {
            get
            {
                return new Vector3( 0.8f, 1.6f, 0.8f );
            }
        }

        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="device">The graphics device to create the player on.</param>
        /// <param name="settings">The settings for the player's camera.</param>
        public Player( GraphicsDevice device, CameraSettings settings )
            : base( device )
        {
            _camera = new PlayerCamera( settings, this );
            _camera.EyeHeight = 0.6f;
            _world = null;
            _currKeys = Keyboard.GetState();
            _prevKeys = Keyboard.GetState();
            _velocityY = 0.0f;
        }

        /// <summary>
        /// Gets a movement vector.
        /// </summary>
        /// <param name="basis">The basis vector.</param>
        /// <param name="units">The "units per second" to multiply by.</param>
        /// <returns></returns>
        private Vector3 GetMovementVector( Vector3 basis, float units )
        {
            Vector3 transformed = Vector3.Transform( basis, _camera.Rotation );
            transformed.Y = 0.0f;
            return transformed * units;
        }

        /// <summary>
        /// Checks user input.
        /// </summary>
        /// <param name="time">The total number of seconds since the last frame.</param>
        private void CheckUserInput( float time )
        {
            // calculate our "units per second"
            float units = time * 6.0f;
            if ( _currKeys.IsKeyDown( Keys.LeftShift ) )
            {
                units *= 2.25f;
            }

            // get transformed basis vectors
            Vector3 forward = GetMovementVector( Vector3.Forward, units );
            Vector3 up      = GetMovementVector( Vector3.Up, units );
            Vector3 right   = GetMovementVector( Vector3.Right, units );
            if ( _camera.Pitch >= 0.0f )
            {
                up = -up;
            }

            // check if we need to strafe forward
            if ( _currKeys.IsKeyDown( Keys.W ) )
            {
                _translation += forward;
                _translation += up;
            }

            // check if we need to strafe backward
            if ( _currKeys.IsKeyDown( Keys.S ) )
            {
                _translation -= forward;
                _translation -= up;
            }

            // check if we need to strafe right
            if ( _currKeys.IsKeyDown( Keys.D ) )
            {
                _translation += right;
            }

            // check if we need to strafe left
            if ( _currKeys.IsKeyDown( Keys.A ) )
            {
                _translation -= right;
            }


            if ( _currKeys.IsKeyDown( Keys.Space ) )
            {
                _translation.Y += units;
            }
            if ( _currKeys.IsKeyDown( Keys.LeftControl ) )
            {
                _translation.Y -= units;
            }
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
                        chunks.Add( _world.GetChunk( new Vector3(
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



        float TICK_COUNT;
        int FRAME_COUNT;
        float TIME_XZ;
        float TIME_Y;



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
            float endY   =  Size.Y / 2.0f;
            int   countY = (int)( Math.Ceiling( Size.Y ) / Cube.Size ) + 1;
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
        private void ApplyPhysics( float time )
        {
            // "jump" if we pressed space
            if ( _currKeys.IsKeyUp( Keys.Space ) && _prevKeys.IsKeyDown( Keys.Space ) )
            {
                _velocityY += 16.0f * time;
            }

            // apply some falling physics
            _velocityY += VelocityDueToGravity * time;
            if ( _velocityY < TerminalVelocity )
            {
                _velocityY = TerminalVelocity;
            }
            _translation.Y = _velocityY;

            // get the surrounding chunks
            Chunk[] surrounding = GetSurroundingChunks().ToArray();


            Stopwatch watch = Stopwatch.StartNew();


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


            TIME_XZ += (float)watch.Elapsed.TotalMilliseconds;
            watch = Stopwatch.StartNew();


            // Y physics (less complex than XZ physics)
            if ( _translation.Y != 0.0f )
            {
                Vector3 dir = ( _translation.Y < 0.0f ) ? Vector3.Down : Vector3.Up;

                // check each surrounding chunk
                foreach ( Chunk chunk in surrounding )
                {
                    // get the absolute minimum collision distance
                    float dist = GetShortestCollisionDistanceY( chunk, dir ) - CollisionBuffer;
                    if ( dist < Math.Abs( _translation.Y ) )
                    {
                        _translation.Y = dist * Math.Sign( _translation.Y );
                        _velocityY = 0.0f;
                    }
                }
            }


            TIME_Y += (float)watch.Elapsed.TotalMilliseconds;
        }



        /// <summary>
        /// Updates the player.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            _currKeys = Keyboard.GetState();

            // we really only need to update if we're in a world
            if ( _world != null )
            {
                float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if ( _camera.HasControl )
                {
                    CheckUserInput( time );
                    ApplyPhysics( time );
                }

                // move the player and reset out translation vector
                Move( _translation );
                _translation = Vector3.Zero;
            }
            _camera.Update( gameTime );

            _prevKeys = _currKeys;


            TICK_COUNT += (float)gameTime.ElapsedGameTime.TotalSeconds;
            ++FRAME_COUNT;
            if ( TICK_COUNT >= 1.0f )
            {
                TICK_COUNT -= 1.0f;
                Terminal.WriteLine( Color.White, 1.0f, "XZ physics: {0:0.00}ms", TIME_XZ / FRAME_COUNT );
                Terminal.WriteLine( Color.White, 1.0f, "Y  physics: {0:0.00}ms", TIME_Y / FRAME_COUNT );
                TIME_Y = 0.0f;
                TIME_XZ = 0.0f;
                FRAME_COUNT = 0;
            }
        }

        /// <summary>
        /// Draws the player.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="effect">The effect to use when drawing.</param>
        public override void Draw( GameTime gameTime, BaseEffect effect )
        {
#if DEBUG
            //Chunk current = _world.GetChunk( Position );
            //if ( current != null )
            //{
            //    current.Octree.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
            //    current.Bounds.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Black );
            //}

            BoundingBox box = GetBounds( 0.0f, 0.0f, 0.0f );
            box.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
#endif
        }
    }
}