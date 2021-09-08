#region
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Saiive.BlockCypher.Core.Objects {
    public class TxInput {
        [JsonProperty("addresses")]
        public IList<string> Addresses { get; set; }
    }
}
