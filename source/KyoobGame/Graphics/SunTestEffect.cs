using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The custom effect wrapper for the Sun test effect.
    /// </summary>
    public sealed class SunTestEffect : LightedEffect
    {
        /// <summary>
        /// Gets the directional light in this effect.
        /// </summary>
        public DirectionalLight Light
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new sun test effect.
        /// </summary>
        public SunTestEffect()
            : base( Game.Instance.Content.Load<Effect>( "shaders/sun_test" ) )
        {
            // handle sprite sheet
            SpriteSheet.Instance.TextureChanged += OnSpriteSheetChanged;
            Effect.Parameters[ "spriteSheet" ].SetValue( SpriteSheet.Instance.Texture );

            // create the light
            // Light = new DirectionalLight( "lightDirection", "lightColor" );
            Light = SceneRenderer.Instance.AddDirectionalLight();
            Light.Direction = Vector3.Down + Vector3.Right + Vector3.Forward;
            Light.Color = Color.White;
        }

        /// <summary>
        /// Applies the directional light to this effect.
        /// </summary>
        public override void ApplyToEffect()
        {
            Light.Apply( this );

            base.ApplyToEffect();
        }

        /// <summary>
        /// The event callback for when the sprite sheet's texture is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnSpriteSheetChanged( object sender, EventArgs args )
        {
            var ss = sender as SpriteSheet;
            Effect.Parameters[ "spriteSheet" ].SetValue( ss.Texture );
        }
    }
}