namespace EasyProt.Core

///This interface ist just for convenience.
///Its a nice API for other .NET languages like C#.
type IPipelineMember =
    abstract member Proceed : string -> string


type IPipeline =
   ///Runs the Message pipeline.
   abstract member RunAsync : IPipelineMember list -> (string -> Async<string>)


type IncomingMessageEventArgs(message: string) =
    inherit System.EventArgs()
    let message = message
    member this.Message with get() = message


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
    abstract member ValidateAsync : message:string -> Async<bool>
    abstract member HandleMessageAsync : message:string -> Async<unit>

    

