using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Mod.Loader.Bootstrap;

namespace Reloaded.Mod.Loader
{
    [Guid("83f622b9-74f4-4700-9167-52c4ce9e79aa")]
    [ComVisible(true)]
    public class API
    {
        /* Ensures DLL Resolution */
        static API()
        {
            Debugger.Launch();
            AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyResolver.ResolveAssembly;
        }

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */
        [ComVisible(true)]
        public void Initialize()
        {
            Debugger.Launch();
        }

        [ComVisible(true)]
        public void GetFunctions()
        {
            Debugger.Launch();
        }

        /* Return list of mod loader pointers. */
        struct LoaderMethodPointers
        {


        }
    }
}
