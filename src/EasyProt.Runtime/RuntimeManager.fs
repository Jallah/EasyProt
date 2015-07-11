module EasyProt.Runtime.RuntimeManager


open EasyProt.Core
open EasyProt.Runtime




//TODO: Make it possible to Register a Pipeline for each Message
//if the User does not give list of PipelineMembers use the default pipeline (input = input)
type RuntimeManager(?pipelineMembers) =
    let pipeline = new Pipeline() :> IPipeline
    let mutable pipeMembers = defaultArg pipelineMembers ({new IPipelineMember with
                                                            member this.Proceed input = input})::[]

    member this.RunPipe input = async{ 
                                        let pipeWorkFlow = pipeline.RunAsync pipeMembers input
                                        let! pipeResult = pipeWorkFlow
                                        return pipeResult
                                    }  
