namespace EasyProt.Runtime


open EasyProt.Core
open EasyProt.Runtime

//TODO: Add a ReconectMethode (see DefaultClient --> just Call ConnectAsync again)
type Client(protClient: IProtClient, pipes : (IPipelineMember list*IProtMessage) list) as this=
    do 
        protClient.OnIncomingMessage.AddHandler(fun o a -> this.IncomingMessageEventHandler(o, a))

    member private this.getMessage msg = pipes |> List.tryFind (fun (_, message) ->  message.Validate(msg))
                                                                                      
    member this.SendAsync msg = match this.getMessage msg with
                                | Some(pipelineMembers, _) -> async{
                                                                     //get the pipeline result --> feed the pipe
                                                                     let! pipelineResult = (new Pipeline() :> IPipeline).RunAsync pipelineMembers msg
                                                                     do! protClient.SendAsync pipelineResult
                                                              } |> Async.StartAsTask
                                | None -> failwith "No matching pipelinemember(s) or default pipelinemember(s) found"
                                  

    member this.ListenAsync = protClient.ListenForMessageAsync |> Async.StartAsTask

    member this.ConnectAsync(ip, port) = protClient.ConnectAsync(ip, port) |> Async.StartAsTask

    member this.DisconnectAsync = protClient.DisconnectAsync |> Async.StartAsTask

    member private this.IncomingMessageEventHandler (sender: obj, a: IncomingMessageEventArgs) = 
        match (a.Message |> this.getMessage) with
        | Some(_, message: IProtMessage) -> (message.HandleMessageAsync a.Message) |> Async.StartAsTask |> ignore
        | None -> failwith "No matching MessageHandler or DefaultHandler found"


//TODO: Add a StopListening() member
type Server(protServer: IProtServer) =
    [<CLIEvent>]
    member this.OnClientConnected = protServer.OnClientConnected
    member this.ListenForClientsAsync port = protServer.ListenForClientsAsync port |> Async.StartAsTask |> ignore


//TODO: Make it possible to Register a Pipeline not just for outgoing messages but also for inc messages
type RuntimeManager(?client, ?server)=
    //let pipeline = new Pipeline() :> IPipeline //private member
    let mutable messages : (IPipelineMember list*IProtMessage) list= []
    let client = defaultArg client (new DefaultProtClient() :> IProtClient)
    let server = defaultArg server (new DefaultProtServer() :> IProtServer)
    let defaultPipeline = ({new IPipelineMember with
                             member this.Proceed input = input})::[]
    
    new() = RuntimeManager(?client = None, ?server = None)

    member this.RegisterMessage (messagePipeLine, message) = messages <- (messagePipeLine,message)::messages
                                                             
    member this.RegisterMessage message = messages <- (defaultPipeline, message)::messages

    member this.GetProtClient() = new Client(client, messages)

    member this.GetProtServer() = new Server(server)

    