using System;
using System.Threading.Tasks;

namespace Saiive.BlockCypher.Core.ConsoleA
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var instance = new Blockcypher("", Endpoint.BtcTest3);

            var txs = await instance.GetTransactions("2N9mnZJzKcd7D4XmdRGbryYZ9y9WoknvFrA");
            var txs2 = await instance.GetTransactions("2Mtka3uLZCq94Ezyb8qxGyH7rpeu4GDnLzi");

            var response = await instance.SendRawTransaction("02000000000101420390fd05aa57b4fb8a2d80e293dca767b1bdac64ef81b88f394a80f01896170000000000ffffffff011085010000000000160014fe249ea942236e74ad4763e0e7acaf9e2cdfc8630247304402205cc6c8224e0587a4ba2991e0b6ff001a9e8b865ca1fdbcc430cdf5fb6c0f242002201a3d1b905f44f5624fdc511c825b7bb6faad9688850a6f0ce1a0551121e50d46012103b8e5ae051b1c56fa7b05081cffd064a9d026bf0583cd3c68a21025f3e45d14ff00000000");

            var tx = await instance.GetUnspentTransactionReference("3GyJvBR93r68eJB6Kp6h8vra8RvekK8i8Y");
            Console.ReadLine();
        }
    }
}
