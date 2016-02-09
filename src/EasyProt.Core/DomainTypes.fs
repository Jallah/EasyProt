namespace EasyProt.Core

/// This interface ist just for convenience.
/// Its a nice API for other .NET languages like C#.
type IPipelineMember<'a,'b> =
    abstract member Proceed : input:'a -> 'b

type IPipelineResponder<'a> =
    abstract member ResponseAsync: System.IO.StreamWriter -> 'a -> Async<unit>

type IPipeline =
   // Runs the message Pipeline.
   abstract member Run : input:string -> stream:System.IO.Stream -> unit

type IncomingMessageEventArgs(message: string, stream: System.IO.Stream) =
    inherit System.EventArgs()
    //let message = message
    member __.Message with get() = message
    member __.Stream with get() = stream

type IngomingMessageDelegate = delegate of obj * IncomingMessageEventArgs -> unit

type IProtClient =
    [<CLIEvent>]
    abstract member OnIncomingMessage : IEvent<IngomingMessageDelegate, IncomingMessageEventArgs>
    abstract member ConnectAsync : ip:string * port:int -> Async<unit>
    abstract member SendAsync : message:obj -> Async<unit>
    abstract member DisconnectAsync: Async<unit>
    abstract member ListenForMessageAsync: Async<unit>

type ClientConnectedEventArgs(clientStream: System.IO.Stream) =
    inherit System.EventArgs()
    let clStream = clientStream
    member this.ClientStream with get() = clStream
    
type ClientConnectedDelegate = delegate of obj * ClientConnectedEventArgs -> unit

type IProtServer =
    abstract member ListenForClientsAsync : port:int -> Async<unit>
    [<CLIEvent>]
    abstract member OnClientConnected : IEvent<ClientConnectedDelegate, ClientConnectedEventArgs>

type IProtMessage<'a> =
//    let outpipe = defaultArg outpipe ({new IPipeline with member __.Run _ = ()})
//    let inpipe = defaultArg inpipe ({new IPipeline with member __.Run _ = ()})
//    new() = IProtMessage(?outpipe = None, ?inpipe = None)
    abstract member Validate: message:'a -> bool
//    member __.OutPipe with get() = outpipe
//    member __.InPipe with get() = inpipe

    

//[<AbstractClass>]
//type IProtMessage<'a> =
//    abstract member Validate: message:'a -> bool
//    default Validate message = 
//
//    inherit IProtMessage
//    override this.InputTypeCheck message = message :? 'a
    

    
    

//type IProtMessage with
//    member 

type Message = 
    {ProtMsgType:System.Type; Content:obj}

//    // This methode will be called to handle incoming messages.
//    // Outgoing messages will be handled by the defined Pipeline wich you can register with RuntimeManager.RegisterMessage()
//    abstract member HandleMessageAsync : message:string -> Async<unit>




