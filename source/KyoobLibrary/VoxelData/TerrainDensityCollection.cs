using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains a collection of terrain densities.
    /// </summary>
    public class TerrainDensityCollection : ICollection<TerrainDensity>
    {
        private List<TerrainDensity> _densities;
        private Dictionary<BlockType, int> _indexCache;

        /// <summary>
        /// Gets the total number of items in this collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _densities.Count;
            }
        }

        /// <summary>
        /// Checks to see if this collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new terrain density collection.
        /// </summary>
        public TerrainDensityCollection()
        {
            _densities = new List<TerrainDensity>();
            _indexCache = new Dictionary<BlockType, int>();

            // add the default values
            // TODO : Do this dynamically
            _densities.Add( TerrainDensity.Find( BlockType.Air   ) );
            _densities.Add( TerrainDensity.Find( BlockType.Dirt  ) );
            _densities.Add( TerrainDensity.Find( BlockType.Sand  ) );
            _densities.Add( TerrainDensity.Find( BlockType.Snow  ) );
            _densities.Add( TerrainDensity.Find( BlockType.Stone ) );
            RebuildIndexCache();
        }

        /// <summary>
        /// Adds the given terrain density to the collection.
        /// </summary>
        /// <param name="item">The density to add.</param>
        public void Add( TerrainDensity item )
        {
            // ensure the new density doesn't overlap another
            for ( int i = 0; i < _densities.Count; ++i )
            {
                var density = _densities[ i ];
                if ( ( item.MinimumDensity >= density.MinimumDensity && item.MinimumDensity <  density.MaximumDensity ) ||
                     ( item.MaximumDensity >  density.MinimumDensity && item.MaximumDensity <= density.MaximumDensity ) )
                {
                    throw new Exception( "Cannot add the given density because it overlaps with an existing density." );
                }
            }

            // add the density
            _densities.Add( item );
            RebuildIndexCache();
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            _densities.Clear();
            Debug.WriteLine( "NOTE: Terrain density collection cleared." );
        }

        /// <summary>
        /// Checks to see if this collection contains the given terrain density.
        /// </summary>
        /// <param name="item">The terrain density to check for.</param>
        /// <returns></returns>
        public bool Contains( TerrainDensity item )
        {
            return _indexCache.ContainsKey( item.BlockType );
        }

        /// <summary>
        /// Copies this collection to the given array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The zero-based index at which to begin copying.</param>
        public void CopyTo( TerrainDensity[] array, int arrayIndex )
        {
            _densities.CopyTo( array, arrayIndex );
        }

        /// <summary>
        /// Finds the block type for the given noise density.
        /// </summary>
        /// <param name="value">The noise density.</param>
        /// <returns></returns>
        public BlockType FindBlockType( float value )
        {
            return FindBlockType( value, BlockType.Bedrock );
        }
        
        /// <summary>
        /// Finds the block type for the given noise density.
        /// </summary>
        /// <param name="value">The noise density.</param>
        /// <param name="fallback">The default block type to return if a density range is not found.</param>
        /// <returns></returns>
        public BlockType FindBlockType( float value, BlockType fallback )
        {
            // attempt to find the density value
            for ( int i = 0; i < _densities.Count; ++i )
            {
                var density = _densities[ i ];
                if ( value >= density.MinimumDensity && value <= density.MaximumDensity )
                {
                    return density.BlockType;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Removes the given terrain density from this collection.
        /// </summary>
        /// <param name="item">The terrain density.</param>
        /// <returns></returns>
        public bool Remove( TerrainDensity item )
        {
            int index = 0;
            if ( _indexCache.TryGetValue( item.BlockType, out index ) )
            {
                // remove info about the density
                _densities.RemoveAt( index );
                _indexCache.Remove( item.BlockType );

                // rebuilt index cache
                RebuildIndexCache();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the enumerator for this collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TerrainDensity> GetEnumerator()
        {
            return _densities.GetEnumerator();
        }

        /// <summary>
        /// Gets the non-generic enumerator for this collection.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _densities.GetEnumerator();
        }

        /// <summary>
        /// Rebuilds the index cache.
        /// </summary>
        private void RebuildIndexCache()
        {
            _indexCache.Clear();
            for ( int i = 0; i < _densities.Count; ++i )
            {
                _indexCache.Add( _densities[ i ].BlockType, i );
            }
        }
    }
}