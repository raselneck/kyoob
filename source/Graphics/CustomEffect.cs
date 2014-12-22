using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// The base class for custom effect wrappers.
    /// </summary>
    public abstract class CustomEffect : IEffectMatrices
    {
        /// <summary>
        /// Gets the effect this custom effect is wrapping.
        /// </summary>
        public Effect Effect
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current technique being used.
        /// </summary>
        public EffectTechnique CurrentTechnique
        {
            get
            {
                return Effect.CurrentTechnique;
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new custom effect.
        /// </summary>
        /// <param name="effect">The XNA effect to use.</param>
        public CustomEffect( Effect effect )
        {
            Effect = effect;
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.Identity;
        }

        /// <summary>
        /// Applies parameters to the underlying XNA effect.
        /// </summary>
        public virtual void ApplyToEffect()
        {
            SetParameter( "projection", Projection );
            SetParameter( "view", View );
            SetParameter( "world", World );
        }

        /// <summary>
        /// Sets the current technique.
        /// </summary>
        /// <param name="name">The name of the technique.</param>
        public void SetTechnique( string name )
        {
            EffectTechnique technique = Effect.Techniques[ name ];
            if ( technique != null )
            {
                Effect.CurrentTechnique = technique;
            }
        }

        /// <summary>
        /// Sets the parameter with the given value.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The new value.</param>
        public void SetParameter( string name, object value )
        {
            // get the effect parameter
            var param = Effect.Parameters[ name ];
            if ( Effect.Parameters[ name ] == null )
            {
                //Debug.WriteLine( string.Format( "Could not find parameter named '{0}'.", name ) );
                return;
            }

            // set the parameter
            if ( value is bool )
            {
                param.SetValue( (bool)value );
            }
            else if ( value is float )
            {
                param.SetValue( (float)value );
            }
            else if ( value is Vector2 )
            {
                param.SetValue( (Vector2)value );
            }
            else if ( value is Vector3 )
            {
                param.SetValue( (Vector3)value );
            }
            else if ( value is Vector4 )
            {
                param.SetValue( (Vector4)value );
            }
            else if ( value is Matrix )
            {
                param.SetValue( (Matrix)value );
            }
            else if ( value is Texture2D )
            {
                param.SetValue( (Texture2D)value );
            }
            else
            {
                Debug.WriteLine( string.Format( "Unknown shader variable type '{0}'.", value.GetType().Name ) );
            }
        }
    }
}