namespace EasyProt.Runtime

open EasyProt.Core

type Pipeline() = 
    interface IPipeline with
        member this.RunAsync pipelineMember = 
            let pipe = 
                pipelineMember
                |> List.map (fun m -> m.ProceedAsync)
                |> List.reduce (>>)
            (fun input -> async { return pipe input })

open Helper
open System.Net
open System.Net.Sockets
open System.IO
open System.Threading.Tasks

type DefautlClient() =

    let tcpClient = new TcpClient()

    interface IProtClient with

        member this.ListenAsync = async{
            let stream = tcpClient.GetStream()
            use streamReader = new StreamReader(stream)

            while true do
                let! line = streamReader.ReadLineAsync() |> Async.AwaitTask
                ()
        }

        member this.ConnectAsync (ip, port) = async { do! awaitTaskVoid (tcpClient.ConnectAsync(host = ip, port = port))}

        member this.SendAsync message = match tcpClient.Connected with
                                        | true -> 
                                            async {
                                                let streamWriter = new StreamWriter(tcpClient.GetStream()) //use would close the baseStream --> dispose the stream elsewhere
                                                do! awaitTaskVoid (streamWriter.WriteLineAsync(message.ToString()))
                                                do! awaitTaskVoid (streamWriter.FlushAsync())
                                            }
                                        | _ -> failwith "client not connected"

        member this.DisconnectAsync = async { do tcpClient.Close()} 

    interface System.IDisposable with
        member this.Dispose() = tcpClient.Close()
        

type DefaultServer() =
    let clientConnectedEvent = new Event<ClientConnectedDelegate, ClientConnectedEventArgs>()

    interface IProtServer with
        [<CLIEvent>]
        member this.OnClientConnected = clientConnectedEvent.Publish

        member this.ListenForClientsAsync port = async { 
            let listener = new TcpListener(IPAddress.Any, port)
            listener.Start()
            while true do
                let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
                clientConnectedEvent.Trigger(this, new ClientConnectedEventArgs(client))
        }
        