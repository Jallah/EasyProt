module Program

open EasyProt.Runtime
open EasyProt.Core
open Helper
open EasyProt.Runtime.Pipe

//let (|Fail|) s v = failwith s
//let getAge (Some age | Fail "Age is missing! " age) = age

let incMsg1 = 
    { new  IProtMessage<string> with
          member __.Validate message = message.[0] = 'X' }

let incMsg2 = 
    { new  IProtMessage<string> with
          member __.Validate message = message.[0] = '2' }

let incMsg3 = 
    { new  IProtMessage<string> with
          member __.Validate message = message.[0] = '3' }

let log = 
    { new IPipelineMember<string,string> with
          member __.Proceed input = 
              System.Console.WriteLine(input)
              input }

let outMsgHandler = 
    { new IPipelineMember<string,string> with
          member __.Proceed input = "S " + input + " got it" }

let pipeResponder = 
    { new IPipelineResponder<string> with
          member __.ResponseAsync writer inPipeResult = 
              async { 
                  do! writer.WriteLineAsync(inPipeResult) |> awaitTaskVoid
                  do! writer.FlushAsync() |> awaitTaskVoid
              } }

let server = 
    let mngr = new RuntimeManager()

    let outPipe = outMsgHandler
                    .Then(log)
                    .CreatePipeWithResponse(pipeResponder)

    let inPipe = pipeResponder
                    .CreatePipeFromResponder()

    mngr.RegisterMessageOut outPipe incMsg1
    mngr.RegisterMessageOut outPipe incMsg2
    mngr.RegisterMessageInc inPipe  incMsg3 
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
