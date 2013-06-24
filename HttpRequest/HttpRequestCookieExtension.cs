using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace MyHttpRequest
{
    public static class HttpRequestCookieExtension
    {
        public static Dictionary<string, CookieCollection> Cookies { get; private set; }

        public static void LoadCookie(this HttpWebRequest request)
        {
            string host = request.RequestUri.Host;

            if (Cookies.ContainsKey(host))
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies[host]);
            }
        }

        public static void ResetCookie(this HttpWebRequest request)
        {
            string host = request.RequestUri.Host;

            if (Cookies.ContainsKey(host))
            {
                Cookies.Remove(host);
            }
        }

        public static void SaveCookie(this HttpWebResponse response)
        {
            if (response.Cookies == null || response.Cookies.Count < 1)
                return;

            string domain = response.Cookies[0].Domain;
            if (Cookies.ContainsKey(domain))
            {
                Cookies[domain] = response.Cookies;
            }
            else
            {
                Cookies.Add(domain, response.Cookies);
            }
        }
    }
}
