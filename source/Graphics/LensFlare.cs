using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;

// all code in this class (plus accompanying resources) taken and adapted from
// http://xbox.create.msdn.com/en-US/education/catalog/sample/lens_flare

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains an easy way to draw a lens flare.
    /// </summary>
    public sealed class LensFlare
    {
        /// <summary>
        /// The class representing each flare produced by the sun.
        /// </summary>
        private class Flare
        {
            /// <summary>
            /// Gets or sets the flare's position.
            /// </summary>
            public float Position
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the flare's scale.
            /// </summary>
            public float Scale
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the flare's color.
            /// </summary>
            public Color Color
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the flare's texture.
            /// </summary>
            public Texture2D Texture
            {
                get;
                private set;
            }

            /// <summary>
            /// Creates a new flare.
            /// </summary>
            /// <param name="position">The initial position.</param>
            /// <param name="scale">The initial scale.</param>
            /// <param name="color">The initial color.</param>
            /// <param name="texture">The texture.</param>
            public Flare( float position, float scale, Color color, Texture2D texture )
            {
                Position = position;
                Scale = scale;
                Color = color;
                Texture = texture;
            }
        }

        /// <summary>
        /// The constant used for how big the circular glow effect is.
        /// </summary>
        private const float GlowSize = 300;

        /// <summary>
        /// The constant used for how big of a rectangle should be examined for checking if
        /// the sun is behind terrain. Larger numbers will make the flare fade out more
        /// gradually as the sun falls behind terrain.
        /// </summary>
        private const float QuerySize = 125;

        /// <summary>
        /// Gets the inverse of the area of the query (aka query size squared).
        /// </summary>
        private const float InvQueryArea = 1.0f / ( QuerySize * QuerySize );

        /// <summary>
        /// The blend state used for disabling color writing.
        /// </summary>
        private static readonly BlendState BlendColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        private Vector2 _lightPosition;
        private Vector3 _lightDirection;
        private bool _isLightBehindCamera;
        private Texture2D _texGlow;
        private Texture2D _texFlare1;
        private Texture2D _texFlare2;
        private Texture2D _texFlare3;
        private OcclusionQuery _occlusionQuery;
        private VertexPositionColor[] _occlusionVertices;
        private bool _isOcclusionQueryActive;
        private float _occlusionAlpha;
        private Flare[] _flares;
        private BasicEffect _effect;

        /// <summary>
        /// Gets or sets the camera being monitored by this lens flare.
        /// </summary>
        public Camera Camera
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the direction the sun is casting its light in.
        /// </summary>
        public Vector3 LightDirection
        {
            get
            {
                return _lightDirection;
            }
            set
            {
                _lightDirection = Vector3.Normalize( value );
            }
        }

        /// <summary>
        /// Gets or sets the color of the lens flare.
        /// </summary>
        public Color Color
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not this lens flare is enabled.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new lens flare component.
        /// </summary>
        /// <param name="camera">The initial camera to monitor.</param>
        /// <param name="lightDirection">The initial light direction.</param>
        public LensFlare( Camera camera, Vector3 lightDirection )
        {
            var content = Game.Instance.Content;
            var device = Game.Instance.GraphicsDevice;

            Enabled = true;
            Camera = camera;
            LightDirection = lightDirection;
            Color = Color.White;
            _effect = new BasicEffect( device );
            _effect.View = Matrix.Identity;
            _effect.VertexColorEnabled = true;

            // load textures
            _texGlow = content.Load<Texture2D>( "textures/lens_glow" );
            _texFlare1 = content.Load<Texture2D>( "textures/lens_flare1" );
            _texFlare2 = content.Load<Texture2D>( "textures/lens_flare2" );
            _texFlare3 = content.Load<Texture2D>( "textures/lens_flare3" );

            // create flares
            _flares = new Flare[ 10 ]
            {
                new Flare( -0.5f, 0.7f, new Color(  50.0f,  25.0f,  50.0f ), _texFlare1 ),
                new Flare(  0.3f, 0.4f, new Color( 100.0f, 255.0f, 200.0f ), _texFlare1 ),
                new Flare(  1.2f, 1.0f, new Color( 100.0f,  50.0f,  50.0f ), _texFlare1 ),
                new Flare(  1.5f, 1.5f, new Color(  50.0f, 100.0f,  50.0f ), _texFlare1 ),

                new Flare( -0.3f, 0.7f, new Color( 200.0f,  50.0f,  50.0f ), _texFlare2 ),
                new Flare(  0.6f, 0.9f, new Color(  50.0f, 100.0f,  50.0f ), _texFlare2 ),
                new Flare(  0.7f, 0.4f, new Color(  50.0f, 200.0f, 200.0f ), _texFlare2 ),

                new Flare( -0.7f, 0.7f, new Color(  50.0f, 100.0f,  25.0f ), _texFlare3 ),
                new Flare(  0.0f, 0.6f, new Color(  25.0f,  25.0f,  25.0f ), _texFlare3 ),
                new Flare(  2.0f, 1.4f, new Color(  25.0f,  50.0f, 100.0f ), _texFlare3 )
            };

            // create occlusion data
            _occlusionVertices = new VertexPositionColor[ 4 ];
            _occlusionVertices[ 0 ].Position = new Vector3( -QuerySize / 2, -QuerySize / 2, -1.0f );
            _occlusionVertices[ 1 ].Position = new Vector3( QuerySize / 2, -QuerySize / 2, -1.0f );
            _occlusionVertices[ 2 ].Position = new Vector3( -QuerySize / 2, QuerySize / 2, -1.0f );
            _occlusionVertices[ 3 ].Position = new Vector3( QuerySize / 2, QuerySize / 2, -1.0f );
            _occlusionQuery = new OcclusionQuery( device );
        }

        /// <summary>
        /// Draws the lens flare.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw( GameTime gameTime )
        {
            if ( Enabled )
            {
                GraphicsState.Push();

                UpdateOcclusion();
                DrawGlow();
                DrawFlares();

                GraphicsState.Pop();
            }
        }

        /// <summary>
        /// Updates the occlusion query.
        /// </summary>
        private void UpdateOcclusion()
        {
            // because the sun is infinitely far away, we need to remove the translation component of our view
            Matrix occlusionView = Camera.View;
            occlusionView.Translation = Vector3.Zero;

            // project the light position into 2D space
            var viewport = Game.Instance.GraphicsDevice.Viewport;
            var projectedPosition = viewport.Project(
                -LightDirection,
                Camera.Projection,
                occlusionView,
                Matrix.Identity
            );

            // don't draw any flares if the light is behind the camera
            if ( projectedPosition.Z < 0.0f || projectedPosition.Z > 1.0f )
            {
                _isLightBehindCamera = true;
                return;
            }

            // update the light information
            _lightPosition.X = projectedPosition.X;
            _lightPosition.Y = projectedPosition.Y;
            _isLightBehindCamera = false;

            // check the occlusion query
            if ( _isOcclusionQueryActive )
            {
                // ensure the previous query has completed
                if ( !_occlusionQuery.IsComplete )
                {
                    return;
                }

                // check what percentage of the sun is visible
                _occlusionAlpha = Math.Min( _occlusionQuery.PixelCount * InvQueryArea, 1.0f );
            }

            // preserve current graphics settings and set ours
            var device = Game.Instance.GraphicsDevice;
            device.BlendState = BlendColorWriteDisable;
            device.DepthStencilState = DepthStencilState.DepthRead;

            // set our effect matrices
            _effect.World = Matrix.CreateTranslation( _lightPosition.X, _lightPosition.Y, 0.0f );
            _effect.Projection = Matrix.CreateOrthographicOffCenter(
                0.0f, viewport.Width,
                viewport.Height, 0.0f,
                0.0f, 1.0f
            );

            // issue the occlusion query
            _effect.CurrentTechnique.Passes[ 0 ].Apply();
            _occlusionQuery.Begin();
            device.DrawUserPrimitives( PrimitiveType.TriangleStrip, _occlusionVertices, 0, 2 );
            _occlusionQuery.End();
            _isOcclusionQueryActive = true;
        }

        /// <summary>
        /// Draws the lens flare glow.
        /// </summary>
        private void DrawGlow()
        {
            // ensure we need to draw the glow
            if ( _isLightBehindCamera || _occlusionAlpha <= 0.0f )
            {
                return;
            }

            var color = Color * _occlusionAlpha;
            var origin = new Vector2( _texGlow.Width, _texGlow.Height ) * 0.5f;
            var scale = GlowSize * 2.0f / _texGlow.Width;

            // draw the glow sprite with the GUI's sprite batch
            GUI.SpriteBatch.Begin();
            GUI.SpriteBatch.Draw(
                _texGlow, _lightPosition, null, color, 0.0f,
                origin, scale, SpriteEffects.None, 0.0f
            );
            GUI.SpriteBatch.End();
        }

        /// <summary>
        /// Draws the lens flare's flares.
        /// </summary>
        private void DrawFlares()
        {
            // ensure we need to draw flares
            if ( _isLightBehindCamera || _occlusionAlpha <= 0.0f )
            {
                return;
            }

            var viewport = Game.Instance.GraphicsDevice.Viewport;
            var screenCenter = new Vector2( viewport.Width, viewport.Height ) * 0.5f;
            var flareVector = screenCenter - _lightPosition;

            // hijack the GUI's sprite batch
            GUI.SpriteBatch.Begin( 0, BlendState.Additive );
            for ( int i = 0; i < _flares.Length; ++i )
            {
                // calculate info about the flare
                var flare = _flares[ i ];
                var flarePosition = _lightPosition + flareVector * flare.Position;
                var flareColor = flare.Color.ToVector4();
                flareColor.W *= _occlusionAlpha;

                // draw the flare using its center as the origin
                var flareOrigin = new Vector2( flare.Texture.Width, flare.Texture.Height ) * 0.5f;
                GUI.SpriteBatch.Draw(
                    flare.Texture, flarePosition, null, new Color( flareColor ),
                    0.0f, flareOrigin, flare.Scale, SpriteEffects.None, 0.0f
                );
            }
            GUI.SpriteBatch.End();
        }
    }
}