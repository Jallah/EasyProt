open EasyProt.Core
open EasyProt.Runtime

let member1 = {new IPipelineMember with
                    member this.Proceed input = input + "XX"}

let member2 = {new IPipelineMember with
                    member this.Proceed input = "XX" + input}


[<EntryPoint>]
let main argv = 

    let pipeline = new Pipeline()
    let pipeMembers = [member1 ; member2]
    let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hallo"
    let result = pipe |> Async.RunSynchronously
    printfn "%s" result
  
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
