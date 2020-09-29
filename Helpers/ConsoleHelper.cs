using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter.Helpers
{
    public static class ConsoleHelper
    {
        public static void Log(string msg)
        {
            Debug.WriteLine($"{DateTime.Now}: {msg}");
        }
        public static void Log(Exception ex)
        {
            Debug.WriteLine($"{DateTime.Now}-EXCEPTION:");
            Debug.WriteLine(ex.Message);
            Debug.WriteLine("STACKTRACE:");
            Debug.WriteLine(ex.StackTrace);
        }
    }
}
