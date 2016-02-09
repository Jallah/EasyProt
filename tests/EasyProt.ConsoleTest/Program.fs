//    let l1 = [0..5]
//    let l2 = 6::l1
//    let pipeline = new Pipeline()
//    let pipeMembers = [member1 ; member2]
//    let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hallo"
//    let result = pipe |> Async.RunSynchronously
//    printfn "%s" result
// return an integer exit code
module Program

open EasyProt.Core
open EasyProt.Runtime.Pipe

let outMember1 = 
    { new IPipelineMember<string,string> with
          member this.Proceed input = input + "XX" }

let outMember2 = 
    { new  IPipelineMember<string,string> with
          member this.Proceed input = "XX" + input }

let onServerResponse = 
    { new  IPipelineMember<string,string> with
          member this.Proceed input = 
              System.Console.WriteLine("ServerResponse: " + input)
              input }

let msg1 = 
    { new  IProtMessage<string> with
          member this.Validate message = message.[0] = '1' }

let msg2 = 
    { new  IProtMessage<string> with
          member this.Validate message = message.[0] = '2' }

let msg3 = 
    { new  IProtMessage<string> with
          member this.Validate message = message.[0] = '3' }

let serverResponse = 
    { new  IProtMessage<string> with
          member this.Validate message = message.[0] = 'S' }

[<EntryPoint>]
let main argv = 

        //let m = msg2.AddTypeCheck()
        let outPipeMsg1 = outMember1
                            .Then(outMember2)
                            .CreatePipe()

        let rntMngr = new EasyProt.Runtime.RuntimeManager()
        // Register a message with an OutGoing-Pipeline
        rntMngr.RegisterMessageOut outPipeMsg1 msg1
        // Register a message with default-In- and default-Out-Pipeline
        rntMngr.RegisterMessage msg2
        // Register a message with an Incoming-Pipeline
        rntMngr.RegisterMessageInc (onServerResponse.CreatePipe()) serverResponse
        rntMngr.RegisterMessageInc (onServerResponse.CreatePipe()) msg3

        let client = rntMngr.GetProtClient()
        client.ConnectAsync("127.0.0.1", 8080).Wait()
        client.ListenAsync() |> ignore
        while true do
            let msg = System.Console.ReadLine()
            client.SendAsync(msg)
            |> Async.AwaitTask
            |> ignore

        System.Console.ReadLine() |> ignore
        0
