using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kyoob.Effects;

namespace Kyoob
{
    /// <summary>
    /// An implementation of a sky box.
    /// </summary>
    public sealed class SkyBox
    {
        private Model _model;
        private SkyBoxEffect _effect;
        private GraphicsDevice _device;

        /// <summary>
        /// Creates a new sky box.
        /// </summary>
        /// <param name="model">The sky box model to use.</param>
        /// <param name="device">The graphics device to render to.</param>
        /// <param name="effect">The sky box effect to use to render.</param>
        /// <param name="texture">The cube map to use.</param>
        public SkyBox( Model model, GraphicsDevice device, SkyBoxEffect effect, TextureCube texture )
        {
            _model = model;
            _device = device;
            _effect = effect;
            _effect.CubeMap = texture;
        }

        /// <summary>
        /// Draws the sky box.
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