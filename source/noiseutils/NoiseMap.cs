using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kyoob.NoiseUtils
{
    /// <summary>
    /// Implements a noise map, a 2-dimensional array of floating-point values.
    /// </summary>
    public class NoiseMap
    {
        /// <summary>
        /// The raster's stride length must be a multiple of this constant.
        /// </summary>
        private const int RasterStrideBoundary = 4;

        /// <summary>
        /// The maximum width of a raster.
        /// </summary>
        private const int RasterMaxWidth = 32767;

        /// <summary>
        /// The maximum height of a raster.
        /// </summary>
        private const int RasterMaxHeight = 32767;

        private int _width;
        private int _height;
        private int _stride;
        private float[ , ] _noiseData;
        private float _border;

        /// <summary>
        /// Gets the map's width.
        /// </summary>
        public int Width
        {
            get
            {
                return _width;
            }
        }

        /// <summary>
        /// Gets the map's height.
        /// </summary>
        public int Height
        {
            get
            {
                return _height;
            }
        }

        /// <summary>
        /// Gets the map's stride.
        /// </summary>
        public int Stride
        {
            get
            {
                return _stride;
            }
        }

        /// <summary>
        /// Gets or sets the map's border value.
        /// </summary>
        public float BorderValue
        {
            get
            {
                return _border;
            }
            set
            {
                _border = value;
            }
        }

        /// <summary>
        /// Gets or sets the noise value at the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns></returns>
        public float this[ int x, int y ]
        {
            get
            {
                return _noiseData[ x, y ];
            }
            set
            {
                _noiseData[ x, y ] = value;
            }
        }

        /// <summary>
        /// Creates a new, empty noise map.
        /// </summary>
        public NoiseMap()
            : this( 0, 0 )
        {
        }

        /// <summary>
        /// Creates a new noise map.
        /// </summary>
        /// <param name="width">The map's width.</param>
        /// <param name="height">The map's height.</param>
        public NoiseMap( int width, int height )
        {
            SetSize( width, height );
            _border = 0.0f;
        }

        /// <summary>
        /// Calculates the stride amount for a noise map.
        /// </summary>
        /// <param name="width">The width of the noise map.</param>
        /// <returns></returns>
        private int CalculateStride( int width )
        {
            return ( ( width + RasterStrideBoundary - 1 ) / RasterStrideBoundary ) * RasterStrideBoundary;
        }

        /// <summary>
        /// Clears the noise map to a specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Clear( float value )
        {
            for ( int x = 0; x < _width; ++x )
            {
                for ( int y = 0; y < _height; ++y )
                {
                    _noiseData[ x, y ] = value;
                }
            }
        }

        /// <summary>
        /// Sets the new size for the noise map.
        /// </summary>
        /// <param name="width">The new width for the noise map.</param>
        /// <param name="height">The new height for the noise map.</param>
        public void SetSize( int width, int height )
        {
            // make sure bounds are legit
            if ( _width < 0 || _width > RasterMaxWidth || _height < 0 || _height > RasterMaxHeight )
            {
                string message = string.Format(
                    "Width must be in [{0},{1}] and height must be in the range [{2},{3}].",
                    0, RasterMaxWidth,
                    0, RasterMaxHeight
                );
                throw new ArgumentOutOfRangeException( message );
            }

            _width = width;
            _height = height;
            _noiseData = new float[ _width, _height ];
            _stride = CalculateStride( _width );
        }
    }
}