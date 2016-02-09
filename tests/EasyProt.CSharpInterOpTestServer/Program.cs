using System;
using System.Collections.Generic;
using EasyProt.Core;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.IO;
using EasyProt.Runtime;

namespace EasyProt.CSharpInterOpTestServer
{

    public class IncMsg1 : IProtMessage<string>
    {
        public bool Validate(string message) => message[0] == 'X';
    }

    public class IncMsg2 : IProtMessage<string>
    {
        public bool Validate(string message) => message[0] == '2';
    }

    public class Log : IPipelineMember<string, string>
    {
        public string Proceed(string input)
        {
            Console.WriteLine(input);
            return input;
        }
    }

    public class outMsgHandler : IPipelineMember<string, string>
    {
        public string Proceed(string input) => "S " + input + " got it";
    }

    public class PipeResponder : IPipelineResponder<string>
    {
        public FSharpAsync<Unit> ResponseAsync( StreamWriter writer, string res)
        {
            var t1 = writer.WriteLineAsync(res);
            var t2 = writer.FlushAsync();
            var voidTasks = Helper.combineVoidTasks(new List<System.Threading.Tasks.Task> { t1, t2 });
            return voidTasks;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var outPipe = (new outMsgHandler())
                            .Then(new Log())
                            .CreatePipe();

            var responder = new PipeResponder().CreatePipeFromResponder();

            var mngr = new RuntimeManager();
            mngr.RegisterMessageIncOut(outPipe, responder, new IncMsg1());
            mngr.RegisterMessageIncOut(outPipe, responder, new IncMsg2());
            var server = mngr.GetProtServer();

            server.OnClientConnected += (s, a) =>
            {
                var networkStream = a.ClientStream as System.Net.Sockets.NetworkStream;
                // do something with the networkstream
                Console.WriteLine("inc con");
            };

            server.ListenForClientsAsync(8080);
            Console.ReadLine();
        }
    }
}
