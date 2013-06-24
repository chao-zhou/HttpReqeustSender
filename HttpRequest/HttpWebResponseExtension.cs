using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace MyHttpRequest
{
    public static class HttpWebResponseExtension
    {
        public static Stream GetDecompressSteam(this HttpWebResponse res)
        {
            switch (res.ContentEncoding.ToUpperInvariant())
            {
                case "GZIP":
                    return new GZipStream(res.GetResponseStream(), CompressionMode.Decompress);

                case "DEFLATE":
                    return new DeflateStream(res.GetResponseStream(), CompressionMode.Decompress);
                default:
                    return res.GetResponseStream();
            }
        }

        public static string GetContent(this HttpWebResponse res, Encoding encoding = null)
        {
            var _encoding = encoding ?? Encoding.GetEncoding(res.ContentEncoding);
            var _content = string.Empty;
            using (var reader = new StreamReader(res.GetDecompressSteam(), encoding))
            {
                _content = reader.ReadToEnd();
            }
            return _content;
        }
    }
}
