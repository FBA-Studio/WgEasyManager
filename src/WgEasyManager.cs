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
        private static string _password = "";
        private static string _serverUrl = "";
        private static bool HasSsl;

        ///<summary>
        /// Welcome to WgEasyClient!
        ///</summary>
        ///<param name="password">The Password for access to your server</param>
        ///<param name="serverUrl">URL with Port for API Requests</param>
        ///<param name="hasSsl">If you have SSL - we recommend set <b>true</b>, false - if you hasn't SSL</param>
        public WgEasyClient(string password, string serverUrl, bool hasSsl) {
            _password = password;
            _serverUrl = serverUrl;
            HasSsl = hasSsl;
            bool hasSession = loadingSession();
            if(!hasSession) {
                loging();
            }
        }

        private void makeRequest(string method, string urlMethod, string key, string value, out string data) {
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
                if (HasSsl == false) {
                    httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                }

                using(HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using StreamReader streamReader = new StreamReader(stream);
                    data = streamReader.ReadToEnd();
                    cash = httpWebResponse.Cookies;
                    updateCookieContainer(cash);
                }
            }
            catch(Exception exc) {
                data = null;
            }
        }
        private void makeRequest(string method, string urlMethod, out byte[] data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_serverUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.CookieContainer = _cookies;
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
                    cash = httpWebResponse.Cookies;
                    updateCookieContainer(cash);
                }
            }
            catch(Exception exc) {
                data = null;
            }
        }

        private void createSession() {
            if(!File.Exists(_session_file)) {
                using(FileStream stream = File.Create(_session_file)) {
                    BinaryFormatter binary = new BinaryFormatter();
                    binary.Serialize(stream, _cookies);
                }
            }
        }
        private bool loadingSession() {
            if(File.Exists(_session_file)) {
                using(Stream stream = File.OpenRead(_session_file)) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    _cookies = (CookieContainer)formatter.Deserialize(stream);
                }
                return true;
            }
            return true;
        }
        private void updateCookieContainer(CookieCollection cookies) {
            foreach(Cookie cookie in cookies) {
                _cookies.Add(cookie);
            }
        }
        private void loging() {
            makeRequest("POST", "api/session", "password", _password, out var temp);
            createSession();
        }

        ///<summary>
        ///Get list of keys
        ///</summary>
        ///<returns><see cref="System.Collections.Generic.List"/> with <see cref="T:WgEasyManager.Types.WireGuardKey"/></returns>
        public List<WireGuardKey> GetKeys() {
            makeRequest("GET", "api/wireguard/client", "none", "none", out var data);
            return(JObject.Parse(data)[""] as JArray).ToObject<List<WireGuardKey>>();
        }

        ///<summary>
        ///Create new key
        ///</summary>
        ///<param name="name">Name of new key</param>
        public bool CreateKey(string name) {
            makeRequest("POST", "api/wireguard/client", "name", name, out var data);
            return true;
        }

        ///<summary>
        /// Delete key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public bool DeleteKey(string clientId) {
            makeRequest("DELETE", $"api/wireguard/client/{clientId}", withoutParametr, withoutParametr, out var data);
            return true;
        }

        ///<summary>
        ///Unblock key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public bool UnbanKey(string clientId) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/enable", withoutParametr, withoutParametr, out var data);
            return true;
        }

        ///<summary>
        ///Block key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public bool BlockKey(string clientId) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/disable", withoutParametr, withoutParametr, out var data);
            return true;
        }

        ///<summary>
        ///Rename key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="name">New name for key</param>
        public bool RenameKey(string clientId, string name) {
            makeRequest("PUT", $"api/wireguard/client/{clientId}/name/", "name", name, out var data);
            return true;
        }

        ///<summary>
        ///Set new IP for connection by this key
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="address">IP Adress for connection</param>
        public bool SetNewIp(string clientId, string address) {
            makeRequest("PUT", $"api/wireguard/client/{clientId}/address/", "address", address, out var data);
            return true;
        }

        ///<summary>
        ///Download .config file for using key in WireGuard Client
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="path">Path for saving config file</param>
        public bool DownloadConfig(string clientId, string path) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/configuration", out var data);
            File.WriteAllBytes($"{path}/{clientId}.config", data);
            return true;
        }
        
        ///<summary>
        ///Download QR-Code file for using key in WireGuard Mobile Client
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="path">Path for saving QR-Code in .svg</param>
        public bool DownloadQrCode(string clientId, string path) {
            makeRequest("POST", $"api/wireguard/client/{clientId}/qrcode.svg", out var data);
            File.WriteAllBytes($"{path}/{clientId}.svg", data);
            return true;
        }
    }
}

