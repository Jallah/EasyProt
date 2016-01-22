module Helper

open System.Threading.Tasks
open EasyProt.Core

let awaitTaskVoid (t : Task) = 
    t
    |> Async.AwaitIAsyncResult
    |> Async.Ignore

//                       inpipeline             outpipeline          
let findMessage (pipe : (IPipelineMember list * IPipelineMember list * IProtMessage * Option<IPipelineResponder>) list) 
    msg = pipe |> List.tryFind (fun (_, _, message, _) -> message.Validate(msg))

let castToFsharpList l = List.ofSeq (l)

let combineVoidTasks tasks = 
    async { 
        for t in tasks do
           do! t |> awaitTaskVoid
    }
