namespace EasyProt.Runtime

open EasyProt.Core

type Pipeline() = 
    interface IPipeline with
        member __.RunAsync pipelineMember = 
            let pipe = 
                pipelineMember
                |> List.map (fun m -> m.Proceed)
                |> List.reduce (>>)
            (fun input -> async { return pipe input })

open Helper
open System.Net
open System.Net.Sockets
open System.IO

type DefaultProtClient() = 
    let mutable tcpClient : TcpClient = Unchecked.defaultof<TcpClient>
    let mutable streamWriter : StreamWriter = Unchecked.defaultof<StreamWriter>
    let incomingMessageEvent = new Event<IngomingMessageDelegate, IncomingMessageEventArgs>()
    
    member private __.DisposeStream() = 
        match streamWriter with
        | null -> ()
        | _ -> streamWriter.Close()
    
    interface IProtClient with
        
        [<CLIEvent>]
        member __.OnIncomingMessage = incomingMessageEvent.Publish
        
        member this.ListenForMessageAsync = 
            async { 
                let stream = tcpClient.GetStream()
                use streamReader = new StreamReader(stream)
                while true do
                    let! line = streamReader.ReadLineAsync() |> Async.AwaitTask
                    incomingMessageEvent.Trigger(this, new IncomingMessageEventArgs(line, stream))
                    ()
            }
        
        member this.ConnectAsync(ip, port) = 
            async { 
                do this.DisposeStream()
                do tcpClient <- new TcpClient()
                do! awaitTaskVoid (tcpClient.ConnectAsync(host = ip, port = port))
            }
        
        member __.SendAsync message = 
            match tcpClient.Connected with
            | true -> 
                async {
                    let writer = new StreamWriter(tcpClient.GetStream())
                    do! awaitTaskVoid (writer.WriteLineAsync(message.ToString()))
                    do! awaitTaskVoid (writer.FlushAsync())
                }
            | _ -> failwith "Client not connected"
        
        member __.DisconnectAsync = async { do tcpClient.Close() }
    
    interface System.IDisposable with
        member this.Dispose() = this.DisposeStream()

type DefaultProtServer() = 
    let clientConnectedEvent = new Event<ClientConnectedDelegate, ClientConnectedEventArgs>()

    interface IProtServer with
        
        [<CLIEvent>]
        member __.OnClientConnected = clientConnectedEvent.Publish
        
        member this.ListenForClientsAsync port = 
            async { 
                let listener = new TcpListener(IPAddress.Any, port)
                listener.Start()
                while true do
                    let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
                    clientConnectedEvent.Trigger(this, new ClientConnectedEventArgs(client))
            }
