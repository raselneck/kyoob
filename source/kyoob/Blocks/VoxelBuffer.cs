using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob.Blocks
{
    /// <summary>
    /// A class containing voxel buffer data.
    /// </summary>
    public sealed class VoxelBuffer : IDisposable
    {
        private VertexBuffer _vertices;
        private List<VertexPositionNormalTexture> _data;
        private HashSet<Vector3> _uniquePoints;
        private int _triangleCount;
        private int _vertexCount;

        /// <summary>
        /// Checks to see if this voxel buffer is on the GPU.
        /// </summary>
        public bool IsOnGPU
        {
            get
            {
                return _vertices != null;
            }
        }

        /// <summary>
        /// Checks to see if this voxel buffer is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _vertices == null || _triangleCount == 0;
            }
        }

        /// <summary>
        /// Creates a new voxel buffer.
        /// </summary>
        public VoxelBuffer()
        {
            _vertices = null;
            _data = new List<VertexPositionNormalTexture>();
            _uniquePoints = new HashSet<Vector3>();
        }

        /// <summary>
        /// Adds face data to the voxel buffer.
        /// </summary>
        /// <param name="data">The data to add.</param>
        public void AddFaceData( VertexPositionNormalTexture[] data )
        {
            if ( data.Length != Cube.VerticesPerFace )
            {
                throw new ArgumentException( "Face data must contain " + Cube.VerticesPerFace + " vertices." );
            }

            // get unique points
            for ( int i = 0; i < data.Length; ++i )
            {
                if ( !_uniquePoints.Contains( data[ i ].Position ) )
                {
                    _uniquePoints.Add( data[ i ].Position );
                }
            }

            _data.AddRange( data );
            _triangleCount += Cube.TrianglesPerFace;
            _vertexCount += Cube.VerticesPerFace;
        }

        /// <summary>
        /// Clears out all data from the voxel buffer.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _triangleCount = 0;
            _vertexCount = 0;
            _uniquePoints.Clear();
        }

        /// <summary>
        /// Compiles the voxel buffer's data so that it can be sent to the GPU.
        /// </summary>
        /// <param name="device">The device to send the data to.</param>
        public void Compile( GraphicsDevice device )
        {
            if ( IsOnGPU )
            {
                return;
            }
            if ( _vertexCount > 0 && !device.IsDisposed )
            {
                _vertices = new VertexBuffer( device, VertexPositionNormalTexture.VertexDeclaration, _vertexCount, BufferUsage.None );
                _vertices.SetData<VertexPositionNormalTexture>( _data.ToArray() );
            }
        }

        /// <summary>
        /// Disposes of this voxel buffer.
        /// </summary>
        public void Dispose()
        {
            if ( _vertices != null )
            {
                _vertices.Dispose();
                _vertices = null;
                _data.Clear();
            }
        }

        /// <summary>
        /// Draws this voxel buffer onto a graphics device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect to render with.</param>
        public void Draw( GraphicsDevice device, BaseEffect effect )
        {
            if ( _vertices != null && _triangleCount > 0 )
            {
                device.SetVertexBuffer( _vertices );
                foreach ( EffectPass pass in effect.Effect.CurrentTechnique.Passes )
                {
                    pass.Apply();
                    device.DrawPrimitives( PrimitiveType.TriangleList, 0, _triangleCount );
                }
                device.SetVertexBuffer( null );
            }
        }
    }
}