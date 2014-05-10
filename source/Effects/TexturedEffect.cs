using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// A wrapper for a simple texturing effect.
    /// </summary>
    public class TexturedEffect : BaseEffect
    {
        private Texture2D _texture;

        /// <summary>
        /// Gets or sets the effect's texture.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                _texture = value;
                Effect.Parameters[ "_texture" ].SetValue( _texture );
            }
        }

        /// <summary>
        /// Creates a new texturing effect wrapper.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public TexturedEffect( Effect effect )
            : base( effect )
        {
            Texture = null;
        }
    }
}