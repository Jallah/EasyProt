// C#

// var manager = new Runtime.RuntimeManager();
//            var server = manager.GetProtServer();
//
//            server.OnClientConnected += 
//            (o, a) =>
//            {
//                Console.WriteLine("inc con: " + a.Client.Client.RemoteEndPoint.ToString());
//
//                var reader = new System.IO.StreamReader(a.Client.GetStream());
//
//                while (true)
//                {
//                    var msg = reader.ReadLine();
//                    Console.WriteLine(msg);
//                }
//
//            };
//
//            server.ListenForClientsAsync(8080);

open EasyProt.Runtime
open EasyProt.Core

[<EntryPoint>]
let main argv = 
    let manager = new RuntimeManager()
    let server = manager.GetProtServer()

    //TODO: make it possible to register messages for the Server
    //TODO: For inc messages --> IPipelineMemberInc.Procced: Stream string -> string


    let incMsgHandler =
        { new IPipelineMember with
          member this.Proceed message = "S " + message + " got it"}
    
    let incMsg = 
        { new IProtMessage with
          member this.Validate message = message.[0] = 'X'}

    let pipe = new Pipeline() :> IPipeline

    

    server.OnClientConnected.AddHandler(fun _ a ->
    
                 System.Console.WriteLine("inc con: " + a.Client.Client.RemoteEndPoint.ToString());

                 

                 let reader = new System.IO.StreamReader(a.Client.GetStream());
                 let writer = new System.IO.StreamWriter(a.Client.GetStream());
 
                 while true do
                    let msg = reader.ReadLine();
                    System.Console.WriteLine(msg);

                    if incMsg.Validate(msg) then writer.WriteLine(incMsgHandler.Proceed msg)
                    else writer.WriteLine("S unknow message")

                    writer.Flush()
               )

    server.ListenForClientsAsync(8080)

    System.Console.ReadLine() |> ignore
    0 
