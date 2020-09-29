using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter.Helpers
{
    public static class RegistryHelper
    {


        public static bool GetStartWithWindowsState(string APPNAME = null)
        {
            if (APPNAME == null)
                APPNAME = AssemblyHelper.InfoProgram.AssemblyTitle;
            return Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue(APPNAME) != null;
        }

        public static void SetStartWithWindows(bool active, string APPNAME = null)
        {
            if (APPNAME == null)
                APPNAME = AssemblyHelper.InfoProgram.AssemblyTitle;
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (active)
                registryKey.SetValue(APPNAME, (object)AppDomain.CurrentDomain.BaseDirectory.ToString());
            else
                registryKey.DeleteValue(APPNAME, false);
        }
    }
}
