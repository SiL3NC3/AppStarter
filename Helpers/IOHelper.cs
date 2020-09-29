using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;


namespace AppStarter.Helpers
{
    public static class IOHelper
    {
        public static byte[] Zip(string str)
        {
            using (MemoryStream memoryStream1 = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream((Stream)memoryStream2, CompressionMode.Compress))
                        IOHelper.CopyTo((Stream)memoryStream1, (Stream)gzipStream);
                    return memoryStream2.ToArray();
                }
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (MemoryStream memoryStream1 = new MemoryStream(bytes))
            {
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream((Stream)memoryStream1, CompressionMode.Decompress))
                        IOHelper.CopyTo((Stream)gzipStream, (Stream)memoryStream2);
                    return Encoding.UTF8.GetString(memoryStream2.ToArray());
                }
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] buffer = new byte[4096];
            int count;
            while ((uint)(count = src.Read(buffer, 0, buffer.Length)) > 0U)
                dest.Write(buffer, 0, count);
        }

        public static long DirSize(DirectoryInfo d)
        {
            long num = 0;
            foreach (FileInfo file in d.GetFiles())
                num += file.Length;
            foreach (DirectoryInfo directory in d.GetDirectories())
                num += IOHelper.DirSize(directory);
            return num;
        }

        private static long GetDirectorySize(string p)
        {
            string[] files = Directory.GetFiles(p, "*.*");
            long num = 0;
            foreach (string fileName in files)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                num += fileInfo.Length;
            }
            return num;
        }

        public static void CopyFile(string sourcePath, string destinationPath)
        {
            FileInfo fileInfo = new FileInfo(sourcePath);
            long fileSize = fileInfo.Length;
            long currentBytesTransferred = 0;
            long totalBytesTransferred = 0;
            Queue<long> snapshots = new Queue<long>(30);
            System.Timers.Timer timer = new System.Timers.Timer(1000.0);
            timer.Elapsed += (ElapsedEventHandler)((sender, e) =>
            {
                if (snapshots.Count == 30)
                    snapshots.Dequeue();
                snapshots.Enqueue(Interlocked.Exchange(ref currentBytesTransferred, 0L));
                double num1 = snapshots.Average();
                long num2 = fileSize - totalBytesTransferred;
                Console.WriteLine("Average speed: {0:#} MBytes / second", (object)(num1 / 1048576.0));
                if (num1 > 0.0)
                    Console.WriteLine("Time left: {0}", (object)TimeSpan.FromSeconds(Math.Round(TimeSpan.FromSeconds((double)num2 / num1).TotalSeconds)));
                else
                    Console.WriteLine("Time left: Infinite");
            });
            FileStream inputStream = fileInfo.OpenRead();
            try
            {
                FileStream outputStream = File.OpenWrite(destinationPath);
                try
                {
                    timer.Start();
                    byte[] buffer = new byte[4096];
                    int numBytes = 0;
                    int length = buffer.Length;
                    TimeSpan timeout = TimeSpan.FromMinutes(10.0);
                    do
                    {
                        ManualResetEvent mre = new ManualResetEvent(false);
                        inputStream.BeginRead(buffer, 0, length, (AsyncCallback)(asyncReadResult =>
                        {
                            numBytes = inputStream.EndRead(asyncReadResult);
                            outputStream.BeginWrite(buffer, 0, numBytes, (AsyncCallback)(asyncWriteResult =>
                            {
                                outputStream.EndWrite(asyncWriteResult);
                                currentBytesTransferred = Interlocked.Add(ref currentBytesTransferred, (long)numBytes);
                                totalBytesTransferred = Interlocked.Add(ref totalBytesTransferred, (long)numBytes);
                                mre.Set();
                            }), (object)null);
                        }), (object)null);
                        mre.WaitOne(timeout);
                    }
                    while ((uint)numBytes > 0U);
                    timer.Stop();
                }
                finally
                {
                    if (outputStream != null)
                        outputStream.Dispose();
                }
            }
            finally
            {
                if (inputStream != null)
                    inputStream.Dispose();
            }
        }

        public static string GetFileSizeHuman(long len)
        {
            string[] strArray = new string[4] { "B", "KB", "MB", "GB" };
            int index = 0;
            while (len >= 1024L && ++index < strArray.Length) len /= 1024L;
            return string.Format("{0:0} {1}", (object)len, (object)strArray[index]);
        }

        public static string GetFileSizeHumanFromFile(string filename)
        {
            string[] strArray = new string[4]
            {
        "B",
        "KB",
        "MB",
        "GB"
            };
            double num = (double)new FileInfo(filename).Length;
            int index = 0;
            while (num >= 1024.0 && ++index < strArray.Length)
                num /= 1024.0;
            return string.Format("{0:0} {1}", (object)num, (object)strArray[index]);
        }

        public static long ConvertFileSizeToBytes(string fileSizeStr)
        {
            long num = 0;
            if (fileSizeStr.Contains("KB"))
                num = long.Parse(fileSizeStr.Replace("KB", string.Empty)) * 1024L;
            else if (fileSizeStr.Contains("MB"))
                num = long.Parse(fileSizeStr.Replace("MB", string.Empty)) * 1024L * 1024L;
            else if (fileSizeStr.Contains("GB"))
                num = long.Parse(fileSizeStr.Replace("GB", string.Empty)) * 1024L * 1024L * 1024L;
            else if (fileSizeStr.Contains("B"))
                num = long.Parse(fileSizeStr.Replace("B", string.Empty));
            return num;
        }

        public static bool WriteByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                FileStream fileStream = new FileStream(_FileName, FileMode.Create, FileAccess.Write);
                fileStream.Write(_ByteArray, 0, _ByteArray.Length);
                fileStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", (object)ex.ToString());
            }
            return false;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter((Stream)memoryStream);
            streamWriter.Write(s);
            streamWriter.Flush();
            memoryStream.Position = 0L;
            return (Stream)memoryStream;
        }

        public static Stream GenerateUnicodeStreamFromString(string s)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter((Stream)memoryStream, Encoding.Unicode);
            streamWriter.Write(s);
            streamWriter.Flush();
            memoryStream.Position = 0L;
            return (Stream)memoryStream;
        }

        public static void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null)
                , FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                PropagationFlags.InheritOnly, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }
    }
}
