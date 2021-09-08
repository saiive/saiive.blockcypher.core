#region
using Newtonsoft.Json;

#endregion

namespace Saiive.BlockCypher.Core.Objects {
    public class Faucet : BaseObject {
        [JsonProperty("tx_ref")]
        public string TxReference { get; set; }
    }
}
