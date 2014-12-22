using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains directional light information.
    /// </summary>
    public class DirectionalLight : Light
    {
        private string _directionName;
        private Vector3 _direction; // to ensure normalized direction

        /// <summary>
        /// Gets or sets this light's direction.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = Vector3.Normalize( value );
            }
        }

        /// <summary>
        /// Creates a new directional light.
        /// </summary>
        /// <param name="directionName">The name of the variable for this light's direction.</param>
        /// <param name="colorName">The name of the variable for this light's color.</param>
        public DirectionalLight( string directionName, string colorName )
            : base( colorName )
        {
            _directionName = directionName;

            Direction = Vector3.Down + Vector3.Right + Vector3.Forward;
        }

        /// <summary>
        /// Applies this directional light to the given effect.
        /// </summary>
        /// <param name="effect">The effect.</param>
        public override void Apply( CustomEffect effect )
        {
            effect.SetParameter( _directionName, _direction );

            base.Apply( effect );
        }
    }
}