using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Kyoob.Graphics;

namespace Kyoob.VoxelData
{
    /// <summary>
    /// The base class for block type information.
    /// </summary>
    public abstract class BlockTypeInfo
    {
        private static Dictionary<BlockType, BlockTypeInfo> _infoMap;

        /// <summary>
        /// Gets the block type this information is for.
        /// </summary>
        public abstract BlockType BlockType
        {
            get;
        }

        /// <summary>
        /// Checks to see if this block type is solid.
        /// </summary>
        public virtual bool IsSolid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Checks to see if this block type is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return !IsSolid;
            }
        }

        /// <summary>
        /// Gets the texture coordinates of the side of this block type.
        /// </summary>
        public virtual Vector2 SideTextureCoordinate
        {
            get
            {
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Gets the texture coordinates of the top of this block type.
        /// </summary>
        public virtual Vector2 TopTextureCoordinate
        {
            get
            {
                return SideTextureCoordinate;
            }
        }

        /// <summary>
        /// Gets the texture coordinates of the bottom of this block type.
        /// </summary>
        public virtual Vector2 BottomTextureCoordinate
        {
            get
            {
                return SideTextureCoordinate;
            }
        }

        /// <summary>
        /// Gets the minimum density for this block type.
        /// </summary>
        public virtual float MinimumDensity
        {
            get
            {
                return float.NegativeInfinity;
            }
        }

        /// <summary>
        /// Gets the maximum density for this block type.
        /// </summary>
        public virtual float MaximumDensity
        {
            get
            {
                return float.NegativeInfinity;
            }
        }

        /// <summary>
        /// Statically initializes the block type information class.
        /// </summary>
        static BlockTypeInfo()
        {
            // TODO : Load all of this dynamically
            _infoMap = new Dictionary<BlockType, BlockTypeInfo>();
            _infoMap.Add( BlockType.Air, new AirBlockTypeInfo() );
            _infoMap.Add( BlockType.Bedrock, new BedrockBlockTypeInfo() );
            _infoMap.Add( BlockType.Dirt, new DirtBlockTypeInfo() );
            _infoMap.Add( BlockType.Grass, new GrassBlockTypeInfo() );
            _infoMap.Add( BlockType.Sand, new SandBlockTypeInfo() );
            _infoMap.Add( BlockType.Snow, new SnowBlockTypeInfo() );
            _infoMap.Add( BlockType.Stone, new StoneBlockTypeInfo() );
            _infoMap.Add( BlockType.Void, new VoidBlockTypeInfo() );
            _infoMap.Add( BlockType.Water, new WaterBlockTypeInfo() );
        }

        /// <summary>
        /// Converts a texture index to the actual texture coordinates.
        /// </summary>
        /// <param name="x">The X index.</param>
        /// <param name="y">The Y index.</param>
        /// <returns></returns>
        protected Vector2 IndexToCoordinates( int x, int y )
        {
            return new Vector2(
                x * SpriteSheet.Instance.TexCoordSize.X,
                y * SpriteSheet.Instance.TexCoordSize.Y
            );
        }

        /// <summary>
        /// Finds the information for the given block type.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <returns></returns>
        public static BlockTypeInfo Find( BlockType type )
        {
            return _infoMap[ type ];
        }

        /// <summary>
        /// Contains information for air blocks.
        /// </summary>
        private class AirBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Air;
                }
            }
            public override bool IsSolid
            {
                get
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Contains information for bedrock blocks.
        /// </summary>
        private class BedrockBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Bedrock;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 0, 2 );
                }
            }
        }

        /// <summary>
        /// Contains information for dirt blocks.
        /// </summary>
        private class DirtBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Dirt;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 1, 0 );
                }
            }
            public override float MinimumDensity
            {
                get
                {
                    return 0.420f;
                }
            }
            public override float MaximumDensity
            {
                get
                {
                    return 1.000f;
                }
            }
        }

        /// <summary>
        /// Contains information for grass blocks.
        /// </summary>
        private class GrassBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Grass;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 2, 2 );
                }
            }
            public override Vector2 TopTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 2, 1 );
                }
            }
            public override Vector2 BottomTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 1, 0 );
                }
            }
            // Don't override densities because only top dirt blocks are grass blocks
        }

        /// <summary>
        /// Contains information for sand blocks.
        /// </summary>
        private class SandBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Sand;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 1, 1 );
                }
            }
            public override float MinimumDensity
            {
                get
                {
                    return 0.250f;
                }
            }
            public override float MaximumDensity
            {
                get
                {
                    return 0.420f;
                }
            }
        }

        /// <summary>
        /// Contains information for snow blocks.
        /// </summary>
        private class SnowBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Snow;
                }
            }
            public override bool IsSolid
            {
                get
                {
                    return true;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 3, 2 );
                }
            }
            public override Vector2 TopTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 3, 1 );
                }
            }
            public override Vector2 BottomTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 1, 0 );
                }
            }
        }

        /// <summary>
        /// Contains information for stone blocks.
        /// </summary>
        private class StoneBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Stone;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 0, 1 );
                }
            }
            public override float MinimumDensity
            {
                get
                {
                    return 0.0f;
                }
            }
            public override float MaximumDensity
            {
                get
                {
                    return 0.25f;
                }
            }
        }

        /// <summary>
        /// Contains information for void blocks.
        /// </summary>
        private class VoidBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Void;
                }
            }
            public override bool IsSolid
            {
                get
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Contains information for water blocks.
        /// </summary>
        private class WaterBlockTypeInfo : BlockTypeInfo
        {
            public override BlockType BlockType
            {
                get
                {
                    return BlockType.Water;
                }
            }
            public override bool IsSolid
            {
                get
                {
                    return false;
                }
            }
            public override Vector2 SideTextureCoordinate
            {
                get
                {
                    return Vector2.Zero;
                }
            }
            public override Vector2 TopTextureCoordinate
            {
                get
                {
                    return IndexToCoordinates( 2, 0 );
                }
            }
        }
    }
}