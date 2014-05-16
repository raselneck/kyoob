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
        private List<BlockType> _types;
        private List<float> _levels;
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
            _levels = new List<float>();
            _types = new List<BlockType>();
        }

        /// <summary>
        /// Adds new block type level data.
        /// </summary>
        /// <param name="level">The height level.</param>
        /// <param name="type">The block type.</param>
        public void AddLevel( float level, BlockType type )
        {
            // make sure the level isn't registered
            if ( _levels.IndexOf( level ) != -1 )
            {
                string message = string.Format( "A block at level {0} already exists.", level );
                throw new ArgumentException( message );
            }

            // make sure the block isn't already registered
            if ( _types.IndexOf( type ) != -1 )
            {
                string message = string.Format( "A block with the type {0} already exists.", type );
                throw new ArgumentException( message );
            }

            // find the index we need to insert at
            int index;
            for ( index = 0; index < _levels.Count; ++index )
            {
                if ( level < _levels[ index ] )
                {
                    break;
                }
            }

            _types.Insert( index, type );
            _levels.Insert( index, level );
        }

        /// <summary>
        /// Gets the list of registered block types.
        /// </summary>
        /// <returns></returns>
        public List<BlockType> GetTypes()
        {
            return _types;
        }

        /// <summary>
        /// Gets the highest level.
        /// </summary>
        /// <returns></returns>
        public float GetHighestLevel()
        {
            return _levels[ _levels.Count - 1 ];
        }

        /// <summary>
        /// Gets the level for the given block type.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <returns></returns>
        public float GetLevelForType( BlockType type )
        {
            int index = _types.IndexOf( type );
            if ( index == -1 )
            {
                return -1.0f;
            }

            return _levels[ index ];
        }

        /// <summary>
        /// Gets the block type for the given level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public BlockType GetTypeForLevel( float level )
        {
            int index = 0;
            while ( level > _levels[ index ] )
            {
                ++index;
                if ( index == _levels.Count - 1 )
                {
                    break;
                }
            }
            return _types[ index ];
        }
    }
}