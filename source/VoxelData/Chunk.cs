using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;

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
        private Octree _octree;

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
            : this( center, false )
        {
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="center">The center of the chunk.</param>
        /// <param name="generateTerrain">True to generate some initial terrain, false to leave each block empty.</param>
        public Chunk( Vector3 center, bool generateTerrain )
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
            _octree = new Octree( _bounds );

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
                        chunk.Data[ x, y, z ].Type = (BlockType)br.ReadByte();
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
        /// Populates this chunk's terrain data.
        /// </summary>
        /// <param name="generator">The terrain to use for generation.</param>
        public void PopulateTerrain()
        {
            Profiler.Start( "Chunk Population" );


            lock ( _data )
            {
                TerrainGenerator generator = TerrainGenerator.Instance;
                var builder = new ChunkBuilder();
                generator.SetCurrentChunk( this );

                // go through for each terrain block
                Block temp = new Block();
                BoundingBox tempBounds = new BoundingBox();
                Vector3 tempCenter = new Vector3();
                for ( int x = 0; x < ChunkData.Size; ++x )
                {
                    for ( int y = 0; y < ChunkData.Size; ++y )
                    {
                        for ( int z = 0; z < ChunkData.Size; ++z )
                        {
                            // ensure we need to create face data for this block
                            Block block = _data[ x, y, z ];
                            block.Type = generator.Query( x, y, z );
                            if ( block.IsEmptyType )
                            {
                                continue;
                            }

                            // get the block's bounds and center
                            Position.LocalToWorld( _data.Center, x, y, z, out tempCenter );

                            // if the block's type is dirt and there's nothing on top, then the block should be grass
                            if ( block.Type == BlockType.Dirt && generator.Query( x, y + 1, z ) == BlockType.Air )
                            {
                                block.Type = BlockType.Grass;
                            }

                            // ********* now check the surrounding blocks *********

                            bool exposed = false;

                            // check above
                            temp.Type = generator.Query( x, y + 1, z );
                            if ( temp.IsEmptyType )
                            {
                                builder.AddFaceData( tempCenter, BlockFace.Top, block.Type );
                                exposed = true;
                            }

                            // check below
                            temp.Type = generator.Query( x, y - 1, z );
                            if ( temp.IsEmptyType )
                            {
                                builder.AddFaceData( tempCenter, BlockFace.Bottom, block.Type );
                                exposed = true;
                            }

                            // check to the left
                            temp.Type = generator.Query( x - 1, y, z );
                            if ( temp.IsEmptyType )
                            {
                                builder.AddFaceData( tempCenter, BlockFace.Left, block.Type );
                                exposed = true;
                            }

                            // check to the right
                            temp.Type = generator.Query( x + 1, y, z );
                            if ( temp.IsEmptyType )
                            {
                                builder.AddFaceData( tempCenter, BlockFace.Right, block.Type );
                                exposed = true;
                            }

                            // check in front
                            temp.Type = generator.Query( x, y, z - 1 );
                            if ( temp.IsEmptyType )
                            {
                                builder.AddFaceData( tempCenter, BlockFace.Front, block.Type );
                                exposed = true;
                            }

                            // check in back
                            temp.Type = generator.Query( x, y, z + 1 );
                            if ( temp.IsEmptyType )
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


            Profiler.Stop( "Chunk Population" );
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
            Vector3 hvec = Vector3.One * 0.5f;

            // set corners
            bounds.Min = p - hvec;
            bounds.Max = p + hvec;
        }
    }
}