using System;
using System.Collections.Generic;
using LibNoise.Models;

using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Kyoob.NoiseUtils
{
    /// <summary>
    /// A planar noise map builder.
    /// </summary>
    public class PlaneMapBuilder : NoiseMapBuilder
    {
        private double _boundLowerX;
        private double _boundLowerZ;
        private double _boundUpperX;
        private double _boundUpperZ;

        /// <summary>
        /// Gets or sets the lower X bound.
        /// </summary>
        public double LowerBoundX
        {
            get
            {
                return _boundLowerX;
            }
            set
            {
                _boundLowerX = value;
            }
        }

        /// <summary>
        /// Gets or sets the lower Z bound.
        /// </summary>
        public double LowerBoundZ
        {
            get
            {
                return _boundLowerZ;
            }
            set
            {
                _boundLowerZ = value;
            }
        }

        /// <summary>
        /// Gets or sets the upper X bound.
        /// </summary>
        public double UpperBoundX
        {
            get
            {
                return _boundUpperX;
            }
            set
            {
                _boundUpperX = value;
            }
        }

        /// <summary>
        /// Gets or sets the upper Z bound.
        /// </summary>
        public double UpperBoundZ
        {
            get
            {
                return _boundUpperZ;
            }
            set
            {
                _boundUpperZ = value;
            }
        }

        /// <summary>
        /// Creates a new plane map builder.
        /// </summary>
        public PlaneMapBuilder()
        {
            SetBounds( 0.0, 0.0, 0.0, 0.0 );
        }

        /// <summary>
        /// Ensures that the bounds are legit.
        /// </summary>
        private void EnsureBoundsAreLegit()
        {
            // swap lower x and upper x if they're inverted
            if ( _boundLowerX > _boundUpperX )
            {
                double temp = _boundUpperX;
                _boundUpperX = _boundLowerX;
                _boundLowerX = temp;
            }

            // swap lower z and upper z if they're inverted
            if ( _boundLowerZ > _boundUpperZ )
            {
                double temp = _boundUpperZ;
                _boundUpperZ = _boundLowerZ;
                _boundLowerZ = temp;
            }
        }

        /// <summary>
        /// Linearly interpolates two values.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="amount">The amount to lerp.</param>
        /// <returns></returns>
        private float LerpF( double a, double b, double amount )
        {
            return MathHelper.Lerp( (float)a, (float)b, (float)amount );
        }

        /// <summary>
        /// Sets the bounds of the builder.
        /// </summary>
        /// <param name="lowerX">The lower X bound.</param>
        /// <param name="upperX">The upper X bound.</param>
        /// <param name="lowerZ">The lower Z bound.</param>
        /// <param name="upperZ">The upper Z bound.</param>
        public void SetBounds( double lowerX, double upperX, double lowerZ, double upperZ )
        {
            // set bounds
            _boundLowerX = lowerX;
            _boundLowerZ = lowerZ;
            _boundUpperX = upperX;
            _boundUpperZ = upperZ;

            EnsureBoundsAreLegit();
        }

        /// <summary>
        /// Builds the plane map.
        /// </summary>
        public override void Build()
        {
            EnsureBoundsAreLegit();

            // create plane model and set the noise map size
            Plane plane = new Plane( SourceModule );
            NoiseMap.SetSize( DestinationWidth, DestinationHeight );

            double xExtent = _boundUpperX - _boundLowerX;
            double zExtent = _boundUpperZ - _boundLowerZ;
            double xDelta  = xExtent / (double)DestinationWidth;
            double zDelta  = zExtent / (double)DestinationHeight;
            double xCur    = _boundLowerX;
            double zCur    = _boundLowerZ;

            // fill the noise map with all of the values from the model
            for ( int z = 0; z < DestinationHeight; ++z )
            {
                xCur = _boundLowerX;

                for ( int x = 0; x < DestinationWidth; ++x )
                {
                    float value = (float)plane.GetValue( xCur, zCur );
                    NoiseMap[ x, z ] = value;
                    xCur += xDelta;
                }

                zCur += zDelta;
                if ( Callback != null )
                {
                    Callback( z );
                }
            }
        }
    }
}