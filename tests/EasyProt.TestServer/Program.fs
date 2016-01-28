module Program

open EasyProt.Runtime
open EasyProt.Core
open Helper

//let (|Fail|) s v = failwith s
//let getAge (Some age | Fail "Age is missing! " age) = age

let incMsg1 = 
    { new IProtMessage with
          member __.Validate message = message.[0] = 'X' }

let incMsg2 = 
    { new IProtMessage with
          member __.Validate message = message.[0] = '2' }

let incMsg3 = 
    { new IProtMessage with
          member __.Validate message = message.[0] = '3' }

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
    mngr.RegisterMessageOut [ outMsgHandler; log ] incMsg1 (Some(pipeResponder))
    mngr.RegisterMessageOut [ outMsgHandler; log ] incMsg2 (Some(pipeResponder))
    mngr.RegisterMessage incMsg3 (Some(pipeResponder))
    mngr.GetProtServer()

[<EntryPoint>]
let main argv = 
//    let x = getAge (Some(3))
//    let x1 = getAge None
    server.OnClientConnected.AddHandler(fun _ a -> 
        let networkStream = a.ClientStream :?> System.Net.Sockets.NetworkStream
        // do something with the networkstream
        System.Console.WriteLine("inc con"))

    server.ListenForClientsAsync(8080)
    System.Console.ReadLine() |> ignore
    0
