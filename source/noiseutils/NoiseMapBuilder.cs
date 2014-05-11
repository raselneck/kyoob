using System;
using System.Collections.Generic;
using LibNoise;

namespace Kyoob.NoiseUtils
{
    /// <summary>
    /// Base class for noise map builders.
    /// </summary>
    public abstract class NoiseMapBuilder
    {
        /// <summary>
        /// The callback to noise map generation.
        /// </summary>
        /// <param name="row">The current map.</param>
        protected delegate void NoiseMapCallback( int row );

        private NoiseMapCallback _callback;
        private int _destWidth;
        private int _destHeight;
        private NoiseMap _noiseMap;
        private IModule _module;

        /// <summary>
        /// Gets or sets the noise map callback.
        /// </summary>
        protected NoiseMapCallback Callback
        {
            get
            {
                return _callback;
            }
            set
            {
                _callback = value;
            }
        }

        /// <summary>
        /// Gets or sets the destination width.
        /// </summary>
        public int DestinationWidth
        {
            get
            {
                return _destWidth;
            }
            set
            {
                _destWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the destination height.
        /// </summary>
        public int DestinationHeight
        {
            get
            {
                return _destHeight;
            }
            set
            {
                _destHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the builder's source module.
        /// </summary>
        public IModule SourceModule
        {
            get
            {
                return _module;
            }
            set
            {
                _module = value;
            }
        }

        /// <summary>
        /// Gets or sets the noise map.
        /// </summary>
        public NoiseMap NoiseMap
        {
            get
            {
                return _noiseMap;
            }
            set
            {
                _noiseMap = value;
            }
        }

        /// <summary>
        /// Creates a new noise map builder.
        /// </summary>
        public NoiseMapBuilder()
        {
            _callback = null;
            _destWidth = 0;
            _destHeight = 0;
            _noiseMap = new NoiseMap();
            _module = null;
        }

        /// <summary>
        /// Builds the noise map.
        /// </summary>
        public abstract void Build();
    }
}