open EasyProt.Core
open EasyProt.Runtime

let member1 = {new IPipelineMember with
                    member this.ProceedAsync input = input + "XX"}

let member2 = {new IPipelineMember with
                    member this.ProceedAsync input = "XX" + input}


[<EntryPoint>]
let main argv = 

    let pipeline = new Pipeline()
    let pipeMembers = [member1 ; member2]
    let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hallo"
    let result = pipe |> Async.RunSynchronously
    printfn "%s" result
  
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
