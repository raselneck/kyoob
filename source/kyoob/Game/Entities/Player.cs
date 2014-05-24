using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kyoob.Blocks;
using Kyoob.Effects;

namespace Kyoob.Game.Entities
{
    /// <summary>
    /// An implementation for the player.
    /// </summary>
    public class Player : Entity
    {
        private const float VelocityDueToGravity    = -0.50f;
        private const float TerminalVelocity        = -2.00f;
        private const float CollisionDistanceBuffer =  0.10f;

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
        /// Creates a new player.
        /// </summary>
        /// <param name="device">The graphics device to create the player on.</param>
        /// <param name="settings">The settings for the player's camera.</param>
        public Player( GraphicsDevice device, CameraSettings settings )
            : base( device )
        {
            // _camera = new PlayerCamera( settings, this );
            _camera = new PlayerCamera( settings, this );
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
            if ( _camera.Pitch >= MathHelper.PiOver4 )
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

            for ( int x = -1; x <= 1; ++x )
            {
                for ( int y = -1; y <= 1; ++y )
                {
                    for ( int z = -1; z <= 1; ++z )
                    {
                        Chunk chunk = _world.GetChunk(
                            new Vector3(
                                Position.X + x * Chunk.Size,
                                Position.Y + y * Chunk.Size,
                                Position.Z + z * Chunk.Size
                            )
                        );
                        if ( chunk != null )
                        {
                            chunks.Add( chunk );
                        }
                    }
                }
            }

            return chunks;
        }

        /// <summary>
        /// Queries each chunk in a given list for blocks intersecting the given ray.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="ray">The ray.</param>
        /// <returns></returns>
        private List<Block> QueryChunks( List<Chunk> chunks, Ray ray )
        {
            List<Block> blocks = new List<Block>();

            foreach ( Chunk chunk in chunks )
            {
                blocks.AddRange( chunk.GetIntersections( ray ) );
            }

            return blocks;
        }

        /// <summary>
        /// Checks physics in the XZ direction.
        /// </summary>
        /// <param name="chunks">The list of chunks to query.</param>
        /// <param name="ray">The ray.</param>
        /// <param name="value">The value to modify.</param>
        private void CheckPhysicsXZ( List<Chunk> chunks, Ray ray, ref float value )
        {
            foreach ( Block block in QueryChunks( chunks, ray ) )
            {
                float dist = block.GetInstersectionDistance( ray ).Value - CollisionDistanceBuffer;
                if ( Math.Abs( value ) > dist )
                {
                    value = dist * Math.Sign( value );
                }
            }
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
            List<Chunk> surrounding = GetSurroundingChunks();
            if ( surrounding.Count == 0 )
            {
                return;
            }

            // Z direction
            if ( _translation.Z < 0.0f )
            {
                CheckPhysicsXZ( surrounding, new Ray( Position, Vector3.Forward ), ref _translation.Z );
            }
            else if ( _translation.Z > 0.0f )
            {
                CheckPhysicsXZ( surrounding, new Ray( Position, Vector3.Backward ), ref _translation.Z );
            }

            // X direction
            if ( _translation.X < 0.0f )
            {
                CheckPhysicsXZ( surrounding, new Ray( Position, Vector3.Left ), ref _translation.X );
            }
            else if ( _translation.X > 0.0f )
            {
                CheckPhysicsXZ( surrounding, new Ray( Position, Vector3.Right ), ref _translation.X );
            }

            // Y direction
            if ( _translation.Y < 0.0f )
            {
                // downward [0,1,0] checking
                Ray ray = new Ray( Position, Vector3.Down );
                foreach ( Block block in QueryChunks( surrounding, ray ) )
                {
                    // dist will be positive, translation will be negative
                    float dist = block.GetInstersectionDistance( ray ).Value - CollisionDistanceBuffer - Size.Y * 0.75f;
                    if ( Math.Abs( _translation.Y ) > dist )
                    {
                        _translation.Y = dist * Math.Sign( _translation.Y );
                        _velocityY = 0.0f;
                    }
                }
            }
            else if ( _translation.Y > 0.0f )
            {
                // upward [0,1,0] checking
                Ray ray = new Ray( Position, Vector3.Up );
                foreach ( Block block in QueryChunks( surrounding, new Ray( Position, Vector3.Up ) ) )
                {
                    // dist will be positive, translation will be positive
                    float dist = block.GetInstersectionDistance( ray ).Value - CollisionDistanceBuffer - Size.Y * 0.25f;
                    if ( Math.Abs( _translation.Y ) > dist )
                    {
                        _translation.Y = dist * Math.Sign( _translation.Y );
                        _velocityY = 0.0f;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the player.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            _camera.Update( gameTime );
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

            _prevKeys = _currKeys;
        }

        /// <summary>
        /// Draws the player.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        /// <param name="effect">The effect to use when drawing.</param>
        public override void Draw( GameTime gameTime, BaseEffect effect )
        {
            //Chunk current = _world.GetChunk( Position );
            //if ( current != null )
            //{
            //    current.Bounds.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
            //    current.Octree.Draw( GraphicsDevice, _camera.View, _camera.Projection, Color.Magenta );
            //}
        }
    }
}