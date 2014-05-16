using System;
using Microsoft.Xna.Framework;

#warning TODO : Try some kind of interpolation for smoothing.

namespace Kyoob.Terrain
{
    /// <summary>
    /// A custom terrain plane implementation.
    /// </summary>
    public class TerrainPlane
    {
        private LibNoise.Models.Plane _plane;
        private float _lowerX;
        private float _upperX;
        private float _lowerZ;
        private float _upperZ;

        /// <summary>
        /// Gets or sets the lower X value for the plane.
        /// </summary>
        public float LowerX
        {
            get
            {
                return _lowerX;
            }
            set
            {
                SetBounds( value, _upperX, _lowerZ, _upperZ );
            }
        }

        /// <summary>
        /// Gets or sets the upper X value for the plane.
        /// </summary>
        public float UpperX
        {
            get
            {
                return _upperX;
            }
            set
            {
                SetBounds( _lowerX, value, _lowerZ, _upperZ );
            }
        }

        /// <summary>
        /// Gets or sets the lower Z value for the plane.
        /// </summary>
        public float LowerZ
        {
            get
            {
                return _lowerZ;
            }
            set
            {
                SetBounds( _lowerX, _upperX, value, _upperZ );
            }
        }

        /// <summary>
        /// Gets or sets the upper Z value for the plane.
        /// </summary>
        public float UpperZ
        {
            get
            {
                return _upperZ;
            }
            set
            {
                SetBounds( _lowerX, _upperX, _lowerZ, value );
            }
        }

        /// <summary>
        /// Creates a new terrain plane.
        /// </summary>
        /// <param name="module">The source module to query.</param>
        public TerrainPlane( LibNoise.IModule module )
        {
            _plane = new LibNoise.Models.Plane( module );
            _lowerX = 0.0f;
            _upperX = 1.0f;
            _lowerZ = 0.0f;
            _upperZ = 1.0f;
        }

        /// <summary>
        /// Generates a height map.
        /// </summary>
        /// <param name="width">The height map's width.</param>
        /// <param name="height">The height map's height.</param>
        public float[,] GenerateHeightMap( int width, int height )
        {
            float destWidth = _upperX - _lowerX;
            float destDepth = _upperZ - _lowerZ;
            float deltaX    = destWidth / width;
            float deltaZ    = destDepth / height;
            float curX      = _lowerX;
            float curZ      = _lowerZ;

            // create heightmap and populate it
            float[ , ] map = new float[ width, height ];
            for ( int x = 0; x < width; ++x )
            {
                curZ = _lowerZ;
                for ( int z = 0; z < height; ++z )
                {
                    float value = (float)_plane.GetValue( curX, curZ );
                    map[ x, z ] = value;

                    curZ += deltaZ;
                }
                curX += deltaX;
            }
            return map;
        }

        /// <summary>
        /// Sets the plane's bounds.
        /// </summary>
        /// <param name="lowerX">The lower X value.</param>
        /// <param name="upperX">The upper X value.</param>
        /// <param name="lowerZ">The lower Z value.</param>
        /// <param name="upperZ">The upper Z value.</param>
        public void SetBounds( float lowerX, float upperX, float lowerZ, float upperZ )
        {
            // check the X bounds
            if ( upperX < lowerX )
            {
                _lowerX = upperX;
                _upperX = lowerX;
            }
            else
            {
                _lowerX = lowerX;
                _upperX = upperX;
            }

            // check the Z bounds
            if ( upperZ < lowerZ )
            {
                _lowerZ = upperZ;
                _upperZ = lowerZ;
            }
            else
            {
                _lowerZ = lowerZ;
                _upperZ = upperZ;
            }
        }
    }
}