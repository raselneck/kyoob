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
        private const string InitialProgressText = "creating some terrain";

        private float _progress;
        private float _progressTime;
        private string _progressText;
        private Texture2D _pixel;
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
            _font = Controller.Engine.Content.Load<SpriteFont>( "font/consolas" );

            _progressTime = 0.0f;
            _progressText = InitialProgressText;

            _pixel = new Texture2D( Controller.Engine.GraphicsDevice, 1, 1 );
            _pixel.SetData( new[] { Color.White } );

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

            // get the surrounding chunks
            Index3D idx;
            List<Chunk> surrounding = new List<Chunk>();
            for ( int x = 0; x < 4; ++x )
            {
                for ( int y = 0; y < 4; ++y )
                {
                    for ( int z = 0; z < 4; ++z )
                    {
                        idx = new Index3D( x, y, z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( x, y, -z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( x, -y, z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( x, -y, -z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( -x, y, z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( -x, y, -z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( -x, -y, z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );

                        idx = new Index3D( -x, -y, -z );
                        surrounding.Add( Controller.World.GetChunk( idx ) );
                    }
                }
            }

            // check each surrounding chunk
            foreach ( Chunk chunk in surrounding )
            {
                if ( chunk == null )
                {
                    continue;
                }
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
        /// Switches to the play state.
        /// </summary>
        private void SwitchToPlayState()
        {
            // get the suitable start position and move the player
            Block start = FindSuitableStartPoint();
            Vector3 pos = start.Position;
            pos.Y += 2;
            ( Controller.GetState( "play" ) as PlayState ).Player.MoveTo( pos );

            // update the chunk manager
            Controller.World.ChunkManager.ViewPosition = pos;

            // switch to the play state
            Controller.Engine.IsMouseVisible = false;
            Controller.ChangeState( "play" );
        }

        /// <summary>
        /// Draws the progress bar.
        /// </summary>
        private void DrawProgressBar()
        {
            float maxWidth = _pp.BackBufferWidth * 0.95f;

            int height = 10;
            int x = (int)Math.Round( _pp.BackBufferWidth * 0.025f );
            int y = _pp.BackBufferHeight / 2 - height / 2;

            Rectangle rectPartial = new Rectangle(
                x + 2, y + 2, (int)Math.Round( _progress * maxWidth ), height - 4
            );

            Rectangle rectFull = new Rectangle(
                x, y, (int)Math.Round( maxWidth ), height
            );

            Renderer2D.Draw( _pixel, rectFull, Color.DarkGray );
            Renderer2D.Draw( _pixel, rectPartial, Color.White );
        }

        /// <summary>
        /// Draws the progress text.
        /// </summary>
        private void DrawProgressText()
        {
            Vector2 size = _font.MeasureString( _progressText );
            Vector2 pos = new Vector2( _pp.BackBufferWidth / 2 - size.X / 2, _pp.BackBufferHeight / 2 - size.Y / 2 );
            pos.Y -= _font.LineSpacing;

            Renderer2D.DrawString( _font, _progressText, pos, Color.White );
        }

        /// <summary>
        /// Disposes of this load state.
        /// </summary>
        public override void Dispose()
        {
            _pixel.Dispose();
        }

        /// <summary>
        /// Updates the load state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Update( GameTime gameTime )
        {
            // switch to the play state if we've loaded the initial world
            _progress = Controller.World.ChunkManager.ChunkCreationProgress;
            if ( _progress == 1.00f )
            {
                SwitchToPlayState();
            }
            
            // update the progress text
            _progressTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if ( _progressTime >= 0.50f )
            {
                _progressTime -= 0.50f;
                _progressText += ".";
                if ( _progressText.EndsWith( "...." ) )
                {
                    _progressText = InitialProgressText;
                }
            }
        }

        /// <summary>
        /// Draws the load state.
        /// </summary>
        /// <param name="gameTime">Frame time information.</param>
        public override void Draw( GameTime gameTime )
        {
            Controller.Engine.GraphicsDevice.Clear( Color.Black );
            Renderer2D.Begin();

            DrawProgressBar();
            DrawProgressText();

            Renderer2D.End();
        }
    }
}