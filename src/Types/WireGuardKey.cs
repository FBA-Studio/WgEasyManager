using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WgEasyManager.Types {
    ///<summary>
    /// Info about WireGuard key
    ///</summary>
    public class WireGuardKey {
        ///<value>Key ID</value>
        public string Id;

        ///<value>Name of key</value>
        public string Name;

        ///<value>Key status. <b>false</b>, if key is blocked</value>
        public bool Enabled;

        ///<value>IP Adrdress for connection</value>
        public string Address;

        ///<value>Public Key of WG Key</value>
        public string PublicKey;

        ///<value>Date of create</value>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt;

        ///<value>Date of last update</value>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt;

        ///<value>Key online status</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public OnlineStatus PersistentKeepAlive;

        ///<value>Latest using</value>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? LatestHandshakeAt;

        ///<value></value>
        public long TransferRx;

        ///<value></value>
        public long TransferTx;
    }
}
