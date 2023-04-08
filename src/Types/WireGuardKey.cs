using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WgEasyManager.Types {
    public class WireGuardKey {
        public string Id;
        public string Name;
        public bool Enabled;
        public string Address;
        public string PublicKey;

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt;

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt;

        [JsonConverter(typeof(StringEnumConverter))]
        public OnlineStatus PersistentKeepAlive;

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LatestHandshakeAt;
        
        public long TransferRx;
        public long TransferTx;
    }
}
