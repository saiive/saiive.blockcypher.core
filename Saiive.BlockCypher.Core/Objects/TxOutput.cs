#region
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Saiive.BlockCypher.Core.Objects {
    public class TxOutput {
        [JsonProperty("addresses")]
        public IList<string> Addresses { get; set; }

        [JsonProperty("value")]
        public Satoshi Value { get; set; }
    }
}
