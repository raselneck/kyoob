using System;
using System.Collections.Generic;
using Kyoob.Blocks;
using Kyoob.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Game.Management
{
    /// <summary>
    /// The game state to be displayed while the world loads initial terrain data.
    /// </summary>
    public sealed class LoadState : GameState
    {
        private float _progress;
        private SpriteFont _font;
        private PresentationParameters _pp;

        /// <summary>
        /// Creates a new load state.
        /// </summary>
        /// <param name="controller">The state system controlling this state.</param>
        public LoadState( StateSystem controller )
            : base( controller )
        {
            _pp = Controller.Engine.GraphicsDevice.PresentationParameters;
            _font = Controller.Engine.Content.Load<SpriteFont>( "font/arial" );

            Controller.Engine.IsMouseVisible = true;

            // start the world chunk management
            Controller.World.StartChunkManagement(
                Controller.Settings.CameraSettings.InitialPosition,
                Controller.Settings.CameraSettings.ClipFar
            );
        }

        /// <summary>
        /// Attempts to find a suitable starting point.
        /// </summary>
        /// <returns></returns>
        private Block FindSuitableStartPoint()
        {
            Index3D start = World.PositionToIndex( Controller.Settings.CameraSettings.InitialPosition );
            int dist = 0;

            // get the surrounding chunks
            List<Chunk> surrounding = new List<Chunk>();
            for ( int x = -3; x < 4; ++x )
            {
                for ( int y = -3; y < 4; ++y )
                {
                    for ( int z = -3; z < 4; ++z )
                    {
                        Index3D idx = new Index3D( x, y, z );
                        Chunk chunk = Controller.World.GetChunk( idx );
                        if ( chunk != null )
                        {
                            surrounding.Add( chunk );
                        }
                    }
                }
            }

            // check each surrounding chunk
            foreach ( Chunk chunk in surrounding )
            {
                foreach ( Block grass in chunk.GrassBlocks )
                {
                    Vector3 pos = grass.Position;
                    pos.Y += 2;
                    BlockType above = Controller.World.GetBlockType( pos );
                    if ( above == BlockType.Air )
                    {
                        return grass;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Updates the load state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            // switch to the play state if we've loaded 90% or more of the world
            _progress = Controller.World.ChunkManager.ChunkCreationProgress;
            if ( _progress >= 0.90f )
            {
                // get the suitable start position and move the player
                Block start = FindSuitableStartPoint();
                Vector3 pos = start.Position;
                pos.Y += 2;
                ( Controller.GetState( "play" ) as PlayState ).Player.MoveTo( pos );

                // switch to the play state
                Controller.Engine.IsMouseVisible = false;
                Controller.ChangeState( "play" );
            }
        }

        /// <summary>
        /// Draws the load state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Draw( GameTime gameTime )
        {
            string text = string.Format( "progress: {0:0.00}%", _progress * 100 );
            Vector2 size = _font.MeasureString( text );
            Vector2 loc = new Vector2( _pp.BackBufferWidth / 2, _pp.BackBufferHeight / 2 );
            loc.X -= size.X / 2;
            loc.Y -= size.Y / 2;

            Controller.Engine.GraphicsDevice.Clear( Color.Black );
            Renderer2D.Begin();
            Renderer2D.DrawString( _font, text, loc, Color.White );
            Renderer2D.End();
        }
    }
}