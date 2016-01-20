namespace EasyProt.Runtime

open EasyProt.Core
open EasyProt.Runtime

//TODO: Add a Reconect()-method (see DefaultClient --> just Call ConnectAsync again)
//      Add a StopListening()-method
type Client(protClient : IProtClient, pipe : (IPipelineMember list * IPipelineMember list * IProtMessage * Option<IPipelineResponder>) list) as this = 
    do protClient.OnIncomingMessage.AddHandler(fun o a -> this.IncomingMessageEventHandler(o, a))
    member private __.GetMessage msg = pipe |> List.tryFind (fun (_, _, message,_) -> message.Validate(msg))
    
    member this.SendAsync msg = 
        match this.GetMessage msg with
        | Some(_, outPipeline, _, _) -> async { 
                                         //get the pipeline result --> feed the pipe
                                         let! pipelineResult = (new Pipeline() :> IPipeline).RunAsync outPipeline msg
                                         do! protClient.SendAsync pipelineResult } |> Async.StartAsTask
        | None -> 
        failwith "No matching pipelinemember(s) or default pipelinemember(s) found" //TODO: Resources
    
    member __.ListenAsync() = protClient.ListenForMessageAsync |> Async.StartAsTask
    member __.ConnectAsync(ip, port) = protClient.ConnectAsync(ip, port) |> Async.StartAsTask
    member __.DisconnectAsync() = protClient.DisconnectAsync |> Async.StartAsTask
    member private this.IncomingMessageEventHandler(sender : obj, a : IncomingMessageEventArgs) = 
        match (a.Message |> this.GetMessage) with
        | Some(incPipeline, _, _: IProtMessage, responder) ->
            async { 
                //get the pipeline result --> feed the pipe
                let! pipelineResult = (new Pipeline() :> IPipeline).RunAsync incPipeline a.Message
                match responder with
                | Some(responder) -> do! responder.Response pipelineResult (new System.IO.StreamWriter(a.Stream))
                | None -> ()
                () }
            |> Async.StartAsTask
            |> ignore
        | None -> failwith "No matching pipelinemember(s) or default pipelinemember(s) found" //TODO: Resources

//TODO: Add a StopListening()-method
//      Add a opportunity to use the IProtMessage(s) and IPipeline(s) for inc. and outgoing messages
type Server(protServer : IProtServer) = 
    
    [<CLIEvent>]
    member __.OnClientConnected = protServer.OnClientConnected
    
    member __.ListenForClientsAsync port = 
        protServer.ListenForClientsAsync port
        |> Async.StartAsTask
        |> ignore

//TODO: Add a opportunity to re-register a message
//      Add a opportunity to register a defaultHandler
type RuntimeManager(?client, ?server) = 
    //                      incoming               outgoing
    let mutable messages : (IPipelineMember list * IPipelineMember list * IProtMessage * Option<IPipelineResponder>) list = []
    let client = defaultArg client (new DefaultProtClient() :> IProtClient)
    let server = defaultArg server (new DefaultProtServer() :> IProtServer)
    
    let defaultPipeline = 
        [ { new IPipelineMember with
                member __.Proceed input = input } ]
    
    new() = RuntimeManager(?client = None, ?server = None)
    member __.RegisterMessageInc incPipeline message responder = messages <- (incPipeline, defaultPipeline, message , responder) :: messages
    member __.RegisterMessageOut outPipeline message responder = messages <- (defaultPipeline, outPipeline, message, responder) :: messages
    member __.RegisterMessageIncOut incPipeline outPipeline message responder = messages <- (incPipeline, outPipeline, message, responder) :: messages
    member __.RegisterMessage message responder= messages <- (defaultPipeline, defaultPipeline, message, responder) :: messages
    member __.GetProtClient() = new Client(client, messages)
    member __.GetProtServer() = new Server(server)
