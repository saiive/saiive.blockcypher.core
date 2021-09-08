using Newtonsoft.Json;

namespace Saiive.BlockCypher.Core.Objects
{
    internal class SendRawTxObject
    {
        [JsonProperty("tx")]
        public string TxHex { get; set; }
    }
}
