using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;

// TODO : Incorporate block light level in saved files?

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains chunk data.
    /// </summary>
    public sealed class Chunk : IDisposable
    {
        private ChunkData _data;
        private BoundingBox _bounds;
        private VoxelBuffer _terrain;
        private ChunkOctree _octree;

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
        public Vector3 Center
        {
            get
            {
                return _data.Center;
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
        /// <param name="center">The center of the chunk.</param>
        public Chunk( Vector3 center )
            : this( center, true )
        {
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="generateTerrain">True to generate some initial terrain, false to leave each block empty.</param>
        private Chunk( Vector3 center, bool generateTerrain )
        {
            _data = new ChunkData( center );
            _terrain = new VoxelBuffer();

            // create bounds
            var sizeOffs = ChunkData.Size * 0.5f - 0.5f;
            Vector3 boundsMin = new Vector3(
                center.X - sizeOffs,
                center.Y - sizeOffs,
                center.Z - sizeOffs
            );
            Vector3 boundsMax = new Vector3(
                center.X + sizeOffs,
                center.Y + sizeOffs,
                center.Z + sizeOffs
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
        /// Writes a chunk to a stream.
        /// </summary>
        /// <param name="bw">The binary writer to use.</param>
        /// <param name="chunk">The chunk to write.</param>
        public static void Write( BinaryWriter bw, Chunk chunk )
        {
            // write chunk center
            bw.Write( chunk.Data.Center.X );
            bw.Write( chunk.Data.Center.Y );
            bw.Write( chunk.Data.Center.Z );

            // write chunk blocks
            for ( int x = 0; x < ChunkData.Size; ++x )
            {
                for ( int y = 0; y < ChunkData.Size; ++y )
                {
                    for ( int z = 0; z < ChunkData.Size; ++z )
                    {
                        bw.Write( (byte)chunk.Data[ x, y, z ].Type );
                    }
                }
            }
        }

        /// <summary>
        /// Reads a chunk from a stream.
        /// </summary>
        /// <param name="br">The binary reader to use.</param>
        public static Chunk Read( BinaryReader br )
        {
            // read chunk center
            var center = new Vector3();
            center.X = br.ReadSingle();
            center.Y = br.ReadSingle();
            center.Z = br.ReadSingle();

            // create the chunk
            var chunk = new Chunk( center, false );

            // read chunk blocks
            for ( int x = 0; x < ChunkData.Size; ++x )
            {
                for ( int y = 0; y < ChunkData.Size; ++y )
                {
                    for ( int z = 0; z < ChunkData.Size; ++z )
                    {
                        chunk.Data[ x, y, z ] = new Block( (BlockType)br.ReadByte(), chunk );
                    }
                }
            }

            return chunk;
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
            Position.LocalToWorld( _data.Center, x, y, z, out p );
            Vector3 hvec = Vector3.One * 0.5f; // TODO : I think this is done elsewhere? See if it can be made into an actual thing somewhere

            // set corners
            bounds.Min = p - hvec;
            bounds.Max = p + hvec;
        }

        /// <summary>
        /// Populates this chunk's terrain data.
        /// </summary>
        public void PopulateTerrain()
        {
            Profiler.Start( "Chunk Population" );


            lock ( _data )
            {
                TerrainGenerator generator = TerrainGenerator.Instance;
                generator.SetCurrentChunk( this );

                // go through for each terrain block
                for ( int x = 0; x < ChunkData.Size; ++x )
                {
                    for ( int y = 0; y < ChunkData.Size; ++y )
                    {
                        for ( int z = 0; z < ChunkData.Size; ++z )
                        {
                            // ensure we need to create face data for this block
                            _data[ x, y, z ] = new Block( generator.Query( x, y, z ), this );
                            var block = _data[ x, y, z ];
                            if ( block.TypeInfo.IsEmpty )
                            {
                                continue;
                            }

                            // if the block's type is dirt and there's nothing on top, then the block should be grass
                            if ( block.Type == BlockType.Dirt && generator.Query( x, y + 1, z ) == BlockType.Air )
                            {
                                _data[ x, y, z ] = new Block( BlockType.Grass, this );
                                block = _data[ x, y, z ];
                            }
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
        /// <param name="needToSetCurrentChunk">True if we need to set this chunk as the current chunk for the terrain generator, false if not.</param>
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
        /// <param name="needToSetCurrentChunk">True if we need to set this chunk as the current chunk for the terrain generator, false if not.</param>
        private void BuildVoxelBufferNoLockData( bool needToSetCurrentChunk )
        {
            // get the terrain generator
            TerrainGenerator generator = TerrainGenerator.Instance;
            if ( needToSetCurrentChunk )
            {
                generator.SetCurrentChunk( this );
            }

            // go through for each terrain block
            var builder = new ChunkBuilder(); // TODO : Can we make this static and reset it every time a chunk uses it?
            var temp = new Block();
            var tempBounds = new BoundingBox();
            var tempCenter = new Vector3();
            for ( int x = 0; x < ChunkData.Size; ++x )
            {
                for ( int y = 0; y < ChunkData.Size; ++y )
                {
                    for ( int z = 0; z < ChunkData.Size; ++z )
                    {
                        // get block info
                        var block = _data[ x, y, z ];
                        var exposed = false;
                        if ( block.TypeInfo.IsEmpty )
                        {
                            continue;
                        }

                        // get the block's and center
                        Position.LocalToWorld( _data.Center, x, y, z, out tempCenter );

                        // check above
                        temp.Type = generator.Query( x, y + 1, z );
                        if ( temp.TypeInfo.IsEmpty )
                        {
                            builder.AddFaceData( tempCenter, BlockFace.Top, block.Type );
                            exposed = true;
                        }

                        // check below
                        temp.Type = generator.Query( x, y - 1, z );
                        if ( temp.TypeInfo.IsEmpty )
                        {
                            builder.AddFaceData( tempCenter, BlockFace.Bottom, block.Type );
                            exposed = true;
                        }

                        // check to the left
                        temp.Type = generator.Query( x - 1, y, z );
                        if ( temp.TypeInfo.IsEmpty )
                        {
                            builder.AddFaceData( tempCenter, BlockFace.Left, block.Type );
                            exposed = true;
                        }

                        // check to the right
                        temp.Type = generator.Query( x + 1, y, z );
                        if ( temp.TypeInfo.IsEmpty )
                        {
                            builder.AddFaceData( tempCenter, BlockFace.Right, block.Type );
                            exposed = true;
                        }

                        // check in front
                        temp.Type = generator.Query( x, y, z - 1 );
                        if ( temp.TypeInfo.IsEmpty )
                        {
                            builder.AddFaceData( tempCenter, BlockFace.Front, block.Type );
                            exposed = true;
                        }

                        // check in back
                        temp.Type = generator.Query( x, y, z + 1 );
                        if ( temp.TypeInfo.IsEmpty )
                        {
                            builder.AddFaceData( tempCenter, BlockFace.Back, block.Type );
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
                builder.PopulateBuffer( _terrain );
            }
        }
    }
}