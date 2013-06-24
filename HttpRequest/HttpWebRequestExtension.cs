using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace MyHttpRequest
{
    public static class HttpWebRequestExtension
    {
        static string AgentString = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8 ( .NET CLR 3.5.30729; .NET4.0C)";

        public static HttpWebResponse GET(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "GET");
        }
        public static HttpWebResponse HEAD(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "HEAD");
        }
        public static HttpWebResponse PUT(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "PUT");
        }
        public static HttpWebResponse POST(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "POST");
        }
        public static HttpWebResponse TRACE(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "TRACE");
        }
        public static HttpWebResponse OPTIONS(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "OPTIONS");
        }
        public static HttpWebResponse DELETE(this HttpWebRequest request, string body)
        {
            return HttpMethod(request, body, "DELETE");
        }
        public static HttpWebResponse HttpMethod(this HttpWebRequest request, string body, string httpMethod)
        {
            HttpWebResponse response = null;

            if (httpMethod == "POST")
            {
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = httpMethod;

                if (request.RequestUri.Scheme.ToUpper() == "HTTPS")
                    request.ClientCertificates = GetCertificates(request.RequestUri);

                request.AllowAutoRedirect = true;
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                request.UserAgent = AgentString;
                response = SendRequest(request, body);
            }
            else
            {
                string queryStringUrl = String.IsNullOrWhiteSpace(body) ? request.RequestUri.ToString() : string.Format("{0}?{1}", request.RequestUri.ToString(), body);
                HttpWebRequest newReq = HttpWebRequest.Create(queryStringUrl) as HttpWebRequest; //this will disable other optoin
                ConfigNewRequest(newReq, request);

                if (newReq.RequestUri.Scheme.ToUpper() == "HTTPS")
                    newReq.ClientCertificates = GetCertificates(request.RequestUri);

                newReq.Method = httpMethod;
                newReq.AllowAutoRedirect = true;
                newReq.UserAgent = AgentString;
                response = newReq.GetResponse() as HttpWebResponse;
            }
            return response;
        }

        private static void ConfigNewRequest(HttpWebRequest newReq, HttpWebRequest oldReq)
        {
            Type type = typeof(HttpWebRequest);

            object val = null;
            foreach (PropertyInfo p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (p.CanWrite)
                {
                    try
                    {
                        val = p.GetValue(oldReq, null);
                        p.SetValue(newReq, val, null);
                    }
                    catch
                    { }
                }
            }

        }
        private static HttpWebResponse SendRequest(HttpWebRequest request, string body)
        {
            //HttpUtility.UrlEncode

            //format value
            string bodyString = EncodeBodyString(body);
            byte[] encodingBytes = Encoding.UTF8.GetBytes(bodyString);
            request.ContentLength = encodingBytes.Length;

            //Send
            Stream os = null;
            try
            {
                os = request.GetRequestStream();
                os.Write(encodingBytes, 0, encodingBytes.Length);
            }
            finally
            {
                if (os != null)
                    os.Close();
            }

            //Get Response
            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
                return response;
            }
            catch (Exception ex)
            {
                if (response != null)
                    response.Close();

                throw ex;
            }

        }
        private static string EncodeBodyString(string body)
        {
            Regex reg = new Regex("(?<key>.*?)=(?<val>[^&]*)",
                  RegexOptions.ExplicitCapture
                | RegexOptions.Singleline
                | RegexOptions.Compiled);

            if (reg.IsMatch(body))
            {
                StringBuilder strb = new StringBuilder();
                foreach (Match c in reg.Matches(body))
                {
                    strb.Append(string.Format("{0}={1}",
                                                            c.Groups["key"].Value,
                                                            HttpUtility.UrlEncode(c.Groups["val"].Value))
                        );
                }
                return strb.ToString();
            }

            return body;
        }

        #region X509 Certificate

        static Dictionary<string, X509Certificate> cerDic = new Dictionary<string, X509Certificate>();

        private static X509CertificateCollection GetCertificates(Uri requestUri)
        {
            X509CertificateCollection certificates = new X509CertificateCollection();

            X509Certificate cer = GetX509Certificate(requestUri, customXertificateValidation);

            if (cer != null)
                certificates.Add(cer);

            return certificates;
        }
        private static X509Certificate GetX509Certificate(Uri requestUri, RemoteCertificateValidationCallback callback)
        {
            if (callback != null)
                ServicePointManager.ServerCertificateValidationCallback += callback;

            if (cerDic.ContainsKey(requestUri.ToString()))
                return cerDic[requestUri.ToString()];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            ServicePoint currentServicePoint = request.ServicePoint;

            //ServicePointManager.ServerCertificateValidationCallback -= callback;
            cerDic.Add(requestUri.ToString(), currentServicePoint.Certificate);
            return currentServicePoint.Certificate;
        }
        private static bool customXertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        #endregion
    }
}
