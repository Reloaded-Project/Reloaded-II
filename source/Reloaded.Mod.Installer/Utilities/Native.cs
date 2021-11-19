using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Reloaded.Mod.Installer.Utilities;

public static class Native
{
    [DllImport("Shlwapi.dll", EntryPoint = "PathIsDirectoryEmptyW", CharSet = CharSet.Unicode)]
    public static extern bool IsDirectoryEmpty(string directory);
}