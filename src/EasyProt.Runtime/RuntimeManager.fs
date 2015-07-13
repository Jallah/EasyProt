module EasyProt.Runtime.RuntimeManager


open EasyProt.Core
open EasyProt.Runtime


type Client(protClient: IProtClient, pipes : (IPipeline*IProtMessage) list) as this=
    do 
        protClient.OnIncomingMessage.AddHandler(fun _ a -> match (a.Message |> this.getMessage) with
                                                            | Some(_, message: IProtMessage) -> (message.HandleMessageAsync a.Message) |> Async.Start
                                                            | None -> failwith "No matching MessageHandler or DefaultHandler found" )

    member private this.getMessage msg = pipes |> List.tryFind (fun (_, message) -> async  {
                                                                                      let! isValid = message.ValidateAsync(msg)
                                                                                      return isValid
                                                                                    } |> Async.RunSynchronously)
    member this.SendAsync msg = protClient.SendAsync(msg) |> Async.StartAsTask
    member this.ListenAsync = protClient.ListenForMessageAsync |> Async.StartAsTask
    member this.ConnectAsync(ip, port) = protClient.ConnectAsync(ip, port) |> Async.StartAsTask
    member this.DisconnectAsync = protClient.DisconnectAsync |> Async.StartAsTask
    member private this.IncomingMessageEventHandler (sender: obj, a: IncomingMessageEventArgs) = 
        match (a.Message |> this.getMessage) with
        | Some(_, message) -> message.HandleMessageAsync a.Message|> Async.StartAsTask |> ignore
        | None -> failwith "No matching MessageHandler or DefaultHandler found"
        

//TODO: Make it possible to Register a Pipeline for each Message
//if the User does not give list of PipelineMembers use the default pipeline (input = input)
type RuntimeManager(?client) =
    let pipeline = new Pipeline() :> IPipeline //private member
    let mutable messages : (IPipeline*IProtMessage) list= []
    let mutable client = defaultArg client (new DefaultProtClient() :> IProtClient)
    let defaultPipeline = ({new IPipelineMember with
                             member this.Proceed input = input})::[]

//    member this.RunPipe input = async{ 
//                                        let pipeWorkFlow = pipeline.RunAsync pipeMembers input
//                                        let! pipeResult = pipeWorkFlow
//                                        return pipeResult
//                                     }  

    member this.RegisterMessage (messagePipeLine, message) = (messagePipeLine,message)::messages
    member this.RegisterMessage message = (defaultPipeline, message)
    //member this.GetProtClient = 