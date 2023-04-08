using System;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using WgEasyManager.Types;

namespace WgEasyManager {
    public class WgEasyClient {
        private static readonly string header = "application/json";
        private static readonly string _session_file = "session.wgmanager";
        private static CookieContainer _cookies = new CookieContainer();
        private static CookieCollection cash = new CookieCollection();
        private static readonly string withoutParametr = "none";
        private static string _password;
        private static string _serverUrl;

        public WgEasyClient(string password, string serverUrl) {
            _password = password;
            _serverUrl = serverUrl;
            bool hasSession = loadingSession();
            if(!hasSession) {
                loging();
            }
        }

        private static bool makeRequest(string method, string urlMethod, string key, string value, out string data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_serverUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.CookieContainer = _cookies;
                httpWebRequest.ContentType = header;

                if(key != "none") {
                    var dict = new Dictionary<string,string>();
                    dict.Add(key, value);
                    string postDataString = string.Join("&", dict.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value)));
                    httpWebRequest.ContentLength = postDataString.Length;

                    using (StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        writer.Write(postDataString);
                    }
                }

                using(HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using StreamReader streamReader = new StreamReader(stream);
                    data = streamReader.ReadToEnd();
                    cash = httpWebResponse.Cookies;
                    updateCookieContainer(cash);
                }
                return true;
            }
            catch(Exception exc) {
                data = null;
                return false;
            }
        }
        private static bool makeRequest(string method, string urlMethod, out byte[] data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_serverUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.CookieContainer = _cookies;
                httpWebRequest.ContentType = header;

                using(HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using(MemoryStream memoryStream = new MemoryStream()) {
                        stream.CopyTo(memoryStream);
                        data = memoryStream.ToArray();
                    }
                    cash = httpWebResponse.Cookies;
                    updateCookieContainer(cash);
                }
                return true;
            }
            catch(Exception exc) {
                data = null;
                return false;
            }
        }

        private static void createSession() {
            if(!File.Exists(_session_file)) {
                using(FileStream stream = File.Create(_session_file)) {
                    BinaryFormatter binary = new BinaryFormatter();
                    binary.Serialize(stream, _cookies);
                }
            }
        }
        private static bool loadingSession() {
            if(File.Exists(_session_file)) {
                using(Stream stream = File.OpenRead(_session_file)) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    _cookies = (CookieContainer)formatter.Deserialize(stream);
                }
                return true;
            }
            return true;
        }
        private static void updateCookieContainer(CookieCollection cookies) {
            foreach(Cookie cookie in cookies)
                _cookies.Add(cookie);
        }
        private static bool loging() {
            makeRequest("POST", "api/session", "password", _password, out var temp);
            createSession();
            return true;
        }

        public static List<WireGuardKey> GetKeys() {
            makeRequest("GET", "api/wireguard/client", "none", "none", out var data);
            return(JObject.Parse(data)[""] as JArray).ToObject<List<WireGuardKey>>();
        }
        public static bool CreateKey(string name) {
            makeRequest("POST", "api/wireguard/client", "name", name, out var data);
            return true;
        }
        public static bool DeleteKey(string clientId) {
            makeRequest("DELETE", $"api/wireguard/client/{clientId}", withoutParametr, withoutParametr, out var data);
            return true;
        }
        public static bool UnbanKey(string clientId) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/enable", withoutParametr, withoutParametr, out var data);
            return true;
        }
        public static bool BlockKey(string clientId) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/disable", withoutParametr, withoutParametr, out var data);
            return true;
        }
        public static bool RenameKey(string clientId, string name) {
            makeRequest("PUT", $"api/wireguard/client/{clientId}/name/", "name", name, out var data);
            return true;
        }
        public static bool SetNewIp(string clientId, string address) {
            makeRequest("PUT", $"api/wireguard/client/{clientId}/address/", "address", address, out var data);
            return true;
        }
        public static bool DownloadConfig(string clientId, string path) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/configuration", out var data);
            File.WriteAllBytes($"{path}/{clientId}.config", data);
            return true;
        }
        public static bool DownloadQrCode(string clientId, string path) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/qrcode.svg", out var data);
            File.WriteAllBytes($"{path}/{clientId}.svg", data);
            return true;
        }
    }
}

