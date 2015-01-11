using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kyoob.Graphics
{
    /// <summary>
    /// Contains spot light information.
    /// </summary>
    public sealed class SpotLight : Light
    {
        private string _positionName;
        private string _directionName;
        private string _coneAngleName;
        private string _falloffName;
        private Vector3 _direction; // to ensure normalized direction

        /// <summary>
        /// Gets or sets this light's position.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

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
        /// Gets or sets this light's cone angle (in radians).
        /// </summary>
        public float ConeAngle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this light's falloff.
        /// </summary>
        public float Falloff
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new spot light.
        /// </summary>
        /// <param name="positionName">The name of the variable for this light's position.</param>
        /// <param name="colorName">The name of the variable for this light's color.</param>
        /// <param name="coneAngleName">The name of the variable for this light's cone angle.</param>
        /// <param name="falloffName">The name of the variable for this light's falloff.</param>
        public SpotLight( string positionName, string directionName, string colorName, string coneAngleName, string falloffName )
            : base( colorName )
        {
            _positionName = positionName;
            _directionName = directionName;
            _coneAngleName = coneAngleName;
            _falloffName = falloffName;


            Position = Vector3.Zero;
            Direction = Vector3.Down;
            ConeAngle = MathHelper.PiOver4;
            Falloff = 2.0f;
        }

        /// <summary>
        /// Applies this spot light to the given effect.
        /// </summary>
        /// <param name="effect">The effect.</param>
        public override void Apply( CustomEffect effect )
        {
            effect.SetParameter( _positionName, Position );
            effect.SetParameter( _directionName, Direction );
            effect.SetParameter( _coneAngleName, ConeAngle );
            effect.SetParameter( _falloffName, Falloff );

            base.Apply( effect );
        }
    }
}