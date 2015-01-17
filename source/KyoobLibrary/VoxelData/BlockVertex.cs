using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// TODO : Do we really need to know the face normal?
//        Maybe if we want to light up an area with a point light with the currently held torch?
namespace Kyoob.VoxelData
{
    /// <summary>
    /// Contains a custom vertex declaration for block vertices.
    /// </summary>
    public struct BlockVertex : IVertexType
    {
        /// <summary>
        /// The vertex declaration for block vertices.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration;

        /// <summary>
        /// Gets the vertex declaration for this vertex type.
        /// </summary>
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return BlockVertex.VertexDeclaration;
            }
        }

        /// <summary>
        /// The position of this vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The normal of this vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The texture coordinate of this vertex.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// The light level of this vertex.
        /// </summary>
        public float LightLevel;

        /// <summary>
        /// Creates a new block vertex.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="norm">The normal.</param>
        /// <param name="tc">The texture coordinate.</param>
        /// <param name="light">The light level.</param>
        public BlockVertex( Vector3 pos, Vector3 norm, Vector2 tc, float light )
        {
            Position = pos;
            Normal = norm;
            TextureCoordinate = tc;
            LightLevel = light;
        }

        /// <summary>
        /// Statically initializes the BlockVertex type.
        /// </summary>
        static BlockVertex()
        {
            var elems = VertexPositionNormalTexture.VertexDeclaration.GetVertexElements();
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(  0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
                new VertexElement( 12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
                new VertexElement( 24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
                new VertexElement( 32, VertexElementFormat.Single,  VertexElementUsage.TextureCoordinate, 1 )
            );
        }
    }
}