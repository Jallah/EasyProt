using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyProt.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {

            //var manager = new Runtime.RuntimeManager.RuntimeManager(Microsoft.FSharp.Core.FSharpOption<Core.IProtClient>.None, Microsoft.FSharp.Core.FSharpOption<Core.IProtServer>.None);
            //
            var manager = new Runtime.RuntimeManager();
            var server = manager.GetProtServer();

            server.OnClientConnected += 
            (o, a) =>
            {
                Console.WriteLine("inc con: " + a.Client.Client.RemoteEndPoint.ToString());

                var reader = new System.IO.StreamReader(a.Client.GetStream());

                while (true)
                {
                    var msg = reader.ReadLine();
                    Console.WriteLine(msg);
                }

            };

            server.ListenForClientsAsync(8080);

            Console.ReadLine();
        }
    }
}
