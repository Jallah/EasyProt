namespace EasyProt.Core

/// This interface ist just for convenience.
/// Its a nice API for other .NET languages like C#.
type IPipelineMember =
    abstract member Proceed : string -> string

type IPipelineResponder =
    abstract member Response: string -> System.IO.StreamWriter -> Async<unit>

type IPipeline =
   /// Runs the message Pipeline.
   abstract member RunAsync : IPipelineMember list -> (string -> Async<string>)

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
    abstract member SendAsync : message:string -> Async<unit>
    abstract member DisconnectAsync: Async<unit>
    abstract member ListenForMessageAsync: Async<unit>

type ClientConnectedEventArgs(client: System.Net.Sockets.TcpClient) =
    inherit System.EventArgs()
    let cl = client
    member this.Client with get() = cl
    
type ClientConnectedDelegate = delegate of obj * ClientConnectedEventArgs -> unit

type IProtServer =
    abstract member ListenForClientsAsync : port:int -> Async<unit>
    [<CLIEvent>]
    abstract member OnClientConnected : IEvent<ClientConnectedDelegate, ClientConnectedEventArgs>

type IProtMessage =
    /// This method is used to determine the message.
    abstract member Validate : message:string -> bool
//    // This methode will be called to handle incoming messages.
//    // Outgoing messages will be handled by the defined Pipeline wich you can register with RuntimeManager.RegisterMessage()
//    abstract member HandleMessageAsync : message:string -> Async<unit>

    

