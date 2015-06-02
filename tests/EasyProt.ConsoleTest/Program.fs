// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open EasyProt.Runtime.Pipeline
open EasyProt.Core.Pipeline

[<EntryPoint>]
let main argv = 

    let pipeline = new Pipeline()
    let pipeMembers = [new testPiepLineMember() :> IPipelineMember ; new testPiepLineMemberTwo() :> IPipelineMember]
    let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hallo"
    let result = pipe |> Async.RunSynchronously
    printfn "%s" result
  
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
