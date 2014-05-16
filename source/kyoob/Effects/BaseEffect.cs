using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Effects
{
    /// <summary>
    /// Contains a base class for custom effect wrappers.
    /// </summary>
    public abstract class BaseEffect : IEffectMatrices
    {
        private Effect _effect;
        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        /// <summary>
        /// Gets the underlying effect.
        /// </summary>
        public Effect Effect
        {
            get
            {
                return _effect;
            }
        }

        /// <summary>
        /// Gets or sets the effect's world matrix.
        /// </summary>
        public Matrix World
        {
            get
            {
                return _world;
            }
            set
            {
                _world = value;
                _effect.Parameters[ "_world" ].SetValue( _world );
            }
        }

        /// <summary>
        /// Gets or sets the effect's view matrix.
        /// </summary>
        public Matrix View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
                _effect.Parameters[ "_view" ].SetValue( _view );
            }
        }

        /// <summary>
        /// Gets or sets the effect's projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return _projection;
            }
            set
            {
                _projection = value;
                _effect.Parameters[ "_projection" ].SetValue( _projection );
            }
        }

        /// <summary>
        /// Creates a new base effect.
        /// </summary>
        /// <param name="effect">The effect to wrap.</param>
        public BaseEffect( Effect effect )
        {
            _effect = effect;
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.Identity;
        }

        /// <summary>
        /// Sets the current technique.
        /// </summary>
        /// <param name="name">The name of the technique.</param>
        public void SetTechnique( string name )
        {
            EffectTechnique technique = _effect.Techniques[ name ];
            if ( technique != null )
            {
                _effect.CurrentTechnique = technique;
            }
        }
    }
}