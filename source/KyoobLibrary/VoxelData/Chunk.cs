using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;

// TODO : Re-build voxel buffer on SetData

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains chunk data.
    /// </summary>
    public sealed class Chunk : IDisposable
    {
        private static ChunkBuilder _chunkBuilder = new ChunkBuilder();

        private ChunkData _data;
        private BoundingBox _bounds;
        private VoxelBuffer _terrain;
        private ChunkOctree _octree;
        private World _world;

        /// <summary>
        /// Gets the data within this chunk.
        /// </summary>
        public ChunkData Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Gets the center of this chunk.
        /// </summary>
        public Vector3 Index
        {
            get
            {
                return _data.Index;
            }
        }

        /// <summary>
        /// Gets the bounds of this chunk.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="world">The world this chunk belongs to.</param>
        /// <param name="index">The index of the chunk.</param>
        public Chunk( World world, Vector3 index )
            : this( world, index, true )
        {
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="world">The world this chunk belongs to.</param>
        /// <param name="index">The index of the chunk.</param>
        /// <param name="generateTerrain">True to generate some initial terrain, false to leave each block empty.</param>
        private Chunk( World world, Vector3 index, bool generateTerrain )
        {
            _world = world;
            _data = new ChunkData( index );
            _terrain = new VoxelBuffer();

            // create bounds
            var sizeOffs = ChunkData.SizeXZ * 0.5f - 0.5f;
            Vector3 boundsMin = new Vector3(
                index.X - sizeOffs,
                -0.5f,
                index.Z - sizeOffs
            );
            Vector3 boundsMax = new Vector3(
                index.X + sizeOffs,
                0.5f + ChunkData.SizeY,
                index.Z + sizeOffs
            );
            _bounds = new BoundingBox( boundsMin, boundsMax );
            _octree = new ChunkOctree( _bounds );

            // check if we need to populate
            if ( generateTerrain )
            {
                PopulateTerrain();
            }
        }

        /// <summary>
        /// Ensures this chunk is disposed.
        /// </summary>
        ~Chunk()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of this chunk.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Draws this chunk, assuming that an effect is currently active.
        /// </summary>
        public void Draw()
        {
            if ( _terrain.HasData )
            {
                _terrain.Draw();
            }
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this chunk.
        /// </summary>
        /// <param name="box">The bounds to check.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box )
        {
            return _octree.Collides( box );
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this chunk.
        /// </summary>
        /// <param name="box">The bounds to check.</param>
        /// <param name="collisions">The list of bounding boxes that the given box collides with.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box, ref List<BoundingBox> collisions )
        {
            return _octree.Collides( box, ref collisions );
        }

        /// <summary>
        /// Gets all of the distances of the intersections a ray makes in this chunk.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns></returns>
        public List<float> GetIntersectionDistances( Ray ray )
        {
            return _octree.GetIntersectionDistances( ray );
        }

        /// <summary>
        /// Gets all of the distances of the intersections a ray makes in this chunk.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="dists">The list of distances to populate.</param>
        /// <returns></returns>
        public void GetIntersectionDistances( Ray ray, ref List<float> dists )
        {
            _octree.GetIntersectionDistances( ray, ref dists );
        }

        /// <summary>
        /// Gets a textual representation of this chunk.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format( "{{Index={0}}}", Index );
        }

        /// <summary>
        /// Disposes of this chunk.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose( bool disposing )
        {
            _terrain.Dispose();
        }

        /// <summary>
        /// Creates the bounding box for a local block.
        /// </summary>
        /// <param name="x">The local X coordinate.</param>
        /// <param name="y">The local Y coordinate.</param>
        /// <param name="z">The local Z coordinate.</param>
        /// <param name="bounds">The bounds to populate.</param>
        private void CreateBlockBounds( int x, int y, int z, ref BoundingBox bounds )
        {
            // get block position
            Vector3 p;
            Position.LocalToWorld( Index, x, y, z, out p );
            Vector3 hvec = Vector3.One * 0.5f; // TODO : I think this is done elsewhere? See if it can be made into an actual thing somewhere

            // set corners
            bounds.Min = p - hvec;
            bounds.Max = p + hvec;
        }

        /// <summary>
        /// Populates this chunk's terrain data.
        /// </summary>
        private void PopulateTerrain()
        {
            Profiler.Start( "Chunk Population" );


            lock ( _data )
            {
                var terrain = Terrain.Instance;
                terrain.SetCurrentChunk( this );

                // go through for each terrain block
                for ( int x = 0; x < ChunkData.SizeXZ; ++x )
                {
                    for ( int z = 0; z < ChunkData.SizeXZ; ++z )
                    {
                        var lightingLevel = Block.MaximumLighting;

                        for ( int y = ChunkData.SizeY - 1; y >= 0; --y )
                        {
                            var isAboveEmpty = ( terrain.Query( x, y + 1, z ) == BlockType.Air );

                            // ensure we need to create face data for this block
                            _data[ x, y, z ] = new Block( terrain.Query( x, y, z ), this, lightingLevel );
                            var block = _data[ x, y, z ];
                            if ( block.TypeInfo.IsEmpty )
                            {
                                continue;
                            }

                            // if the block's type is dirt and there's nothing on top, then the block should be grass
                            if ( block.Type == BlockType.Dirt && isAboveEmpty )
                            {
                                _data[ x, y, z ] = new Block( BlockType.Grass, this, lightingLevel );
                                block = _data[ x, y, z ];
                            }

                            lightingLevel = Block.MinimumLighting;
                        }
                    }
                }

                // build the voxel buffer
                BuildVoxelBufferNoLockData( false );
            }


            Profiler.Stop( "Chunk Population" );
        }

        /// <summary>
        /// Builds the voxel buffer.
        /// </summary>
        /// <param name="needToSetCurrentChunk">True if we need to set this chunk as the current chunk for the terrain, false if not.</param>
        private void BuildVoxelBuffer( bool needToSetCurrentChunk )
        {
            lock ( _data )
            {
                BuildVoxelBufferNoLockData( needToSetCurrentChunk );
            }
        }

        /// <summary>
        /// Builds the voxel buffer without locking the chunk data.
        /// </summary>
        /// <param name="needToSetCurrentChunk">True if we need to set this chunk as the current chunk for the terrain, false if not.</param>
        private void BuildVoxelBufferNoLockData( bool needToSetCurrentChunk )
        {
            Profiler.Start( "Voxel Buffer Building" );


            lock ( _chunkBuilder )
            {
                _chunkBuilder.Reset();

                // get the terrain
                var terrain = Terrain.Instance;
                if ( needToSetCurrentChunk )
                {
                    terrain.SetCurrentChunk( this );
                }

                // go through for each terrain block
                var temp = new Block();
                var tempBounds = new BoundingBox();
                var blockCenter = new Vector3();
                var useSmoothLighting = Game.Instance.Settings.SmoothLighting;
                for ( int x = 0; x < ChunkData.SizeXZ; ++x )
                {
                    for ( int z = 0; z < ChunkData.SizeXZ; ++z )
                    {
                        var lighting = new BlockFaceLighting( Block.MaximumLighting );

                        for ( int y = ChunkData.SizeY - 1; y >= 0; --y )
                        {
                            // get block info
                            var block = _data[ x, y, z ];
                            var exposed = false;
                            if ( block.TypeInfo.IsEmpty )
                            {
                                continue;
                            }

                            // get the block's and center
                            Position.LocalToWorld( _data.Index, x, y, z, out blockCenter );

                            // get surrounding block light level
                            // TODO : We don't need to get a lot of these if smooth lighting is disabled
                            var right            = GetLightFromBlock( terrain, x + 1, y,     z     );
                            var rightTop         = GetLightFromBlock( terrain, x + 1, y + 1, z     );
                            var rightBottom      = GetLightFromBlock( terrain, x + 1, y - 1, z     );
                            var left             = GetLightFromBlock( terrain, x - 1, y,     z     );
                            var leftTop          = GetLightFromBlock( terrain, x - 1, y + 1, z     );
                            var leftBottom       = GetLightFromBlock( terrain, x - 1, y - 1, z     );
                            var back             = GetLightFromBlock( terrain, x,     y,     z + 1 );
                            var backLeft         = GetLightFromBlock( terrain, x - 1, y,     z + 1 );
                            var backRight        = GetLightFromBlock( terrain, x + 1, y,     z + 1 );
                            var backTop          = GetLightFromBlock( terrain, x,     y + 1, z + 1 );
                            var backTopLeft      = GetLightFromBlock( terrain, x - 1, y + 1, z + 1 );
                            var backTopRight     = GetLightFromBlock( terrain, x + 1, y + 1, z + 1 );
                            var backBottom       = GetLightFromBlock( terrain, x,     y - 1, z + 1 );
                            var backBottomLeft   = GetLightFromBlock( terrain, x - 1, y - 1, z + 1 );
                            var backBottomRight  = GetLightFromBlock( terrain, x + 1, y - 1, z + 1 );
                            var front            = GetLightFromBlock( terrain, x,     y,     z - 1 );
                            var frontLeft        = GetLightFromBlock( terrain, x - 1, y,     z - 1 );
                            var frontRight       = GetLightFromBlock( terrain, x + 1, y,     z - 1 );
                            var frontTop         = GetLightFromBlock( terrain, x,     y + 1, z - 1 );
                            var frontTopLeft     = GetLightFromBlock( terrain, x - 1, y + 1, z - 1 );
                            var frontTopRight    = GetLightFromBlock( terrain, x + 1, y + 1, z - 1 );
                            var frontBottom      = GetLightFromBlock( terrain, x,     y - 1, z - 1 );
                            var frontBottomLeft  = GetLightFromBlock( terrain, x - 1, y - 1, z - 1 );
                            var frontBottomRight = GetLightFromBlock( terrain, x + 1, y - 1, z - 1 );
                            var top              = GetLightFromBlock( terrain, x,     y + 1, z     );
                            var bottom           = GetLightFromBlock( terrain, x,     y - 1, z     );



                            // check to the right
                            temp.Type = terrain.Query( x + 1, y, z );
                            if ( temp.TypeInfo.IsEmpty )
                            {
                                lighting = new BlockFaceLighting( right );

                                // calculate smooth lighting
                                if ( useSmoothLighting )
                                {
                                    lighting.LowerLeft  = 0.25f * ( right + rightBottom + frontBottomRight + frontRight );
                                    lighting.LowerRight = 0.25f * ( right + rightBottom + backBottomRight  + backRight  );
                                    lighting.UpperRight = 0.25f * ( right + rightTop    + backTopRight     + backRight  );
                                    lighting.UpperLeft  = 0.25f * ( right + rightTop    + frontTopRight    + frontRight );
                                }

                                _chunkBuilder.AddFaceData( blockCenter, BlockFace.Right, block.Type, lighting );
                                exposed = true;
                            }

                            // check to the left
                            temp.Type = terrain.Query( x - 1, y, z );
                            if ( temp.TypeInfo.IsEmpty )
                            {
                                lighting = new BlockFaceLighting( left );

                                // calculate smooth lighting
                                if ( useSmoothLighting )
                                {
                                    lighting.LowerLeft  = 0.25f * ( left + leftBottom + backBottomLeft  + backLeft  );
                                    lighting.LowerRight = 0.25f * ( left + leftBottom + frontBottomLeft + frontLeft );
                                    lighting.UpperRight = 0.25f * ( left + leftTop    + frontTopLeft    + frontLeft );
                                    lighting.UpperLeft  = 0.25f * ( left + leftTop    + backTopLeft     + backLeft  );
                                }

                                _chunkBuilder.AddFaceData( blockCenter, BlockFace.Left, block.Type, lighting );
                                exposed = true;
                            }

                            // check in back
                            temp.Type = terrain.Query( x, y, z + 1 );
                            if ( temp.TypeInfo.IsEmpty )
                            {
                                lighting = new BlockFaceLighting( back );

                                // calculate smooth lighting
                                if ( useSmoothLighting )
                                {
                                    lighting.LowerLeft  = 0.25f * ( back + backBottom + backBottomRight + backRight );
                                    lighting.LowerRight = 0.25f * ( back + backBottom + backBottomLeft  + backLeft  );
                                    lighting.UpperRight = 0.25f * ( back + backTop    + backTopLeft     + backLeft  );
                                    lighting.UpperLeft  = 0.25f * ( back + backTop    + backTopRight    + backRight );
                                }

                                _chunkBuilder.AddFaceData( blockCenter, BlockFace.Back, block.Type, lighting );
                                exposed = true;
                            }

                            // check in front
                            temp.Type = terrain.Query( x, y, z - 1 );
                            if ( temp.TypeInfo.IsEmpty )
                            {
                                lighting = new BlockFaceLighting( front );

                                // calculate smooth lighting
                                if ( useSmoothLighting )
                                {
                                    lighting.LowerLeft  = 0.25f * ( front + frontBottom + frontBottomLeft  + frontLeft  );
                                    lighting.LowerRight = 0.25f * ( front + frontBottom + frontBottomRight + frontRight );
                                    lighting.UpperRight = 0.25f * ( front + frontTop    + frontTopRight    + frontRight );
                                    lighting.UpperLeft  = 0.25f * ( front + frontTop    + frontTopLeft     + frontLeft  );
                                }

                                _chunkBuilder.AddFaceData( blockCenter, BlockFace.Front, block.Type, lighting );
                                exposed = true;
                            }

                            // check above
                            temp.Type = terrain.Query( x, y + 1, z );
                            if ( temp.TypeInfo.IsEmpty )
                            {
                                lighting = new BlockFaceLighting( top );

                                // calculate smooth lighting
                                if ( useSmoothLighting )
                                {
                                    lighting.LowerLeft  = 0.25f * ( top + frontTop + frontTopLeft  + leftTop  );
                                    lighting.LowerRight = 0.25f * ( top + frontTop + frontTopRight + rightTop );
                                    lighting.UpperRight = 0.25f * ( top + backTop  + backTopRight  + rightTop );
                                    lighting.UpperLeft  = 0.25f * ( top + backTop  + backTopLeft   + leftTop  );
                                }

                                _chunkBuilder.AddFaceData( blockCenter, BlockFace.Top, block.Type, lighting );
                                exposed = true;
                            }

                            // check below
                            temp.Type = terrain.Query( x, y - 1, z );
                            if ( temp.TypeInfo.IsEmpty )
                            {
                                lighting = new BlockFaceLighting( bottom );

                                // calculate smooth lighting
                                if ( useSmoothLighting )
                                {
                                    lighting.LowerLeft  = 0.25f * ( bottom + backBottom  + backBottomLeft   + leftBottom  );
                                    lighting.LowerRight = 0.25f * ( bottom + backBottom  + backBottomRight  + rightBottom );
                                    lighting.UpperRight = 0.25f * ( bottom + frontBottom + frontBottomRight + rightBottom );
                                    lighting.UpperLeft  = 0.25f * ( bottom + frontBottom + frontBottomLeft  + leftBottom  );
                                }

                                _chunkBuilder.AddFaceData( blockCenter, BlockFace.Bottom, block.Type, lighting );
                                exposed = true;
                            }



                            // add the block's bounds to the octree if it's exposed
                            if ( exposed )
                            {
                                CreateBlockBounds( x, y, z, ref tempBounds );
                                _octree.Add( tempBounds );
                            }
                        }
                    }
                }

                // now build the terrain
                lock ( _terrain )
                {
                    _chunkBuilder.PopulateBuffer( _terrain );
                }
            }


            Profiler.Stop( "Voxel Buffer Building" );
        }

        /// <summary>
        /// Gets the light value from the block at the local coordinates.
        /// </summary>
        /// <param name="terrain">The terrain to use.</param>
        /// <param name="x">The relative local X coordinate.</param>
        /// <param name="y">The relative local Y coordinate.</param>
        /// <param name="z">The relative local Z coordinate.</param>
        private float GetLightFromBlock( Terrain terrain, int x, int y, int z )
        {
            // if we're within actual bounds, then we can just return the already calculated light
            if ( x >= 0 && x < ChunkData.SizeXZ &&
                 z >= 0 && z < ChunkData.SizeXZ &&
                 y >= 0 && y < ChunkData.SizeY )
            {
                return _data[ x, y, z ].Lighting;
            }

            // TODO : We need to query the world later when the terrain is modifiable

            // if we're not within bounds, then we need to estimate the light level
            float lighting = Block.MaximumLighting;
            for ( int toCheckY = ChunkData.SizeY - 1; toCheckY > y; --toCheckY )
            {
                if ( terrain.Query( x, toCheckY, z ) != BlockType.Air )
                {
                    lighting = Block.MinimumLighting;
                    break;
                }
            }
            return lighting;
        }
    }
}