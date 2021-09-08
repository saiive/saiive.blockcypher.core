using System;
using System.Threading.Tasks;

namespace Saiive.BlockCypher.Core.ConsoleA
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var instance = new Blockcypher("");

            var response = await instance.SendRawTransaction("01000000011935b41d12936df99d322ac8972b74ecff7b79408bbccaf1b2eb8015228beac8000000006b483045022100921fc36b911094280f07d8504a80fbab9b823a25f102e2bc69b14bcd369dfc7902200d07067d47f040e724b556e5bc3061af132d5a47bd96e901429d53c41e0f8cca012102152e2bb5b273561ece7bbe8b1df51a4c44f5ab0bc940c105045e2cc77e618044ffffffff0240420f00000000001976a9145fb1af31edd2aa5a2bbaa24f6043d6ec31f7e63288ac20da3c00000000001976a914efec6de6c253e657a9d5506a78ee48d89762fb3188ac00000000");

            var tx = await instance.GetUnspentTransactionReference("3GyJvBR93r68eJB6Kp6h8vra8RvekK8i8Y");
            Console.ReadLine();
        }
    }
}
