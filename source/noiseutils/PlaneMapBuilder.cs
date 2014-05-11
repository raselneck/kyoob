using System;
using System.Collections.Generic;
using LibNoise.Models;

using MathHelper = Microsoft.Xna.Framework.MathHelper;

#warning TODO : For seamless, try lerping all 9 surrounding values instead of just 4.

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
        private bool _isSeamless;

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
        /// Gets or sets whether or not the map builder is seamless.
        /// </summary>
        public bool IsSeamless
        {
            get
            {
                return _isSeamless;
            }
            set
            {
                _isSeamless = value;
            }
        }

        /// <summary>
        /// Creates a new plane map builder.
        /// </summary>
        public PlaneMapBuilder()
        {
            SetBounds( 0.0, 0.0, 0.0, 0.0 );
            _isSeamless = false;
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
            double xDelta  = xExtent / DestinationWidth;
            double zDelta  = zExtent / DestinationHeight;
            double xCur    = _boundLowerX;
            double zCur    = _boundLowerZ;

            // fill the noise map with all of the values from the model
            for ( int z = 0; z < DestinationHeight; ++z )
            {
                xCur = _boundLowerX;

                for ( int x = 0; x < DestinationWidth; ++x )
                {
                    float value = 0.0f;
                    if ( _isSeamless )
                    {
                        // basically lerps values all around the current one together
                        double sw = plane.GetValue( xCur          , zCur           );
                        double se = plane.GetValue( xCur + xExtent, zCur           );
                        double nw = plane.GetValue( xCur          , zCur + zExtent );
                        double ne = plane.GetValue( xCur + xExtent, zCur + zExtent );

                        double xBlend = 1.0 - ( ( xCur - _boundLowerX ) / xExtent );
                        double zBlend = 1.0 - ( ( zCur - _boundLowerZ ) / zExtent );

                        double z0 = MathHelper.Lerp( (float)sw, (float)se, (float)xBlend );
                        double z1 = MathHelper.Lerp( (float)nw, (float)ne, (float)xBlend );
                        value     = MathHelper.Lerp( (float)z0, (float)z1, (float)zBlend );
                    }
                    else
                    {
                        value = (float)plane.GetValue( xCur, zCur );
                    }

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