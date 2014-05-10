using System;
using Microsoft.Xna.Framework;
using Kyoob.Blocks;
using LibNoise;
using LibNoise.Modifiers;

#pragma warning disable 1587 // disable "invalid XML comment placement"

/**
 * Module and example code provided by Jason Bell.
 * See https://libnoisedotnet.codeplex.com/ for original code.
 */

#warning TODO : Add settings for planet terrain.

namespace Kyoob.Terrain
{
    /// <summary>
    /// An implementation of a fast planet terrain generator.
    /// </summary>
    public class PlanetTerrain : TerrainGenerator
    {
        private FastNoise _genContinents;
        private FastBillow _genLowlands;
        private ScaleBiasOutput _scaleLowlands;
        private FastRidgedMultifractal _genMountainBase;
        private ScaleBiasOutput _scaleMountainBase;
        private FastTurbulence _genMountains;
        private FastNoise _genLand;
        private FastBillow _genOceanBase;
        private ScaleOutput _scaleOceans;
        private IModule _generator;

        /// <summary>
        /// Gets or sets the planet generator's seed.
        /// </summary>
        public override int Seed
        {
            get
            {
                return base.Seed;
            }
            set
            {
                base.Seed = value;
                CompileGenerator();
            }
        }

        /// <summary>
        /// Creates a new planet terrain generator.
        /// </summary>
        /// <param name="seed">The generator's seed.</param>
        public PlanetTerrain( int seed )
            : base( seed )
        {
            // continents
            _genContinents = new FastNoise( seed );
            _genContinents.Frequency = 1.5;

            // lowlands
            _genLowlands = new FastBillow();
            _genLowlands.Frequency = 4;
            _scaleLowlands = new ScaleBiasOutput( _genLowlands );
            _scaleLowlands.Scale = 0.2;
            _scaleLowlands.Bias = 0.5;

            // base mountains
            _genMountainBase = new FastRidgedMultifractal( seed );
            _genMountainBase.Frequency = 4;
            _scaleMountainBase = new ScaleBiasOutput( _genMountainBase );
            _scaleMountainBase.Scale = 0.4;
            _scaleMountainBase.Bias = 0.85;

            // mountains
            _genMountains = new FastTurbulence( _genMountainBase );
            _genMountains.Power = 0.1;
            _genMountains.Frequency = 50;

            // flat land
            _genLand = new FastNoise( seed + 1 );
            _genLand.Frequency = 6;

            // oceans
            _genOceanBase = new FastBillow( seed );
            _genOceanBase.Frequency = 15;
            _scaleOceans = new ScaleOutput( _genOceanBase, 0.1 );

            CompileGenerator();
        }

        /// <summary>
        /// Compiles all of the terrain items into the single terrain generation module.
        /// </summary>
        private void CompileGenerator()
        {
            Select planetLand = new Select( _genLand, _scaleLowlands, _genMountains );
            planetLand.SetBounds( 0.0, 1000.0 );
            planetLand.EdgeFalloff = 0.5;

            Select planetFinal = new Select( _genContinents, _scaleOceans, planetLand );
            planetFinal.SetBounds( 0.0, 1000.0 );
            planetFinal.EdgeFalloff = 0.5;

            _generator = planetFinal;
        }

        /// <summary>
        /// Gets the block type for the designated world coordinates.
        /// </summary>
        /// <param name="position">The world coordinates.</param>
        /// <returns></returns>
        public override BlockType GetBlockType( Vector3 position )
        {
            double value = _generator.GetValue( position.X, position.Y, position.Z );

            throw new NotImplementedException();
        }
    }
}