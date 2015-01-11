using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;
using Kyoob.VoxelData;

// TODO : Take out the pre-lighting renderer for this and rename it to WorldRenderer

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains an easy way to render a scene.
    /// </summary>
    public sealed class SceneRenderer
    {
        /// <summary>
        /// The draw callback delegate type.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private delegate void RenderDelegate( GameTime gameTime );

        private static SceneRenderer _instance;
        private List<DirectionalLight> _dirLights;
        private List<PointLight> _pointLights;
        private List<SpotLight> _spotLights;
        private DirectionalLightEffect _dirLightEffect;
        private PointLightEffect _pointLightEffect;
        private SpotLightEffect _spotLightEffect;
        private DepthNormalEffect _depthNormalEffect;
        private MultilightEffect _multilightEffect;
        private RenderTarget2D _depthTarget;
        private RenderTarget2D _normalTarget;
        private RenderTarget2D _lightTarget;
        private GraphicsDevice _device;
        private Settings _settings;
        private RenderMode _renderMode;
        private LensFlare _lensFlare;
        private Model _lightMesh;
        private World _world;

        /// <summary>
        /// Gets the scene renderer instance.
        /// </summary>
        public static SceneRenderer Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new SceneRenderer();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the render mode for this scene renderer.
        /// </summary>
        public RenderMode RenderMode
        {
            get
            {
                return _renderMode;
            }
            set
            {
                _renderMode = value;
                switch ( _renderMode )
                {
                    case RenderMode.Normal:
                        RenderCallback = DrawScene;
                        break;
                    case RenderMode.LightMap:
                        RenderCallback = DrawLightMap;
                        break;
                    case RenderMode.NormalMap:
                        RenderCallback = DrawNormalMap;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the render callback.
        /// </summary>
        private RenderDelegate RenderCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new scene renderer.
        /// </summary>
        private SceneRenderer()
        {
            _device = Game.Instance.GraphicsDevice;
            _device.SamplerStates[ 0 ] = SamplerState.PointClamp;
            _device.BlendState = BlendState.Opaque;

            RenderMode = RenderMode.Normal; // use property
            _dirLights = new List<DirectionalLight>();
            _spotLights = new List<SpotLight>();
            _pointLights = new List<PointLight>();

            _world = World.Instance;
            _settings = Settings.Instance;
        }

        /// <summary>
        /// Loads the content used by the scene renderer.
        /// </summary>
        /// <param name="content">The content manager to use.</param>
        public void LoadContent( ContentManager content )
        {
            // create the lens flare
            _lensFlare = new LensFlare( Player.Instance.Camera, Vector3.Down );

            // create the render targets
            var viewport = _device.Viewport;
            _depthTarget  = new RenderTarget2D( _device, viewport.Width, viewport.Height, false, SurfaceFormat.Single, DepthFormat.Depth24 );
            _normalTarget = new RenderTarget2D( _device, viewport.Width, viewport.Height, false, SurfaceFormat.Color,  DepthFormat.Depth24 );
            _lightTarget  = new RenderTarget2D( _device, viewport.Width, viewport.Height, false, SurfaceFormat.Color,  DepthFormat.Depth24 );

            // load effects
            _dirLightEffect = new DirectionalLightEffect( _depthTarget, _normalTarget );
            _pointLightEffect = new PointLightEffect( _depthTarget, _normalTarget );
            _spotLightEffect = new SpotLightEffect( _depthTarget, _normalTarget );
            _depthNormalEffect = new DepthNormalEffect();
            _multilightEffect = new MultilightEffect( _lightTarget );

            // load light mesh
            _lightMesh = content.Load<Model>( "meshes/light_mesh" );

            // now the world can load content
            _world.LoadContent( content );
        }

        /// <summary>
        /// Creates a spot light in this scene.
        /// </summary>
        /// <returns></returns>
        public SpotLight AddSpotLight()
        {
            // create the light
            var light = new SpotLight( "lightPosition", "lightDirection", "lightColor", "lightConeAngle", "lightFalloff" );

            // record it and return it
            _spotLights.Add( light );
            return light;
        }

        /// <summary>
        /// Creates a point light in this scene.
        /// </summary>
        /// <returns></returns>
        public PointLight AddPointLight()
        {
            // create the light
            var light = new PointLight( "lightPosition", "lightColor", "lightAtten", "lightFalloff" );

            // record it and return it
            _pointLights.Add( light );
            return light;
        }

        /// <summary>
        /// Creates a directional light in this scene.
        /// </summary>
        /// <returns></returns>
        public DirectionalLight AddDirectionalLight()
        {
            // create the light
            var light = new DirectionalLight( "lightDirection", "lightColor" );

            // record it and return it
            _dirLights.Add( light );
            return light;
        }

        /// <summary>
        /// Updates the scene renderer.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update( GameTime gameTime )
        {
            Profiler.Start( "Scene Update" );


            // update the world
            _world.Update( gameTime );

            // update lens flare
            _lensFlare.Color = _world.SunLight.Color;
            _lensFlare.LightDirection = _world.SunLight.Direction;

            // set effect parameters
            var camera = Player.Instance.Camera;
            _depthNormalEffect.View = camera.View;
            _depthNormalEffect.Projection = camera.Projection;
            _multilightEffect.View = camera.View;
            _multilightEffect.Projection = camera.Projection;
            _multilightEffect.SetDefaultFogRange();


            Profiler.Stop( "Scene Update" );
        }

        /// <summary>
        /// Draws the scene renderer.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw( GameTime gameTime )
        {
            Profiler.Start( "Scene Draw" );


            DrawToDepthNormalMap( gameTime );
            DrawToLightMap( gameTime );
            RenderCallback( gameTime );


            Profiler.Stop( "Scene Draw" );
        }

        /// <summary>
        /// Draws to the depth and normal maps.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void DrawToDepthNormalMap( GameTime gameTime )
        {
            _device.SetRenderTargets( _normalTarget, _depthTarget );
            _device.Clear( Color.White );

            // use the depth render effect
            _depthNormalEffect.ApplyToEffect();
            _depthNormalEffect.CurrentTechnique.Passes[ 0 ].Apply();
            _world.Draw( gameTime );

            _device.SetRenderTarget( null );
        }

        /// <summary>
        /// Draws to the light map.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void DrawToLightMap( GameTime gameTime )
        {
            var camera = Player.Instance.Camera;
            var vp = camera.View * camera.Projection;
            var ivp = Matrix.Invert( vp );
            
            // set and clear render target
            _device.SetRenderTarget( _lightTarget );
            _device.Clear( Color.Black );

            // set render states
            GraphicsState.Push();
            _device.DepthStencilState = DepthStencilState.None;

            // draw lights (directional FIRST, then the others)
            if ( _dirLights.Count > 0 )
            {
                RenderDirectionalLights( ref vp, ref ivp );
            }
            if ( _pointLights.Count > 0 )
            {
                RenderPointLights( ref vp, ref ivp );
            }
            if ( _spotLights.Count > 0 )
            {
                RenderSpotLights( ref vp, ref ivp );
            }

            // restore graphics states
            GraphicsState.Pop();
            _device.SetRenderTarget( null );
        }

        /// <summary>
        /// Renders the spot lights to the current render target.
        /// </summary>
        /// <param name="vp">The view-projection matrix.</param>
        /// <param name="ivp">The inverse view-projection matrix.</param>
        private void RenderPointLights( ref Matrix vp, ref Matrix ivp )
        {
            _pointLightEffect.InverseViewProjection = ivp;
            _lightMesh.Meshes[ 0 ].MeshParts[ 0 ].Effect = _pointLightEffect.Effect;

            // draw lights
            for ( int i = 0; i < _pointLights.Count; ++i )
            {
                var light = _pointLights[ i ];
                light.Apply( _pointLightEffect );

                // calculate WVP
                var wvp = ( Matrix.CreateScale( light.Attenuation )
                          * Matrix.CreateTranslation( light.Position ) )
                          * vp;
                _pointLightEffect.WorldViewProjection = wvp;
                _pointLightEffect.ApplyToEffect();

                // if the camera is inside of the light, we need to reverse cull
                var dist = Vector3.Distance( Player.Instance.EyePosition, light.Position );
                if ( dist <= light.Attenuation )
                {
                    var rs = _device.RasterizerState;
                    _device.RasterizerState = RasterizerState.CullClockwise;

                    // draw the light
                    _lightMesh.Meshes[ 0 ].Draw();

                    _device.RasterizerState = rs;
                }
                else
                {
                    // draw the light
                    _lightMesh.Meshes[ 0 ].Draw();
                }
            }
        }

        /// <summary>
        /// Renders the spot lights to the current render target.
        /// </summary>
        /// <param name="vp">The view-projection matrix.</param>
        /// <param name="ivp">The inverse view-projection matrix.</param>
        private void RenderSpotLights( ref Matrix vp, ref Matrix ivp )
        {
            _spotLightEffect.InverseViewProjection = ivp;
            _lightMesh.Meshes[ 0 ].MeshParts[ 0 ].Effect = _spotLightEffect.Effect;

            // draw lights
            for ( int i = 0; i < _spotLights.Count; ++i )
            {
                var light = _spotLights[ i ];
                light.Apply( _spotLightEffect );

                // calculate WVP matrix
                var wvp = ( Matrix.CreateTranslation( light.Position ) ) * vp;
                _spotLightEffect.WorldViewProjection = wvp;
                _spotLightEffect.ApplyToEffect();

                // if the camera is inside of the light, we need to reverse cull
                var dist = Vector3.Distance( Player.Instance.EyePosition, light.Position );
                if ( dist <= _lightMesh.Meshes[ 0 ].BoundingSphere.Radius )
                {
                    var rs = _device.RasterizerState;
                    _device.RasterizerState = RasterizerState.CullClockwise;

                    // draw the spot light
                    _lightMesh.Meshes[ 0 ].Draw();

                    _device.RasterizerState = rs;
                }
                else
                {
                    // draw the spot light
                    _lightMesh.Meshes[ 0 ].Draw();
                }
            }
        }

        /// <summary>
        /// Renders the directional lights to the current render target.
        /// </summary>
        /// <param name="vp">The view-projection matrix.</param>
        /// <param name="ivp">The inverse view-projection matrix.</param>
        private void RenderDirectionalLights( ref Matrix vp, ref Matrix ivp )
        {
            _device.BlendState = BlendState.Opaque;

            // set effect parameters
            _dirLightEffect.WorldViewProjection = vp;
            _dirLightEffect.InverseViewProjection = ivp;
            _dirLightEffect.ApplyToEffect();

            // draw lights
            for ( int i = 0; i < _dirLights.Count; ++i )
            {
                var light = _dirLights[ i ];
                light.Apply( _dirLightEffect );

                // draw the directional light (use world instead of light map)
                _dirLightEffect.CurrentTechnique.Passes[ 0 ].Apply();
                _world.Draw( null );
            }

            _device.BlendState = BlendState.Additive;
        }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void DrawScene( GameTime gameTime )
        {
            _device.Clear( Color.CornflowerBlue );
            _lensFlare.Draw( gameTime );

            // draw the world again with the multi-lighting effect
            _multilightEffect.ApplyToEffect();
            _multilightEffect.CurrentTechnique.Passes[ 0 ].Apply();
            _world.Draw( gameTime );
        }

        /// <summary>
        /// Draws the light map.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void DrawLightMap( GameTime gameTime )
        {
            _device.Clear( Color.CornflowerBlue );
            _lensFlare.Draw( gameTime );

            GUI.Begin();
            GUI.SpriteBatch.Draw( _lightTarget, Vector2.Zero, Color.White );
            GUI.End();
        }

        /// <summary>
        /// Draws the light map.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void DrawNormalMap( GameTime gameTime )
        {
            _device.Clear( Color.CornflowerBlue );
            _lensFlare.Draw( gameTime );

            GUI.Begin();
            GUI.SpriteBatch.Draw( _normalTarget, Vector2.Zero, Color.White );
            GUI.End();
        }
    }
}