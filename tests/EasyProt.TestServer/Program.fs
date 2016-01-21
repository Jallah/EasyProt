module Program

open EasyProt.Runtime
open EasyProt.Core
open Helper

let incMsg = 
    { new IProtMessage with
          member __.Validate message = message.[0] = 'X' }

let log = 
    { new IPipelineMember with
          member __.Proceed input = 
              System.Console.WriteLine(input)
              input }

let outMsgHandler = 
    { new IPipelineMember with
          member __.Proceed input = "S " + input + " got it" }

let pipeResponder = 
    { new IPipelineResponder with
          member __.ResponseAsync inPipeResult writer = 
              async { 
                  do! writer.WriteLineAsync(inPipeResult) |> awaitTaskVoid
                  do! writer.FlushAsync() |> awaitTaskVoid
              } }

let server = 
    let mngr = new RuntimeManager()
    mngr.RegisterMessageOut [ outMsgHandler; log ] incMsg (Some(pipeResponder))
    mngr.GetProtServer()

[<EntryPoint>]
let main argv = 
    server.OnClientConnected.AddHandler(fun _ a -> 
        let networkStream = a.ClientStream :?> System.Net.Sockets.NetworkStream
        // do something with the networkstream
        System.Console.WriteLine("inc con"))
    server.ListenForClientsAsync(8080)
    System.Console.ReadLine() |> ignore
    0
