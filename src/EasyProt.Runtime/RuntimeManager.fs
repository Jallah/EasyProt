namespace EasyProt.Runtime

open EasyProt.Core
open EasyProt.Runtime

//TODO: Add a Reconect()-method (see DefaultClient --> just Call ConnectAsync again)
//      Add a StopListening()-method
type Client(protClient : IProtClient, pipe : (IPipelineMember list * IPipelineMember list * IProtMessage) list) as this = 
    do protClient.OnIncomingMessage.AddHandler(fun o a -> this.IncomingMessageEventHandler(o, a))
    member private this.GetMessage msg = pipe |> List.tryFind (fun (_, _, message) -> message.Validate(msg))
    
    member this.SendAsync msg = 
        match this.GetMessage msg with
        | Some(_, outPipeline, _) -> async { 
                                         //get the pipeline result --> feed the pipe
                                         let! pipelineResult = (new Pipeline() :> IPipeline).RunAsync outPipeline msg
                                         do! protClient.SendAsync pipelineResult } //do System.Console.WriteLine(pipelineResult)
                                                                                   |> Async.StartAsTask
        | None -> failwith "No matching pipelinemember(s) or default pipelinemember(s) found" //TODO: Resources
    
    member this.ListenAsync() = protClient.ListenForMessageAsync |> Async.StartAsTask
    member this.ConnectAsync(ip, port) = protClient.ConnectAsync(ip, port) |> Async.StartAsTask
    member this.DisconnectAsync() = protClient.DisconnectAsync |> Async.StartAsTask
    member private this.IncomingMessageEventHandler(sender : obj, a : IncomingMessageEventArgs) = 
        match (a.Message |> this.GetMessage) with
        | Some(incPipeline, _, _ : IProtMessage) -> 
            async { 
                //get the pipeline result --> feed the pipe
                let! pipelineResult = (new Pipeline() :> IPipeline).RunAsync incPipeline a.Message
                // TODO: do something with pipelineResult
                () }
            |> Async.StartAsTask
            |> ignore
        | None -> failwith "No matching pipelinemember(s) or default pipelinemember(s) found" //TODO: Resources

//TODO: Add a StopListening()-method
//      Add a opportunity to use the IProtMessage(s) and IPipeline(s) for inc. and outgoing messages
type Server(protServer : IProtServer) = 
    
    [<CLIEvent>]
    member this.OnClientConnected = protServer.OnClientConnected
    
    member this.ListenForClientsAsync port = 
        protServer.ListenForClientsAsync port
        |> Async.StartAsTask
        |> ignore

//TODO: Add a opportunity to re-register a message
//      Add a opportunity to register a defaultHandler
type RuntimeManager(?client, ?server) = 
    // incoming               outgoing
    let mutable messages : (IPipelineMember list * IPipelineMember list * IProtMessage) list = []
    let client = defaultArg client (new DefaultProtClient() :> IProtClient)
    let server = defaultArg server (new DefaultProtServer() :> IProtServer)
    
    let defaultPipeline = 
        [ { new IPipelineMember with
                member this.Proceed input = input } ]
    
    new() = RuntimeManager(?client = None, ?server = None)
    member this.RegisterMessageInc incPipeline message = messages <- (incPipeline, defaultPipeline, message) :: messages
    member this.RegisterMessageOut outPipeline message = messages <- (defaultPipeline, outPipeline, message) :: messages
    member this.RegisterMessageIncOut incPipeline outPipeline message = 
        messages <- (incPipeline, outPipeline, message) :: messages
    member this.RegisterMessage message = messages <- (defaultPipeline, defaultPipeline, message) :: messages
    member this.GetProtClient() = new Client(client, messages)
    member this.GetProtServer() = new Server(server)
