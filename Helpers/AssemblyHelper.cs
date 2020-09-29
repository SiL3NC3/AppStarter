using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter.Helpers
{
    public static class AssemblyHelper
    {
        public static DateTime GetLinkerTime(string filePath = null, TimeZoneInfo target = null)
        {
            if (filePath == null)
                filePath = System.Reflection.Assembly.GetCallingAssembly().Location;

            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        static public class InfoProgram
        {
            static public string AssemblyGuid
            {
                get
                {
                    object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return String.Empty;
                    }
                    return ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
                }
            }
            static public string AssemblyTitle
            {
                get
                {
                    object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                    if (attributes.Length > 0)
                    {
                        AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                        if (titleAttribute.Title != "")
                        {
                            return titleAttribute.Title;
                        }
                    }
                    return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
                }
            }
            static public string AssemblyName
            {
                get { return Assembly.GetExecutingAssembly().GetName().Name; }
            }
        }

        public static Version GetVersion(Type t)
        {
            var a = Assembly.GetAssembly(t);
            return GetVersion(a);
        }
        public static Version GetVersion(Assembly a)
        {
            return a.GetName().Version;
        }
        public static Version GetVersion()
        {
            var a = Assembly.GetExecutingAssembly();
            return GetVersion(a);
        }
        public static string GetVersionString()
        {
            var a = Assembly.GetExecutingAssembly();
            var v = GetVersion(a);
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
        public static string GetVersionString(Assembly a)
        {
            var v = GetVersion(a);
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }

        public static string Version
        {
            get
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
        }
    }
}
