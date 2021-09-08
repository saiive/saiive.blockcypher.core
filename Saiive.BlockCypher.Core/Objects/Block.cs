using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Saiive.BlockCypher.Core.Objects
{
    public class Block
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("fees")]
        public int Fees { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("vsize")]
        public int Vsize { get; set; }

        [JsonProperty("ver")]
        public int Ver { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("received_time")]
        public DateTime ReceivedTime { get; set; }

        [JsonProperty("coinbase_addr")]
        public string CoinbaseAddr { get; set; }

        [JsonProperty("relayed_by")]
        public string RelayedBy { get; set; }

        [JsonProperty("bits")]
        public int Bits { get; set; }

        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [JsonProperty("n_tx")]
        public int NTx { get; set; }

        [JsonProperty("prev_block")]
        public string PrevBlock { get; set; }

        [JsonProperty("mrkl_root")]
        public string MrklRoot { get; set; }

        [JsonProperty("txids")]
        public List<string> Txids { get; set; }

        [JsonProperty("depth")]
        public int Depth { get; set; }

        [JsonProperty("prev_block_url")]
        public string PrevBlockUrl { get; set; }

        [JsonProperty("tx_url")]
        public string TxUrl { get; set; }

        [JsonProperty("next_txids")]
        public string NextTxids { get; set; }
    }
}
