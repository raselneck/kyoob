using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// TODO : GraphicsDevice.SamplerStates and GraphicsDevice.VertexSamplerStates (?)

namespace Kyoob.Graphics
{
#if DEBUG
    /// <summary>
    /// An internal way of dumping current graphics information.
    /// </summary>
    internal static class GraphicsInfo
    {
        private static StringBuilder _sb;

        /// <summary>
        /// Dumps the graphics information to a file.
        /// </summary>
        /// <param name="file">The file to dump to.</param>
        public static void Dump( string file )
        {
            _sb = new StringBuilder();
            var d = Game.Instance.GraphicsDevice;

            // append info
            AppendGeneralInfo( d );
            AppendAdapter( d.Adapter );
            AppendBlendState( d.BlendState );
            AppendDepthStencilState( d.DepthStencilState );
            AppendRasterizerState( d.RasterizerState );

            // now append info that I just want to have
            _sb.AppendLine( "********************************************" );
            _sb.AppendLine( "*          GENERAL XNA STATE INFO          *" );
            _sb.AppendLine( "********************************************" );
            _sb.AppendLine();
            _sb.AppendLine();
            AppendBlendState( BlendState.Additive );
            AppendBlendState( BlendState.AlphaBlend );
            AppendBlendState( BlendState.NonPremultiplied );
            AppendBlendState( BlendState.Opaque );
            AppendDepthStencilState( DepthStencilState.Default );
            AppendDepthStencilState( DepthStencilState.DepthRead );
            AppendDepthStencilState( DepthStencilState.None );
            AppendRasterizerState( RasterizerState.CullClockwise );
            AppendRasterizerState( RasterizerState.CullCounterClockwise );
            AppendRasterizerState( RasterizerState.CullNone );

            // output all of the information to the file
            using ( var sw = File.CreateText( file ) )
            {
                sw.Write( _sb.ToString() );
            }
        }

        /// <summary>
        /// Appends adapter information to the string builder.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        private static void AppendAdapter( GraphicsAdapter adapter )
        {
            _sb.AppendFormat( "**** Graphics Adapter ({0}) ****\n", adapter.DeviceName );
            _sb.AppendFormat( "* Description:                            {0}\n", adapter.Description );
            _sb.AppendFormat( "* DeviceID:                               {0}\n", adapter.DeviceId );
            _sb.AppendFormat( "* DisplayMode:\n" );
            _sb.AppendFormat( "* - AspectRatio:                          {0}\n", adapter.CurrentDisplayMode.AspectRatio );
            _sb.AppendFormat( "* - SizeX:                                {0}\n", adapter.CurrentDisplayMode.Width );
            _sb.AppendFormat( "* - SizeY:                                {0}\n", adapter.CurrentDisplayMode.Height );
            _sb.AppendFormat( "* - SurfaceFormat:                        {0}\n", adapter.CurrentDisplayMode.Format );
            _sb.AppendFormat( "* IsDefault:                              {0}\n", adapter.IsDefaultAdapter );
            _sb.AppendFormat( "* IsWidescreen:                           {0}\n", adapter.IsWideScreen );
            _sb.AppendFormat( "* Revision #:                             {0}\n", adapter.Revision );
            _sb.AppendFormat( "* SubsystemID:                            {0}\n", adapter.SubSystemId );
            _sb.AppendFormat( "* VendorID:                               {0}\n", adapter.VendorId );

            _sb.AppendLine();
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends blend state information to the string builder.
        /// </summary>
        /// <param name="bs">The blend state.</param>
        private static void AppendBlendState( BlendState bs )
        {
            _sb.AppendFormat( "**** Blend State ({0}) ****\n", bs.Name );
            _sb.AppendFormat( "* AlphaBlendFunction:                     {0}\n", bs.AlphaBlendFunction );
            _sb.AppendFormat( "* AlphaDestinationBlend:                  {0}\n", bs.AlphaDestinationBlend );
            _sb.AppendFormat( "* AlphaSourceBlend:                       {0}\n", bs.AlphaSourceBlend );
            _sb.AppendFormat( "* BlendFactor:                            {0}\n", bs.BlendFactor );
            _sb.AppendFormat( "* ColorBlendFunction:                     {0}\n", bs.ColorBlendFunction );
            _sb.AppendFormat( "* ColorDestinationBlend:                  {0}\n", bs.ColorDestinationBlend );
            _sb.AppendFormat( "* ColorSourceBlend:                       {0}\n", bs.ColorSourceBlend );
            _sb.AppendFormat( "* ColorWriteChannels:                     {0}\n", bs.ColorWriteChannels );
            _sb.AppendFormat( "* ColorWriteChannels1:                    {0}\n", bs.ColorWriteChannels1 );
            _sb.AppendFormat( "* ColorWriteChannels2:                    {0}\n", bs.ColorWriteChannels2 );
            _sb.AppendFormat( "* ColorWriteChannels3:                    {0}\n", bs.ColorWriteChannels3 );

            _sb.AppendLine();
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends depth/stencil state information to the string builder.
        /// </summary>
        /// <param name="bs">The depth stencil state.</param>
        private static void AppendDepthStencilState( DepthStencilState dss )
        {
            _sb.AppendFormat( "**** Depth/Stencil State ({0}) ****\n", dss.Name );
            _sb.AppendFormat( "* CounterClockwiseStencilDepthBufferFail: {0}\n", dss.CounterClockwiseStencilDepthBufferFail );
            _sb.AppendFormat( "* CounterClockwiseStencilFail:            {0}\n", dss.CounterClockwiseStencilFail );
            _sb.AppendFormat( "* CounterClockwiseStencilFunction:        {0}\n", dss.CounterClockwiseStencilFunction );
            _sb.AppendFormat( "* CounterClockwiseStencilPass:            {0}\n", dss.CounterClockwiseStencilPass );
            _sb.AppendFormat( "* DepthBufferEnable:                      {0}\n", dss.DepthBufferEnable );
            _sb.AppendFormat( "* DepthBufferFunction:                    {0}\n", dss.DepthBufferFunction );
            _sb.AppendFormat( "* DepthBufferWriteEnable:                 {0}\n", dss.DepthBufferWriteEnable );
            _sb.AppendFormat( "* ReferenceStencil:                       {0}\n", dss.ReferenceStencil );
            _sb.AppendFormat( "* StencilDepthBufferFail:                 {0}\n", dss.StencilDepthBufferFail );
            _sb.AppendFormat( "* StencilEnable:                          {0}\n", dss.StencilEnable );
            _sb.AppendFormat( "* StencilFail:                            {0}\n", dss.StencilFail );
            _sb.AppendFormat( "* StencilFunction:                        {0}\n", dss.StencilFunction );
            _sb.AppendFormat( "* StencilMask:                            {0}\n", dss.StencilMask );
            _sb.AppendFormat( "* StencilPass:                            {0}\n", dss.StencilPass );
            _sb.AppendFormat( "* StencilWriteMask:                       {0}\n", dss.StencilWriteMask );
            _sb.AppendFormat( "* TwoSidedStencilMode:                    {0}\n", dss.TwoSidedStencilMode );

            _sb.AppendLine();
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends general graphics device information to the string builder.
        /// </summary>
        /// <param name="gd">The graphics device.</param>
        private static void AppendGeneralInfo( GraphicsDevice gd )
        {
            _sb.AppendFormat( "**** Graphics Device ****\n" );
            _sb.AppendFormat( "* BlendFactor:                            {0}\n", gd.BlendFactor );
            _sb.AppendFormat( "* GraphicsDeviceStatus:                   {0}\n", gd.GraphicsDeviceStatus );
            _sb.AppendFormat( "* GraphicsProfile:                        {0}\n", gd.GraphicsProfile );
            _sb.AppendFormat( "* MultiSampleMask:                        {0}\n", gd.MultiSampleMask );
            _sb.AppendFormat( "* PresentationParameters:\n" );
            _sb.AppendFormat( "* - BackBufferFormat:                     {0}\n", gd.PresentationParameters.BackBufferFormat );
            _sb.AppendFormat( "* - BackBufferHeight:                     {0}\n", gd.PresentationParameters.BackBufferHeight );
            _sb.AppendFormat( "* - BackBufferWidth:                      {0}\n", gd.PresentationParameters.BackBufferWidth );
            _sb.AppendFormat( "* - DepthStencilFormat:                   {0}\n", gd.PresentationParameters.DepthStencilFormat );
            _sb.AppendFormat( "* - DisplayOrientation:                   {0}\n", gd.PresentationParameters.DisplayOrientation );
            _sb.AppendFormat( "* - IsFullScreen:                         {0}\n", gd.PresentationParameters.IsFullScreen );
            _sb.AppendFormat( "* - MultiSampleCount:                     {0}\n", gd.PresentationParameters.MultiSampleCount );
            _sb.AppendFormat( "* - PresentationInterval:                 {0}\n", gd.PresentationParameters.PresentationInterval );
            _sb.AppendFormat( "* - RenderTargetUsage:                    {0}\n", gd.PresentationParameters.RenderTargetUsage );
            _sb.AppendFormat( "* ReferenceStencil:                       {0}\n", gd.ReferenceStencil );
            _sb.AppendFormat( "* Viewport:                               {0}\n", gd.Viewport );

            _sb.AppendLine();
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends rasterizer state information to the string builder.
        /// </summary>
        /// <param name="rs">The rasterizer state.</param>
        private static void AppendRasterizerState( RasterizerState rs )
        {
            _sb.AppendFormat( "**** Rasterizer State ({0}) ****\n", rs.Name );
            _sb.AppendFormat( "* CullMode:                               {0}\n", rs.CullMode );
            _sb.AppendFormat( "* DepthBias:                              {0}\n", rs.DepthBias );
            _sb.AppendFormat( "* FillMode:                               {0}\n", rs.FillMode );
            _sb.AppendFormat( "* MultiSampleAntiAlias:                   {0}\n", rs.MultiSampleAntiAlias );
            _sb.AppendFormat( "* ScissorTestEnable:                      {0}\n", rs.ScissorTestEnable );
            _sb.AppendFormat( "* SlopeScaleDepthBias:                    {0}\n", rs.SlopeScaleDepthBias );

            _sb.AppendLine();
            _sb.AppendLine();
        }
    }
#endif
}