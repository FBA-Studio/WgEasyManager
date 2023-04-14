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

using WgEasyManager.Exceptions;
using WgEasyManager.Types;

namespace WgEasyManager {
    public class WgEasyClient {
        private static readonly string header = "application/json";
        private static string _session_file = "session.wgmanager";
        private static CookieContainer _cookies = new CookieContainer();
        private static CookieCollection cash = new CookieCollection();
        private static readonly string withoutParametr = "none";
        private static string _password = "";
        private static string _serverUrl = "";
        public bool HasSsl;

        ///<summary>
        /// Welcome to WgEasyClient!
        ///</summary>
        ///<param name="password">The Password for access to your server</param>
        ///<param name="serverUrl">URL with Port for API Requests</param>
        ///<param name="hasSsl">If you have SSL - we recommend set <b>true</b>, false - if you hasn't SSL</param>
        public WgEasyClient(string serverUrl, string password, bool hasSsl) {
            _password = password;
            _serverUrl = serverUrl;
            HasSsl = hasSsl;
            _session_file = "wg-sessions/" + serverUrl + "_server" + ".wgmanager";
            loadingSession();
        }

        private Task makeRequest(string method, string urlMethod, string key, string value, out string data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_serverUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.ProtocolVersion = HttpVersion.Version11;
                httpWebRequest.CookieContainer = _cookies;
                httpWebRequest.ContentType = header;

                if (key != "none") {
                    var dict = new Dictionary<string, string>();
                    dict.Add(key, value);
                    string postDataString = string.Join("&", dict.Select(x => "{\"" + Uri.EscapeDataString(x.Key) + "\":" + "\"" + Uri.EscapeDataString(x.Value) + "\"}"));
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
                    cash = httpWebResponse.Cookies;
                }
            }
            catch (Exception exc) {
                data = null;
                throwApiException(exc);
            }
            return Task.CompletedTask;
        }
        private Task makeRequest(string method, string urlMethod, out byte[] data) {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_serverUrl + "/" + urlMethod);
                httpWebRequest.Method = method;
                httpWebRequest.ProtocolVersion = HttpVersion.Version11;
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
                }
            }
            catch(Exception exc) {
                data = null;
                throwApiException(exc);
            }
            return Task.CompletedTask;
        }

        private Task createSession() {
            if(!File.Exists(_session_file)) {
                using(FileStream stream = File.Create(_session_file)) {
                    var session = JsonConvert.SerializeObject(_cookies);
                    File.WriteAllText(_session_file, session);
                }
            }
            else {
                File.Delete(_session_file);
                using(FileStream stream = File.Create(_session_file)) {
                    var session = JsonConvert.SerializeObject(_cookies);
                    File.WriteAllText(_session_file, session);
                }
            }
            return Task.CompletedTask;
        }
        private Task loadingSession() {
            if(File.Exists(_session_file)) {
                using(Stream stream = File.OpenRead(_session_file)) {
                    var session = File.ReadAllText(_session_file);
                    _cookies = JsonConvert.DeserializeObject<CookieContainer>(session);
                }
            }
            return Task.CompletedTask;
        }
        private Task updateCookieContainer(CookieCollection cookies) {
            foreach(Cookie cookie in cookies) {
                _cookies.Add(cookie);
            }
            return Task.CompletedTask;
        }
        private async Task<LoginStatus> checkAuthrization() {
            await makeRequest("GET", "api/session", withoutParametr, withoutParametr, out var data);
            return (JObject.Parse(data)).ToObject<LoginStatus>();
        }

        private static void throwApiException(Exception exception) {
            if (exception.Message == "The SSL connection could not be established, see inner exception.")
                throw new WgEasyException("Your Wg-Easy Server hasn't SSL connection, please set hasSsl = false", exception);
            else if (exception.Message.Contains($"({_serverUrl}:443)"))
                throw new WgEasyException("Wg-Easy server not found. Please check you URL and port");
            else if(exception.Message.Contains("(404) Not Found"))
                throw new WgEasyException("Server returned 404: Key by ClientId not found");
            else
                throw new WgEasyException("Unknown exception occured. See inner Exception for more information");
        }

        ///<summary>
        /// Login to API Server with session check.
        ///</summary>
        public async Task LoginToServerIfNeeded() {
            await makeRequest("GET", "api/session", withoutParametr, withoutParametr, out var data);
            if(!(JObject.Parse(data)).ToObject<LoginStatus>().Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
        }

        ///<summary>
        ///Get list of keys
        ///</summary>
        ///<returns><see cref="System.Collections.Generic.List"/> with <see cref="T:WgEasyManager.Types.WireGuardKey"/></returns>
        public async Task<List<WireGuardKey>?> GetKeys() {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("GET", "api/wireguard/client", "none", "none", out var data);
            if(string.IsNullOrEmpty(data))
                return null;
            return JArray.Parse(data).ToObject<List<WireGuardKey>>();
        }

        ///<summary>
        ///Create new key
        ///</summary>
        ///<param name="name">Name of new key</param>
        public async Task<WireGuardKey> CreateKey(string name) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("POST", "api/wireguard/client", "name", name, out var data);
            return (JObject.Parse(data)).ToObject<WireGuardKey>();
        }

        ///<summary>
        /// Delete key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public async Task DeleteKey(string clientId) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("DELETE", $"api/wireguard/client/{clientId}", withoutParametr, withoutParametr, out _);
        }

        ///<summary>
        ///Unblock key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public async Task UnbanKey(string clientId) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("POST", $"api/wireguard/client/{clientId}/enable", withoutParametr, withoutParametr, out _);
        }

        ///<summary>
        ///Block key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        public async Task BlockKey(string clientId) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("POST", $"api/wireguard/client/{clientId}/disable", withoutParametr, withoutParametr, out _);
        }

        ///<summary>
        ///Rename key by Client Id
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="name">New name for key</param>
        public async Task RenameKey(string clientId, string name) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("PUT", $"api/wireguard/client/{clientId}/name/", "name", name, out _);
        }

        ///<summary>
        ///Set new IP for connection by this key
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="address">IP Adress for connection</param>
        public async Task SetNewIp(string clientId, string address) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("PUT", $"api/wireguard/client/{clientId}/address/", "address", address, out _);
        }

        ///<summary>
        ///Download .config file for using key in WireGuard Client
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="path">Path for saving config file</param>
        public async Task DownloadConfig(string clientId, string path) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("POST", $"api/wireguard/client/{clientId}/configuration", out var data);
            await File.WriteAllBytesAsync($"{path}/{clientId}.conf", data);
        }

        ///<summary>
        ///Download QR-Code file for using key in WireGuard Mobile Client
        ///</summary>
        ///<param name="clientId">Id of client</param>
        ///<param name="path">Path for saving QR-Code in .svg</param>
        public async Task DownloadQrCode(string clientId, string path) {
            var status = await checkAuthrization();
            if(!status.Authenticated) {
                await makeRequest("POST", "api/session", "password", _password, out _);
                await updateCookieContainer(cash);
                await createSession();
            }
            await makeRequest("POST", $"api/wireguard/client/{clientId}/qrcode.svg", out var data);
            await File.WriteAllBytesAsync($"{path}/{clientId}.svg", data);
        }
    }
}

