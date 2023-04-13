using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace WgEasyManager.Advanced {
    public class WgEasyAdvanced {
        private static readonly string header = "application/json";
        private static string Password;
        private static string ServerUrl;
        private static bool HasSsl;
        public CookieContainer Cookies = new CookieContainer();
        public CookieCollection Cash;


        public WgEasyAdvanced(string password, string serverUrl, bool hasSsl) {
            Password = password;
            ServerUrl = serverUrl;
            HasSsl = hasSsl;
        }

        public Task MakeRequest(string method, string urlMethod, Dictionary<string, string> postContent, out string data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ServerUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.ProtocolVersion = HttpVersion.Version11;
                httpWebRequest.CookieContainer = _cookies;
                httpWebRequest.ContentType = header;

                if (postContent.Count > 0) {
                    string postDataString = string.Join("&", postContent.Select(x => "{\"" + Uri.EscapeDataString(x.Key) + "\":" + "\"" + Uri.EscapeDataString(x.Value) + "\"}"));
                    httpWebRequest.ContentLength = postDataString.Length;

                    using (StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream())) {
                        writer.Write(postDataString);
                    }
                }
                if (HasSsl == false) {
                    httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                }

                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using StreamReader streamReader = new StreamReader(stream);
                    data = streamReader.ReadToEnd();
                    Cash = httpWebResponse.Cookies;
                }
            }
            catch (Exception exc) {
                data = null;
            }
            return Task.CompletedTask;
        }
        public Task MakeRequest(string method, string urlMethod, out byte[] data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ServerUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.ProtocolVersion = HttpVersion.Version11;
                httpWebRequest.CookieContainer = Cookies;
                httpWebRequest.ContentType = header;

                if (HasSsl == false) {
                    httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                }

                using(HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using(MemoryStream memoryStream = new MemoryStream()) {
                        stream.CopyTo(memoryStream);
                        data = memoryStream.ToArray();
                    }
                    Cash = httpWebResponse.Cookies;
                }
            }
            catch(Exception exc) {
                data = null;
            }
            return Task.CompletedTask;
        }
    }
}
