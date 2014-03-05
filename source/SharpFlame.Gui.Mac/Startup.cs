﻿using System;
using Ninject;
using SharpFlame.Gui.Controls;
using SharpFlame.Gui.Infrastructure;
using SharpFlame.Gui.Mac.EtoCustom;

namespace SharpFlame.Gui.Mac
{
    public static class Startup
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            var macGenerator = new Eto.Platform.Mac.Generator();
			macGenerator.Add<IGLSurfaceHandler>( () => new MacGLSurfaceHandler() );

            //var app = new SharpFlameApplication( macGenrator );

            var kernel = Bootstrap.KernelWith(macGenerator);

            var app = kernel.Get<SharpFlameApplication>();

            app.Run( args );
        }
    }
}