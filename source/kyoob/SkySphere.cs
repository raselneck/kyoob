using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob
{
    /// <summary>
    /// An implementation of a sky sphere.
    /// </summary>
    public sealed class SkySphere
    {
        private Model _model;
        private SkySphereEffect _effect;
        private GraphicsDevice _device;

        /// <summary>
        /// Creates a new sky sphere.
        /// </summary>
        /// <param name="sphere">The sky sphere model to use.</param>
        /// <param name="device">The graphics device to render to.</param>
        /// <param name="effect">The sky sphere effect to use to render.</param>
        /// <param name="texture">The cube map to use.</param>
        public SkySphere( Model sphere, GraphicsDevice device, SkySphereEffect effect, TextureCube texture )
        {
            _model = sphere;
            _device = device;
            _effect = effect;
            _effect.CubeMap = texture;
            foreach ( ModelMesh mesh in _model.Meshes )
            {
                foreach ( ModelMeshPart part in mesh.MeshParts )
                {
                    part.Effect = _effect.Effect;
                }
            }
        }

        /// <summary>
        /// Draws the sky sphere.
        /// </summary>
        /// <param name="camera">The camera to obtain view data from.</param>
        public void Draw( Camera camera )
        {
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;
            _effect.World = Matrix.CreateTranslation( camera.Position );

            // update the effect for each mesh part (should only be 1) and then draw
            foreach ( EffectPass pass in _effect.Effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                foreach ( ModelMesh mesh in _model.Meshes )
                {
                    foreach ( ModelMeshPart part in mesh.MeshParts )
                    {
                        part.Effect = _effect.Effect;
                    }
                    mesh.Draw();
                }
            }

            _device.RasterizerState = RasterizerState.CullCounterClockwise;
            _device.DepthStencilState = DepthStencilState.Default;
        }
    }
}