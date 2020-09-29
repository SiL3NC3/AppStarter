using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppStarter.Helpers
{
    public static class NetworkHelper
    {
        public static bool IsValidEmail(string strIn)
        {
            return Regex.IsMatch(strIn, "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z", RegexOptions.IgnoreCase);
        }
        public static bool IsInternetAvailable(string server = "4.2.2.2")
        {
            try
            {
                return Dns.GetHostAddresses(server)[0].ToString().Length > 6;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsPingable(string hostname)
        {
            Uri url = new Uri(hostname);
            string pingurl = string.Format("{0}", url.Host);
            string host = pingurl;
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch { }
            return result;
        }
    }
}
