using System;
using System.Collections.Generic;
using System.IO;
using EasyProt.Core;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace EasyProt.CSharpInterOpTest
{

    // Pipeline member
    public class OutMember1 : IPipelineMember
    {
        public string Proceed(string input) => input + "XX";
    }

    public class OutMember2 : IPipelineMember
    {
        public string Proceed(string input) => "XX" + input;
    }

    public class OnServerResponse : IPipelineMember
    {
        public string Proceed(string input)
        {
            Console.WriteLine("ServerResponse: " + input);
            return input;
        }
    }

    // Messags
    public class Msg1 : IProtMessage
    {
        public bool Validate(string message) => message[0] == '1';
    }

    public class Msg2 : IProtMessage
    {
        public bool Validate(string message) => message[0] == '2';
    }

    public class ServerResponse : IProtMessage
    {
        public bool Validate(string message) => message[0] == 'S';
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            var msg1OutPipe = Helper.castToFsharpList(new List<IPipelineMember> { new OutMember1(), new OutMember2() });
            var serverResponsePipe = Helper.castToFsharpList(new List<IPipelineMember> { new OnServerResponse() });
            var nonResponder = FSharpOption<IPipelineResponder>.None;

            var rntMngr = new EasyProt.Runtime.RuntimeManager();
            // Register a message with an OutGoing-Pipeline
            rntMngr.RegisterMessageOut(msg1OutPipe, new Msg1(), nonResponder);
            // Register a message with default-In- and default-Out-Pipeline
            rntMngr.RegisterMessage(new Msg2(), nonResponder);
            // Register a message with an Incoming-Pipeline
            rntMngr.RegisterMessageInc(serverResponsePipe, new ServerResponse(), nonResponder);

            var client = rntMngr.GetProtClient();
            client.ConnectAsync("127.0.0.1", 8080).Wait();
            client.ListenAsync();

            while (true)
            {
                var msg = System.Console.ReadLine();
                client.SendAsync(msg).Wait();
            }
        }
    }
}
