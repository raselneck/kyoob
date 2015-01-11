using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Graphics;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains a way to build chunks into buffers.
    /// </summary>
    internal class ChunkBuilder
    {
        private static readonly Vector3 _vTLF;
        private static readonly Vector3 _vBLF;
        private static readonly Vector3 _vTRF;
        private static readonly Vector3 _vBRF;
        private static readonly Vector3 _vTLB;
        private static readonly Vector3 _vTRB;
        private static readonly Vector3 _vBLB;
        private static readonly Vector3 _vBRB;
        private Vector2 _tcTL;
        private Vector2 _tcTR;
        private Vector2 _tcBL;
        private Vector2 _tcBR;
        private List<VertexPositionNormalTexture> _vertices;

        /// <summary>
        /// Statically initializes the chunk builder.
        /// </summary>
        static ChunkBuilder()
        {
            _vTRB = new Vector3(  0.5f,  0.5f,  0.5f );
            _vTRF = new Vector3(  0.5f,  0.5f, -0.5f );
            _vBRB = new Vector3(  0.5f, -0.5f,  0.5f );
            _vBRF = new Vector3(  0.5f, -0.5f, -0.5f );
            _vTLB = new Vector3( -0.5f,  0.5f,  0.5f );
            _vTLF = new Vector3( -0.5f,  0.5f, -0.5f );
            _vBLB = new Vector3( -0.5f, -0.5f,  0.5f );
            _vBLF = new Vector3( -0.5f, -0.5f, -0.5f );
        }

        /// <summary>
        /// Creates a new chunk builder.
        /// </summary>
        public ChunkBuilder()
        {
            _vertices = new List<VertexPositionNormalTexture>();
        }

        /// <summary>
        /// Adds face data to an internal list.
        /// </summary>
        /// <param name="list">The list of face data.</param>
        /// <param name="center">The center of the current block.</param>
        /// <param name="face">The current block face.</param>
        /// <param name="type">The current block type.</param>
        public void AddFaceData( Vector3 center, BlockFace face, BlockType type )
        {
            // set texture coordinate info (avoid using "new")
            var texSize = SpriteSheet.Instance.TexCoordSize;
            var typeInfo = BlockTypeInfo.GetInfoForType( type );
            switch ( face )
            {
                case BlockFace.Back:
                case BlockFace.Front:
                case BlockFace.Left:
                case BlockFace.Right:
                    _tcTL = typeInfo.SideTextureCoordinate;
                    break;
                case BlockFace.Bottom:
                    _tcTL = typeInfo.BottomTextureCoordinate;
                    break;
                case BlockFace.Top:
                    _tcTL = typeInfo.TopTextureCoordinate;
                    break;
            }
            _tcTR.X = _tcTL.X + texSize.X;
            _tcTR.Y = _tcTL.Y;
            _tcBL.X = _tcTL.X;
            _tcBL.Y = _tcTL.Y + texSize.Y;
            _tcBR.X = _tcTL.X + texSize.X;
            _tcBR.Y = _tcTL.Y + texSize.Y;

            // now add to the vertex list
            switch ( face )
            {
                case BlockFace.Front:
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLF, Vector3.Forward, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLF, Vector3.Forward, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRF, Vector3.Forward, _tcTR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLF, Vector3.Forward, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRF, Vector3.Forward, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRF, Vector3.Forward, _tcTR ) );
                    break;
                case BlockFace.Back:
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLB, Vector3.Backward, _tcTR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRB, Vector3.Backward, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLB, Vector3.Backward, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLB, Vector3.Backward, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRB, Vector3.Backward, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRB, Vector3.Backward, _tcBL ) );
                    break;
                case BlockFace.Top:
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLF, Vector3.Up, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRB, Vector3.Up, _tcTR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLB, Vector3.Up, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLF, Vector3.Up, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRF, Vector3.Up, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRB, Vector3.Up, _tcTR ) );
                    break;
                case BlockFace.Bottom:
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLF, Vector3.Down, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLB, Vector3.Down, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRB, Vector3.Down, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLF, Vector3.Down, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRB, Vector3.Down, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRF, Vector3.Down, _tcTR ) );
                    break;
                case BlockFace.Left:
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLF, Vector3.Left, _tcTR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLB, Vector3.Left, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLF, Vector3.Left, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLB, Vector3.Left, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBLB, Vector3.Left, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTLF, Vector3.Left, _tcTR ) );
                    break;
                case BlockFace.Right:
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRF, Vector3.Right, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRF, Vector3.Right, _tcBL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRB, Vector3.Right, _tcBR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRB, Vector3.Right, _tcTR ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vTRF, Vector3.Right, _tcTL ) );
                    _vertices.Add( new VertexPositionNormalTexture( center + _vBRB, Vector3.Right, _tcBR ) );
                    break;
            }
        }

        /// <summary>
        /// Populates a voxel buffer with the current face data.
        /// </summary>
        /// <param name="buff">The buffer.</param>
        public void PopulateBuffer( VoxelBuffer buff )
        {
            buff.Dispose();
            if ( _vertices.Count > 0 )
            {
                buff.SetData( _vertices.ToArray() );
            }
        }

        /// <summary>
        /// Resets the chunk builder.
        /// </summary>
        public void Reset()
        {
            _vertices.Clear();
        }
    }
}