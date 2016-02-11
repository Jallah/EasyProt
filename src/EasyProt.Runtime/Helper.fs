namespace EasyProt.Runtime
open Newtonsoft.Json

module Helper =
    open System.Threading.Tasks
    open EasyProt.Core
    
    let serializerSettings = 
        let settings = JsonSerializerSettings()
        settings.TypeNameHandling <- TypeNameHandling.All
        settings

    let compose (pMember1:IPipelineMember<'a, 'b>) (pMember2:IPipelineMember<'b, 'c>) =
        { new IPipelineMember<'a, 'c> with
              member __.Proceed input = pMember1.Proceed input |> pMember2.Proceed }
        
    let awaitTaskVoid (t : Task) = 
        t
        |> Async.AwaitIAsyncResult
        |> Async.Ignore
    
    //                           inpipeline  outpipeline          
    let findMessage (messages : (IPipeline * IPipeline * IProtMessage<string>) list) msg =
       messages
       |> List.tryFind (fun (_,_,pMsg) -> pMsg.Validate msg)
    
    let castToFsharpList l = List.ofSeq (l)
    
    let combineVoidTasks tasks = 
        async { 
            for t in tasks do
               do! t |> awaitTaskVoid
        }
