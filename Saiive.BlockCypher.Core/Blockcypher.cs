#region
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BlockCypher.Helpers;
using BlockCypher.Objects;
using BlockCypher.Pcl;

#endregion

namespace BlockCypher {
    public class Blockcypher {
        public Uri BaseUrl { get; set; }
        public Endpoint Endpoint { get; set; }
        public bool EnsureSuccessStatusCode { get; set; }
        public int ThrottleRequests { get; set; }
        public string UserToken { get; set; }

        public Blockcypher(string token = "", Endpoint endpoint = Endpoint.BtcMain) {
            UserToken = token;
            Endpoint = endpoint;

            switch (endpoint) {
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

        public Task<Faucet> Faucet(string address, Satoshi amount) {
            if (Endpoint != Endpoint.BcyTest)
                throw new Exception("Invalid endpoint: faucet is only allowed for BcyTest");

            return PostAsync<Faucet>("faucet", new {
                address,
                amount = (int) amount.Value
            });
        }

        public Task<AddressInfo> GenerateAddress() {
            return PostAsync<AddressInfo>("addrs", null);
        }

        public Task<HookInfo> GenerateHook(string address, HookEvent hook, string url) {
            try {
                string evt = "";

                switch (hook) {
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

                return PostAsync<HookInfo>("hooks", new {
                    @event = evt,
                    url,
                    address
                });
            } catch {
                return Task.Factory.StartNew(() => new HookInfo {
                    Error = "Hook already exists"
                });
            }
        }

        public Task<AddressBalance> GetBalanceForAddress(string address) {
            return GetAsync<AddressBalance>(string.Format("addrs/{0}", address));
        }

        public IEnumerable<Task<AddressBalance>> GetBalanceForAddresses(params string[] addresses) {
            return addresses.Select(GetBalanceForAddress);
        }

        public Task<Transaction[]> GetTransactions(AddressInfo fromAddress) {
            return GetTransactions(fromAddress.Public);
        }

        public async Task<Transaction[]> GetTransactions(string fromAddress) {
            var addressInfo = await GetBalanceForAddress(fromAddress);

            if (addressInfo.Transactions == null)
                return new Transaction[0];

            var txs = addressInfo.Transactions.Select(t => t.TxHash).Distinct().ToArray();
            var groups = txs.Select((x, i) => new {
                Key = i / 40,
                Value = x
            }).GroupBy(x => x.Key, x => x.Value, (k, g) => g.ToArray()).ToArray();

            var list = new List<Transaction>();

            foreach (string url in groups.Select(g => string.Format("txs/{0}", string.Join(";", g)))) {
                var transactions = await GetAsync<Transaction[]>(url);

                if (transactions != null)
                    list.AddRange(transactions);
            }

            return list.OrderBy(t => t.Confirmed).ToArray();
        }


        public class SendingHolder {
            public Satoshi Value { get; set; }
            public string Wallet { get; set; }

            public TxOutput ToTxn() {
                return new TxOutput {
                    Addresses = new[] {
                        Wallet
                    },
                    Value = Value
                };
            }
        }

        #region Helpers
        internal async Task<T> GetAsync<T>(string url) {
            var client = GetClient();

            var response = await client.GetAsync(string.Format("{0}/{1}", BaseUrl, url));

            if (EnsureSuccessStatusCode)
                response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            if (ThrottleRequests > 0)
                await Task.Delay(ThrottleRequests);

            return content.FromJson<T>();
        }

        internal HttpClient GetClient() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public static bool EnableLogging = true;
        public string LastResponse;

        internal async Task<T> PostAsync<T>(string url, object obj) where T : new() {
            var client = GetClient();

            string targetUrl = string.Format("{0}/{1}", BaseUrl, url);
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
                response.EnsureSuccessStatusCode();

            if (ThrottleRequests > 0)
                await Task.Delay(ThrottleRequests);

            return content.FromJson<T>();
        }
        #endregion
    }
}
