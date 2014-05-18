using System;
using System.Collections.Generic;
using Kyoob.Blocks;

#warning TODO : Gradient level collision checking.

namespace Kyoob.Terrain
{
    /// <summary>
    /// Contains terrain level data.
    /// </summary>
    public sealed class TerrainLevels
    {
        /// <summary>
        /// A structure containing terrain gradient information.
        /// </summary>
        private struct TerrainGradient
        {
            /// <summary>
            /// The lower bound of this type's gradient section.
            /// </summary>
            public float LowerBound;

            /// <summary>
            /// The upper bound of this type's gradient section.
            /// </summary>
            public float UpperBound;

            /// <summary>
            /// The type.
            /// </summary>
            public BlockType Type;
        }

        private List<TerrainGradient> _values;
        private float _waterLevel;

        /// <summary>
        /// Gets or sets the water level.
        /// </summary>
        public float WaterLevel
        {
            get
            {
                return _waterLevel;
            }
            set
            {
                _waterLevel = value;
            }
        }

        /// <summary>
        /// Creates a new terrain level settings object.
        /// </summary>
        public TerrainLevels()
        {
            _values = new List<TerrainGradient>();
        }

        /// <summary>
        /// Sets a block type's bounds for terrain levels.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <param name="lower">The lower bounds.</param>
        /// <param name="upper">The upper bounds.</param>
        public void SetBounds( BlockType type, float lower, float upper )
        {
            // create the gradient
            TerrainGradient grad = new TerrainGradient()
            {
                LowerBound = lower,
                UpperBound = upper,
                Type = type
            };
            _values.Add( grad );
        }

        /// <summary>
        /// Gets the block type for the given noise value.
        /// </summary>
        /// <param name="value">The noise value.</param>
        /// <returns></returns>
        public BlockType GetType( float value )
        {
            // find the index of the gradient entry that the given value belongs to
            int index = -1;
            for ( int i = 0; i < _values.Count; ++i )
            {
                if ( value >= _values[ i ].LowerBound && value <= _values[ i ].UpperBound )
                {
                    index = i;
                    break;
                }
            }

            // if the index wasn't found, let's return bedrock so we'll know we fucked up
            if ( index == -1 )
            {
                return BlockType.Bedrock;
            }
            return _values[ index ].Type;
        }
    }
}