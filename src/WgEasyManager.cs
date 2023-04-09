using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
            Task<bool> loading = loadingSession();
            Task _loging = loging();
            bool hasSession = loading.Result;
            if(!hasSession) {
                _loging.Wait();
            }
        }

        private Task makeRequest(string method, string urlMethod, string key, string value, out string data) {
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
            return Task.CompletedTask;
        }
        private Task makeRequest(string method, string urlMethod, out byte[] data) {
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
            return Task.CompletedTask;
        }

        private Task createSession() {
            if(!File.Exists(_session_file)) {
                using(FileStream stream = File.Create(_session_file)) {
                    BinaryFormatter binary = new BinaryFormatter();
                    binary.Serialize(stream, _cookies);
                }
            }
            return Task.CompletedTask;
        }
        private Task<bool> loadingSession() {
            if(File.Exists(_session_file)) {
                using(Stream stream = File.OpenRead(_session_file)) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    _cookies = (CookieContainer)formatter.Deserialize(stream);
                }
                return true;
            }
            return false;
        }
        private void updateCookieContainer(CookieCollection cookies) {
            foreach(Cookie cookie in cookies) {
                _cookies.Add(cookie);
            }
        }
        private async Task loging() {
            await makeRequest("POST", "api/session", "password", _password, out _);
            await createSession();
        }

        ///<summary>
        ///Get list of keys
        ///</summary>
        ///<returns><see cref="System.Collections.Generic.List"/> with <see cref="T:WgEasyManager.Types.WireGuardKey"/></returns>
        public async Task<List<WireGuardKey>> GetKeys() {
            await makeRequest("GET", "api/wireguard/client", "none", "none", out var data);
            return(JObject.Parse(data)[""] as JArray).ToObject<List<WireGuardKey>>();
        }

        ///<summary>
        ///Create new key
        ///</summary>
        ///<param name="name">Name of new key</param>
        public async Task<WireGuardKey> CreateKey(string name) {
            await makeRequest("POST", "api/wireguard/client", "name", name, out var data);
            return (JObject.Parse(data)).ToObject<WireGuardKey>();
        }

        ///<summary>
        /// Delete key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public async Task DeleteKey(string clientId) {
            await makeRequest("DELETE", $"api/wireguard/client/{clientId}", withoutParametr, withoutParametr, out _);
        }

        ///<summary>
        ///Unblock key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public async Task UnbanKey(string clientId) {
            await makeRequest("POST", $"api/wireguard/client/{clientId}/enable", withoutParametr, withoutParametr, out _);
        }

        ///<summary>
        ///Block key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public async Task BlockKey(string clientId) {
            await makeRequest("POST", $"api/wireguard/client/{clientId}/disable", withoutParametr, withoutParametr, out _);
        }

        ///<summary>
        ///Rename key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="name">New name for key</param>
        public async Task RenameKey(string clientId, string name) {
            await makeRequest("PUT", $"api/wireguard/client/{clientId}/name/", "name", name, out _);
        }

        ///<summary>
        ///Set new IP for connection by this key
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="address">IP Adress for connection</param>
        public async Task SetNewIp(string clientId, string address) {
            await makeRequest("PUT", $"api/wireguard/client/{clientId}/address/", "address", address, out _);
        }

        ///<summary>
        ///Download .config file for using key in WireGuard Client
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="path">Path for saving config file</param>
        public async Task DownloadConfig(string clientId, string path) {
            await makeRequest("POST", $"api/wireguard/client/{clientId}/configuration", out var data);
            await File.WriteAllBytesAsync($"{path}/{clientId}.config", data);
        }

        ///<summary>
        ///Download QR-Code file for using key in WireGuard Mobile Client
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="path">Path for saving QR-Code in .svg</param>
        public async Task DownloadQrCode(string clientId, string path) {
            await makeRequest("POST", $"api/wireguard/client/{clientId}/qrcode.svg", out var data);
            await File.WriteAllBytesAsync($"{path}/{clientId}.svg", data);
        }
    }
}

