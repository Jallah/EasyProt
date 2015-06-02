module EasyProt.Runtime.Pipeline

open EasyProt.Core.Pipeline

type Pipeline() = 
    interface IPipeline with
        member this.RunAsync pipelineMember = 
            let pipe = 
                pipelineMember
                |> List.map (fun m -> m.ProceedAsync)
                |> List.reduce (>>)
            (fun input -> async { return pipe input })


type testPiepLineMember() = 
    interface IPipelineMember with
        member this.ProceedAsync input = input + "XX"


type testPiepLineMemberTwo() = 
    interface IPipelineMember with
        member this.ProceedAsync input = "XX" + input
                