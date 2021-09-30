#region
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Saiive.BlockCypher.Core.Objects;
using Saiive.BlockCypher.Core.Pcl;
#endregion

namespace Saiive.BlockCypher.Core
{
    public class Blockcypher
    {
        public Uri BaseUrl { get; set; }
        public Endpoint Endpoint { get; set; }
        public bool EnsureSuccessStatusCode { get; set; }
        public int ThrottleRequests { get; set; }
        public string UserToken { get; set; }

        public Blockcypher(string token = "", Endpoint endpoint = Endpoint.BtcMain)
        {
            UserToken = token;
            Endpoint = endpoint;

            switch (endpoint)
            {
                case Endpoint.BcyTest:
                    BaseUrl = new Uri("https://api.blockcypher.com/v1/bcy/test");
                    break;

                case Endpoint.BtcMain:
                    BaseUrl = new Uri("https://api.blockcypher.com/v1/btc/main");
                    break;

                case Endpoint.BtcTest3:
                    BaseUrl = new Uri("https://api.blockcypher.com/v1/btc/test3");
                    break;

                case Endpoint.LtcMain:
                    BaseUrl = new Uri("https://api.blockcypher.com/v1/ltc/main");
                    break;

                case Endpoint.UroMain:
                    BaseUrl = new Uri("https://api.blockcypher.com/v1/uro/main");
                    break;

                case Endpoint.DogeMain:
                    BaseUrl = new Uri("https://api.blockcypher.com/v1/doge/main");
                    break;
            }
        }

        public Task<Faucet> Faucet(string address, Satoshi amount)
        {
            if (Endpoint != Endpoint.BcyTest)
                throw new Exception("Invalid endpoint: faucet is only allowed for BcyTest");

            return PostAsync<Faucet>("faucet", new
            {
                address,
                amount = (int)amount.Value
            });
        }

        public Task<AddressInfo> GenerateAddress()
        {
            return PostAsync<AddressInfo>("addrs", null);
        }

        public Task<HookInfo> GenerateHook(string address, HookEvent hook, string url)
        {
            try
            {
                string evt = "";

                switch (hook)
                {
                    case HookEvent.ConfirmedTransaction:
                        evt = "confirmed-tx";
                        break;

                    case HookEvent.DoubleSpendTransaction:
                        evt = "double-spend-tx";
                        break;

                    case HookEvent.NewBlock:
                        evt = "new-block";
                        break;

                    case HookEvent.TransactionConfirmation:
                        evt = "tx-confirmation";
                        break;

                    case HookEvent.UnconfirmedTransaction:
                        evt = "unconfirmed-tx";
                        break;
                }

                return PostAsync<HookInfo>("hooks", new
                {
                    @event = evt,
                    url,
                    address
                });
            }
            catch
            {
                return Task.Factory.StartNew(() => new HookInfo
                {
                    Error = "Hook already exists"
                });
            }
        }

        public Task<AddressBalance> GetBalanceForAddress(string address)
        {
            return GetAsync<AddressBalance>(string.Format("addrs/{0}", address));
        }

        public async Task<IList<TxReference>> GetUnspentTransactionReference(string address)
        {
            var addrBalance = await GetAsync<AddressBalance>(string.Format("addrs/{0}", address), "unspentOnly=true");

            return addrBalance.Transactions;
        }

        public IEnumerable<Task<AddressBalance>> GetBalanceForAddresses(params string[] addresses)
        {
            return addresses.Select(GetBalanceForAddress);
        }

        public Task<Transaction[]> GetTransactions(AddressInfo fromAddress)
        {
            return GetTransactions(fromAddress.Public);
        }

        public Task<Transaction> GetTransactionByHash(string txHash)
        {
            return GetAsync<Transaction>($"txs/{txHash}");
        }

        public async Task<IList<Transaction>> GetTransactionsByBlockHash(string blockHash)
        {
            var ret = new List<Transaction>();

            var block = await GetAsync<Block>($"blocks/{blockHash}");

            foreach (var txId in block.Txids)
            {
                var tx = await GetTransactionByHash(txId);
                ret.Add(tx);
            }

            return ret;
        }
        public async Task<IList<Transaction>> GetTransactionsByBlockHeight(long height)
        {
            var ret = new List<Transaction>();

            var block = await GetAsync<Block>($"blocks/{height}");

            foreach (var txId in block.Txids)
            {
                var tx = await GetTransactionByHash(txId);
                ret.Add(tx);
            }

            return ret;
        }

        public Task<Stats> GetStats()
        {
            return GetAsync<Stats>("");
        }

        public Task<Block> GetBlockByHeight(long height)
        {
            return GetAsync<Block>($"blocks/{height}");
        }

        public Task<Block> GetBlockByHash(string blockHash)
        {
            return GetAsync<Block>($"blocks/{blockHash}");
        }

        public Task<Transaction> SendRawTransaction(string hex)
        {
            var sendTxObject = new SendRawTxObject();
            sendTxObject.TxHex = hex;
            return PostAsync<Transaction>("txs/push", sendTxObject);
        }

        public async Task<Transaction[]> GetTransactions(string fromAddress)
        {
            var addressInfo = await GetBalanceForAddress(fromAddress);

            if (addressInfo.Transactions == null)
                return new Transaction[0];

            var txs = addressInfo.Transactions.Select(t => t.TxHash).Distinct().ToArray();
            var groups = txs.Select((x, i) => new
            {
                Key = i / 40,
                Value = x
            }).GroupBy(x => x.Key, x => x.Value, (k, g) => g.ToArray()).ToArray();

            var list = new List<Transaction>();

            foreach (var url in groups.Select(g => g.ToList()))
            {
                var txsUrl = string.Format("txs/{0}", string.Join(";", url));
                if (url.Count == 1)
                {
                    var transactions = await GetAsync<Transaction>(txsUrl);

                    if (transactions != null)
                        list.Add(transactions);
                }
                else
                {
                    var transactions = await GetAsync<Transaction[]>(txsUrl);

                    if (transactions != null)
                        list.AddRange(transactions);
                }
            }

            return list.OrderBy(t => t.Confirmed).ToArray();
        }


        public class SendingHolder
        {
            public Satoshi Value { get; set; }
            public string Wallet { get; set; }

            public TxOutput ToTxn()
            {
                return new TxOutput
                {
                    Addresses = new[] {
                        Wallet
                    },
                    Value = Value
                };
            }
        }

        private string GetUrl(string url, string queryParams)
        {
            var retUrl = $"{BaseUrl}/{url}";
            if(!String.IsNullOrEmpty(UserToken))
            {
                if(String.IsNullOrEmpty(queryParams))
                {
                    queryParams = $"token={UserToken}";
                }
                else
                {
                    queryParams += $"&token={UserToken}";
                }
            }
            return $"{retUrl}?{queryParams}";
        }

        #region Helpers
        internal async Task<T> GetAsync<T>(string url, string queryParams = "")
        {
            var client = GetClient();

            var response = await client.GetAsync(GetUrl(url, queryParams));

            if (EnsureSuccessStatusCode)
                response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            if (ThrottleRequests > 0)
                await Task.Delay(ThrottleRequests);

            return content.FromJson<T>();
        }

        internal HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public static bool EnableLogging = true;
        public string LastResponse;

        internal async Task<T> PostAsync<T>(string url, object obj) where T : new()
        {
            var client = GetClient();

            string targetUrl = GetUrl(url, null);
            string requestJson = (obj ?? new object()).ToJson();
            if (EnableLogging)
                Debug.WriteLine("BlockCypher Request -> {0}\n{1}", targetUrl, requestJson);

            var response =
                await client.PostAsync(targetUrl, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            string content = await response.Content.ReadAsStringAsync();
            LastResponse = content;
            if (EnableLogging)
                Debug.WriteLine("BlockCypher Response:\n{0}", content);

            if (EnsureSuccessStatusCode)
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch(HttpRequestException ex)
                {
                    throw new EndpointException(content, response.StatusCode, ex);
                }
            }

            if (ThrottleRequests > 0)
                await Task.Delay(ThrottleRequests);

            return content.FromJson<T>();
        }
        #endregion
    }
}
