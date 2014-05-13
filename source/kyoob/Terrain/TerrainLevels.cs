using System;
using System.Collections.Generic;
using Kyoob.Blocks;

namespace Kyoob.Terrain
{
    /// <summary>
    /// Contains terrain level data.
    /// </summary>
    public sealed class TerrainLevels
    {
        /// <summary>
        /// Contains block type and height level pair data.
        /// </summary>
        private struct LevelTypePair
        {
            /// <summary>
            /// The height level for this block type to start at.
            /// </summary>
            public float Level;

            /// <summary>
            /// The block type for this level.
            /// </summary>
            public BlockType Type;

            /// <summary>
            /// Creates a new block type and level pair.
            /// </summary>
            /// <param name="level">The level.</param>
            /// <param name="type">The block type.</param>
            public LevelTypePair( float level, BlockType type )
            {
                Level = level;
                Type = type;
            }
        }

        private List<LevelTypePair> _levels;
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
            _levels = new List<LevelTypePair>();
            _levels.Add( new LevelTypePair( 1024.0f, BlockType.Air ) );
        }

        /// <summary>
        /// Adds new block type level data.
        /// </summary>
        /// <param name="level">The height level.</param>
        /// <param name="type">The block type.</param>
        public void AddLevel( float level, BlockType type )
        {
            LevelTypePair pair = new LevelTypePair( level, type );

            // get the index of the data pair that lies above this new pair
            int index;
            for ( index = 0; index < _levels.Count; ++index )
            {
                if ( _levels[ index ].Level > pair.Level )
                {
                    break;
                }

                // we can't have two block types at the same exact level
                if ( _levels[ index ].Level == pair.Level )
                {
                    throw new ArgumentException( "Only one block type per exact level can be stored." );
                }
            }

            _levels.Insert( index, pair );
        }

        /// <summary>
        /// Gets the maximum level for the given block type.
        /// </summary>
        /// <param name="type">The type to get the level for.</param>
        /// <returns></returns>
        public float GetLevel( BlockType type )
        {
            int index = 0;
            while ( type != _levels[ index ].Type )
            {
                ++index;
                if ( index == _levels.Count - 1 )
                {
                    break;
                }
            }
            return _levels[ index ].Level;
        }

        /// <summary>
        /// Gets the block type for the given height.
        /// </summary>
        /// <param name="height">The height to get the block data for.</param>
        /// <returns></returns>
        public BlockType GetBlockType( float height )
        {
            int index = 0;
            while ( height > _levels[ index ].Level )
            {
                ++index;
                if ( index == _levels.Count - 1 )
                {
                    break;
                }
            }
            return _levels[ index ].Type;

            /*
            int index;
            for ( index = 0; index < _levels.Count; ++index )
            {
                if ( height <= _levels[ index ].Level )
                {
                    break;
                }
            }

            if ( index == _levels.Count )
            {
                --index;
            }
            return _levels[ index ].Type;
            */
        }
    }
}