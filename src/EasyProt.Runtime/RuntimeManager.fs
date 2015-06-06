module EasyProt.Runtime.RuntimeManager


open EasyProt.Core
open EasyProt.Runtime

type RuntimeManager(pipelineMembers) =
    let pipeline = new Pipeline() :> IPipeline

    member this.RunPipe input = async{ 
                                        let pipeWorkFlow = pipeline.RunAsync pipelineMembers input
                                        let! pipeResult = pipeWorkFlow
                                        return pipeResult
                                    }  
