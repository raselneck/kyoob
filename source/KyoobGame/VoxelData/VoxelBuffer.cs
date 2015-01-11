using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Graphics;

// TODO : Use BlockVertex instead of VertexPositionNormalTexture (see http://msdn.microsoft.com/en-us/library/bb976065.aspx)
// TODO : Take out prelighting system and use block light levels instead with new shader

namespace Kyoob.VoxelData
{
    /// <summary>
    /// A (mostly?) thread-safe wrapper for an XNA vertex buffer.
    /// </summary>
    public sealed class VoxelBuffer : IDisposable
    {
        private VertexBuffer _vertexBuffer;
        private int _triangleCount;
        private object _mutex;

        /// <summary>
        /// Checks to see if this voxel buffer is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                bool isDisposed = false;
                lock ( _mutex )
                {
                    isDisposed = ( _vertexBuffer == null ) ? true : _vertexBuffer.IsDisposed;
                }
                return isDisposed;
            }
        }

        /// <summary>
        /// Checks to see if this voxel buffer has data.
        /// </summary>
        public bool HasData
        {
            get
            {
                bool hasData = false;
                lock ( _mutex )
                {
                    hasData = _vertexBuffer != null;
                }
                return hasData;
            }
        }

        /// <summary>
        /// Gets the mutex used by this voxel buffer.
        /// </summary>
        public object Mutex
        {
            get
            {
                return _mutex;
            }
        }

        /// <summary>
        /// Gets the underlying vertex buffer.
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get
            {
                VertexBuffer copy = null;
                lock ( _mutex )
                {
                    copy = _vertexBuffer;
                }
                return copy;
            }
        }

        /// <summary>
        /// Creates a new voxel buffer.
        /// </summary>
        public VoxelBuffer()
        {
            _mutex = new object();
            _vertexBuffer = null;
        }

        /// <summary>
        /// Ensures this voxel buffer is disposed.
        /// </summary>
        ~VoxelBuffer()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of this voxel buffer
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Sets the underlying vertex buffer's data.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="data">The data to set.</param>
        public void SetData( VertexPositionNormalTexture[] data )
        {
            lock ( _mutex )
            {
                DisposeNoLock();
                if ( data.Length > 0 )
                {
                    if ( _vertexBuffer == null )
                    {
                        _vertexBuffer = new VertexBuffer( Game.Instance.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, data.Length, BufferUsage.None );
                    }
                    _vertexBuffer.SetData( data, 0, data.Length );
                    _triangleCount = data.Length / 3;
                }
            }
        }

        /// <summary>
        /// Draws this voxel buffer, assuming that an effect is currently active.
        /// </summary>
        public void Draw()
        {
            if ( _vertexBuffer == null )
            {
                return;
            }
            lock ( _vertexBuffer )
            {
                var device = Game.Instance.GraphicsDevice;
                device.SetVertexBuffer( _vertexBuffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, _triangleCount );
            }
        }

        /// <summary>
        /// Disposes of this voxel buffer.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose( bool disposing )
        {
            lock ( _mutex )
            {
                DisposeNoLock();
            }
        }

        /// <summary>
        /// Disposes of the underlying vertex buffer without locking the mutex.
        /// </summary>
        private void DisposeNoLock()
        {
            if ( _vertexBuffer != null )
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }
        }
    }
}