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

let member1 = 
    { new IPipelineMember with
          member this.Proceed input = input + "XX" }

let member2 = 
    { new IPipelineMember with
          member this.Proceed input = "XX" + input }

let msg1 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = '1'
          member this.HandleMessageAsync message = async { System.Console.WriteLine("msg1: " + message) } }

let msg2 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = '2'
          member this.HandleMessageAsync message = async { System.Console.WriteLine("msg2: " + message) } }

let msg3 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = 'S'
          member this.HandleMessageAsync message = async { System.Console.WriteLine("ServerResponse: " + message) } }

[<EntryPoint>]
let main argv = 
    let rntMngr = new EasyProt.Runtime.RuntimeManager()

    rntMngr.RegisterMessage(member1 :: member2 :: [], msg1) |> ignore
    rntMngr.RegisterMessage(msg2) |> ignore
    rntMngr.RegisterMessage(msg3) |> ignore

    let client = rntMngr.GetProtClient()

    client.ConnectAsync("127.0.0.1", 8080).Wait()
    client.ListenAsync() |> ignore

    while true do
        let msg = System.Console.ReadLine()
        client.SendAsync(msg) |> ignore

    System.Console.ReadLine() |> ignore
    0
