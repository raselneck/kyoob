using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Entities;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The base class for lighting effects.
    /// </summary>
    internal abstract class LightEffect : CustomEffect
    {
        /// <summary>
        /// Gets or sets the depth map.
        /// </summary>
        public Texture2D DepthMap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        public Texture2D NormalMap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the world view projection matrix.
        /// </summary>
        public Matrix WorldViewProjection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the inverse view projection matrix.
        /// </summary>
        public Matrix InverseViewProjection
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new light effect.
        /// </summary>
        /// <param name="effect">The XNA effect to wrap.</param>
        /// <param name="depthMap">The depth map to use.</param>
        /// <param name="normalMap">The normal map to use.</param>
        public LightEffect( Effect effect, Texture2D depthMap, Texture2D normalMap )
            : base( effect )
        {
            DepthMap = depthMap;
            NormalMap = normalMap;
        }

        /// <summary>
        /// Applies parameters to the underlying XNA effect.
        /// </summary>
        public override void ApplyToEffect()
        {
            var viewport = Game.Instance.GraphicsDevice.Viewport;
            SetParameter( "viewportWidth", (float)viewport.Width );
            SetParameter( "viewportHeight", (float)viewport.Height );
            SetParameter( "worldViewProjection", WorldViewProjection );
            SetParameter( "invViewProjection", InverseViewProjection );
            SetParameter( "depthTexture", DepthMap );
            SetParameter( "normalTexture", NormalMap );

            base.ApplyToEffect();
        }
    }

    /// <summary>
    /// Contains directional light effect information.
    /// </summary>
    internal class DirectionalLightEffect : LightEffect
    {
        /// <summary>
        /// Creates a directional light effect.
        /// </summary>
        /// <param name="depthMap">The depth map to use.</param>
        /// <param name="normalMap">The normal map to use.</param>
        public DirectionalLightEffect( Texture2D depthMap, Texture2D normalMap )
            : base( Game.Instance.Content.Load<Effect>( "shaders/dir_light" ), depthMap, normalMap )
        {
        }
    }

    /// <summary>
    /// Contains point light effect information.
    /// </summary>
    internal class PointLightEffect : LightEffect
    {
        /// <summary>
        /// Creates a point light effect.
        /// </summary>
        /// <param name="depthMap">The depth map to use.</param>
        /// <param name="normalMap">The normal map to use.</param>
        public PointLightEffect( Texture2D depthMap, Texture2D normalMap )
            : base( Game.Instance.Content.Load<Effect>( "shaders/point_light" ), depthMap, normalMap )
        {
        }
    }

    /// <summary>
    /// Contains spot light effect information.
    /// </summary>
    internal class SpotLightEffect : LightEffect
    {
        /// <summary>
        /// Creates a spot light effect.
        /// </summary>
        /// <param name="depthMap">The depth map to use.</param>
        /// <param name="normalMap">The normal map to use.</param>
        public SpotLightEffect( Texture2D depthMap, Texture2D normalMap )
            : base( Game.Instance.Content.Load<Effect>( "shaders/spot_light" ), depthMap, normalMap )
        {
        }
    }
}