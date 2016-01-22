using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyProt.Core;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.IO;
using EasyProt.Runtime;

namespace EasyProt.CSharpInterOpTestServer
{

    public class IncMsg1 : IProtMessage
    {
        public bool Validate(string message) => message[0] == 'X';
    }

    public class IncMsg2 : IProtMessage
    {
        public bool Validate(string message) => message[0] == '2';
    }

    public class Log : IPipelineMember
    {
        public string Proceed(string input)
        {
            Console.WriteLine(input);
            return input;
        }
    }

    public class outMsgHandler : IPipelineMember
    {
        public string Proceed(string input) => "S " + input + " got it";
    }

    public class PipeResponder : IPipelineResponder
    {
        public FSharpAsync<Unit> ResponseAsync(string res, StreamWriter writer)
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
            var outPipe = Helper.castToFsharpList(new List<IPipelineMember> { new outMsgHandler(), new Log() });
            var responder = new FSharpOption<IPipelineResponder>(new PipeResponder());

            var mngr = new RuntimeManager();
            mngr.RegisterMessageOut(outPipe, new IncMsg1(), responder);
            mngr.RegisterMessageOut(outPipe, new IncMsg2(), responder);
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
