open EasyProt.Core
open EasyProt.Runtime

let member1 = {new IPipelineMember with
                    member this.Proceed input = input + "XX"}

let member2 = {new IPipelineMember with
                    member this.Proceed input = "XX" + input}


let msg1 = { new IProtMessage with 
                member this.ValidateAsync message = async { return message.[0] = '1' }
                member this.HandleMessageAsync message = async { System.Console.WriteLine("msg1: " + message) } }

let msg2 = { new IProtMessage with 
                member this.ValidateAsync message = async { return message.[0] = '2' }
                member this.HandleMessageAsync message = async { System.Console.WriteLine("msg2: " + message) } }

                 
[<EntryPoint>]
let main argv = 

    let rntMngr = new EasyProt.Runtime.RuntimeManager();

    rntMngr.RegisterMessage(member1::member2::[], msg1) |> ignore
    rntMngr.RegisterMessage(msg2) |> ignore

    let client = rntMngr.GetProtClient()

    client.ConnectAsync("127.0.0.1", 8080) |> ignore

    while true
        do
            let msg = System.Console.ReadLine()
            client.SendAsync(msg) |> ignore


//    let pipeline = new Pipeline()
//    let pipeMembers = [member1 ; member2]
//    let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hallo"
//    let result = pipe |> Async.RunSynchronously
//    printfn "%s" result
  
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
