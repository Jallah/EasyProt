namespace EasyProt.Runtime

open EasyProt.Core
open EasyProt.Runtime
open System.Collections.Concurrent
open System
open System.IO

//TODO: Add a Reconect()-method (see DefaultClient --> just Call ConnectAsync again)
//      Add a StopListening()-method
type Client(protClient : IProtClient,
            pipe : (IPipelineMember list * IPipelineMember list * IProtMessage * Option<IPipelineResponder>) list)
            as this = 
    do protClient.OnIncomingMessage.AddHandler(fun s a -> this.IncomingMessageEventHandler(s, a))
    member private __.GetMessage msg = Helper.findMessage pipe msg //pipe |> List.tryFind (fun (_, _, message,_) -> message.Validate(msg))
    
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
    member private this.IncomingMessageEventHandler ((sender : obj), a : IncomingMessageEventArgs) = 
        match (a.Message |> this.GetMessage) with
        | Some(incPipeline, _, _: IProtMessage, responder) ->
            async { 
                //get the pipeline result --> feed the pipe
                let! pipelineResult = (new Pipeline() :> IPipeline).RunAsync incPipeline a.Message
                match responder with
                | Some(responder) -> do! responder.ResponseAsync pipelineResult (new System.IO.StreamWriter(a.Stream))
                | None -> ()
                () }
            |> Async.StartAsTask
            |> ignore
        | None -> failwith "No matching pipelinemember(s) or default pipelinemember(s) found" //TODO: Resources


type ClientHandling(id: Guid,
                    reader: StreamReader,
                    disconnectAsync: Guid -> Async<unit>,
                    runInPipeAsync: string -> Async<unit>) =
    member __.ListenForMessagesAsync = async {
                                                while true do
                                                try
                                                    let! msg = reader.ReadLineAsync() |> Async.AwaitTask
                                                    do! runInPipeAsync msg
                                                with
                                                 | _ -> do! disconnectAsync id
                                              } |> Async.StartAsTask

//TODO: Add a StopListening()-method
//      Add a opportunity to use the IProtMessage(s) and IPipeline(s) for inc. and outgoing messages
type Server(protServer : IProtServer,
            pipe : (IPipelineMember list * IPipelineMember list * IProtMessage * Option<IPipelineResponder>) list)
            as this = 
    do protServer.OnClientConnected.AddHandler(fun s a -> this.OnIncomingConnectionHanlder(s, a))
    let clientHandler = new ConcurrentDictionary<Guid,ClientHandling>();
    
    [<CLIEvent>]
    member __.OnClientConnected = protServer.OnClientConnected
    member private __.RunInPipeAsync (stream: Stream) msg =
        match Helper.findMessage pipe msg with
        | Some(inPipeLine, outPipeline, _, responder) ->
          async { 
              //get the pipeline result --> feed the pipe
              let! inPipeResult = (new Pipeline() :> IPipeline).RunAsync inPipeLine msg
              match responder with
              | Some(resp) ->
                match outPipeline with
                | [] -> do! resp.ResponseAsync inPipeResult (new StreamWriter(stream))
                | pipe -> let! outPipeResult = (new Pipeline() :> IPipeline).RunAsync pipe inPipeResult // or msg ???
                          do! resp.ResponseAsync outPipeResult (new StreamWriter(stream))
              | _ -> ()}
        | None -> failwith "No matching pipelinemember(s) or default pipelinemember found" //TODO: Resources

    member private this.OnIncomingConnectionHanlder((_: obj), a: ClientConnectedEventArgs) =

        let client = new ClientHandling(Guid.NewGuid(),
                                                new StreamReader(a.ClientStream),
                                                (fun id -> async{ clientHandler.TryRemove id |> ignore}),
                                                this.RunInPipeAsync (a.ClientStream))
        client.ListenForMessagesAsync |> ignore // dont have to wait (do!)
        clientHandler.TryAdd(Guid.NewGuid(), client) |> ignore
                             
    
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
    member __.GetProtServer() = new Server(server, messages)
