## Build status
**Master Branch**

[![Build Status](https://travis-ci.org/Jallah/EasyProt.svg?branch=master)](https://travis-ci.org/Jallah/EasyProt)
[![Build status](https://ci.appveyor.com/api/projects/status/cyi4mev19l12jyya?svg=true)](https://ci.appveyor.com/project/Jallah/easyprot)

## Description
EasyProt is a very lightwight Framwork for implementing your own Client-Server-Based Protocol. Usually you just have to define your messages by implementing the `IProtMessage ` interface. Some use case could be an instant messenger. The communication between the Clients and the Sever runs asynchronus. There are much things planned. This Framework is not yet complete.

## Usage
There are much cool things you can do to create your own Protocol. EasyProt is written in F# but you can write your implementation in every .NET Language even in VB .NET (don't take it too serious VB-Lovers :P). You can define a Pipeline (see below) for handling incoming messages and a Pipeline for outgoing messages. You can even write your own implementation for the Client-Server-Communication by implementing the ``IProtClient`` and/or ``IProtServer`` interface. The default implementation uses a simple ``TcpClient`` with an unencrypted stream. But you can write your own implementation which uses a ``SslStream``.  Let's have a look at the main parts of this small Framework:

**IPipelineMember**

This interface just consits of one method:
``` fsharp
// C# string Proceed(string message)
abstract member Proceed : string -> string
```
Let's look at some very simple implementations:
``` fsharp
let member1 = 
    { new IPipelineMember with
          member this.Proceed input = input + "XX" }

let member2 = 
    { new IPipelineMember with
          member this.Proceed input = "XX" + input }

let onServerResponse = { new IPipelineMember with 
                                member this.Proceed input = System.Console.WriteLine("ServerResponse: " + input)
                                                            input} 
```

**Or if you like C#:**

``` csharp
// ...
class member1 : IPipelineMember
{
    public string Proceed(string input) => input + "XX";
}
// ...
class member2 : IPipelineMember
{
    public string Proceed(string input) => "XX" + input;
}
// ...
class onServerResponse : IPipelineMember
{
    public string Proceed(string input)
    {
        System.Console.WriteLine("ServerResponse: " + input);
        return input;
    }
}
// ...
```

The result of these pipeline members (member1, member2) will be a string with leading and trailing **"XX"** (see screenshot below). The third one (onServerResponse) just writes the response to the console. Some real world example could be a member which logs the message somewhere. Or a member could act as an insult filter wich detects bad words and converts it into **$%+!?#&** or whatever. Some other implementation could convert from one format to another e.g. from XML to Json. It's a very flexible way to do some stuff with your outgoing or incoming messages with no limits being set to your imagination :).

**IPipelineResponder**

If you want to write a response after an icoming message you can write an ``IPipelineResponder``. The containing ``ResponseAsync`` methode gets the pipeline result and a ``StreamWriter``.
Let's look at a simple implementation. Also have a look at the ``EasyProt.TestServer`` project to see it in action.

**F#**
``` fsharp
let pipeResponder =
        { new IPipelineResponder with
          member __.ResponseAsync pipeResult writer =
            let response = "S " + pipeResult + " got it"
            async { 
                do! writer.WriteLineAsync(response) |> awaitTaskVoid
                do! writer.FlushAsync() |> awaitTaskVoid
                }}
```
**C#**
``` csharp
public class PipeResponder : IPipelineResponder
{
    public FSharpAsync<Unit> ResponseAsync(string res, StreamWriter writer)
    {
        var t1 = writer.WriteLineAsync(res);
        var t2 = writer.FlushAsync();

        var voidTasks = Helper.combineVoidTasks(new List<System.Threading.Tasks.Task> { t1, t2 });

        return voidTasks;
    }
}
```
Some interoperability code is planned so you don't have to add a reference to FSharp.Core to your C# project. Meanwhile you can use the Helper module.

**IProtMessage**

``` fsharp
// C# bool Validate(string message)
abstract member Validate : message:string -> bool
```
The ``Validate()`` method is responsible to determine the message. Let's look at a very simple implementation:

**F#**
``` fsharp
let msg1 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = '1'
```
**C#**
``` csharp
public class msg1 : IProtMessage
{
    public bool Validate(string message) => message[0] == '1';
}
```
So every time when the first sign of an incoming message is a **_1_** the Client or Server (depending on where you register your messages) knows which Pipeline will be used for incoming or outgoing messages.

**RuntimeManager**

After defining your messages and pipelines you should use the ``RuntimeManager`` as follows:

**F#**
``` fsharp
let rntMngr = new EasyProt.Runtime.RuntimeManager()
// Register a message with an OutGoing-Pipeline
rntMngr.RegisterMessageOut [member1 ; member2] msg1 None |> ignore
// Register a message with default-In- and default-Out-Pipeline
rntMngr.RegisterMessage msg2 None |> ignore
// Register a message with an Incoming-Pipeline
rntMngr.RegisterMessageInc [onServerResponse] serverResponse  None|> ignore
// There is also a RegisterMessageIncOut
```
**C#**
``` csharp
var rntMngr = new EasyProt.Runtime.RuntimeManager()
// Register a message with an OutGoing-Pipeline
rntMngr.RegisterMessageOut(new List<IPipelineMember>{member1 ; member2},  msg1, Microsoft.FSharp.Core.Option<IPipelineResponder>.None); 
// Register a message with default-In- and default-Out-Pipeline
rntMngr.RegisterMessage(msg2, Microsoft.FSharp.Core.Option<IPipelineResponder>.None);
// Register a message with an Incoming-Pipeline
rntMngr.RegisterMessageInc(new List<IPipelineMember>{onServerResponse}, serverResponse, Microsoft.FSharp.Core.Option<IPipelineResponder>.None);
// There is also a RegisterMessageIncOut
```
After registering your messages you can let the RuntimeManager Create the Client and/or Server for you:
``` fsharp
// ...
// C# var client = rntMngr.GetProtClient()
let client = rntMngr.GetProtClient()
```

``` fsharp
// ...
// C# var server = rntMngr.GetProtServer()
let server = rntMngr.GetProtServer()
```
The ``RuntimeManager``-ctor is overloaded so you can pass your own ``IProtClient`` and ``IProtServer`` implemenation. Otherwise the default ones will be used (EasyProt/src/EasyProt.Runtime/Runtime.fs).

**Client and Server**

Now you got the Client and Server you can start connecting them:

*Note: This is just sample code to demonstate the usage. For reason of clarity the exception handling has been omitted.*

**F# Server-side**
``` fsharp
module Program

open EasyProt.Runtime
open EasyProt.Core
open Helper

let incMsg1 = 
    { new IProtMessage with
          member __.Validate message = message.[0] = 'X' }

let incMsg2 = 
    { new IProtMessage with
          member __.Validate message = message.[0] = '2' }

let log = 
    { new IPipelineMember with
          member __.Proceed input = 
              System.Console.WriteLine(input)
              input }

let outMsgHandler = 
    { new IPipelineMember with
          member __.Proceed input = "S " + input + " got it" }

let pipeResponder = 
    { new IPipelineResponder with
          member __.ResponseAsync inPipeResult writer = 
              async { 
                  do! writer.WriteLineAsync(inPipeResult) |> awaitTaskVoid
                  do! writer.FlushAsync() |> awaitTaskVoid
              } }

let server = 
    let mngr = new RuntimeManager()
    mngr.RegisterMessageOut [ outMsgHandler; log ] incMsg1 (Some(pipeResponder))
    mngr.RegisterMessageOut [ outMsgHandler; log ] incMsg2 (Some(pipeResponder))
    mngr.GetProtServer()

[<EntryPoint>]
let main argv = 
    server.OnClientConnected.AddHandler(fun _ a -> 
        let networkStream = a.ClientStream :?> System.Net.Sockets.NetworkStream
        // do something with the networkstream
        System.Console.WriteLine("inc con"))
    server.ListenForClientsAsync(8080)
    System.Console.ReadLine() |> ignore
    0
```
**F# Client-side**
``` fsharp
module Program

open EasyProt.Core

let outMember1 = 
    { new IPipelineMember with
          member this.Proceed input = input + "XX" }

let outMember2 = 
    { new IPipelineMember with
          member this.Proceed input = "XX" + input }

let onServerResponse = 
    { new IPipelineMember with
          member this.Proceed input = 
              System.Console.WriteLine("ServerResponse: " + input)
              input }

let msg1 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = '1' }

let msg2 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = '2' }

let serverResponse = 
    { new IProtMessage with
          member this.Validate message = message.[0] = 'S' }

[<EntryPoint>]
let main argv = 
    let rntMngr = new EasyProt.Runtime.RuntimeManager()
    // Register a message with an OutGoing-Pipeline
    rntMngr.RegisterMessageOut [ outMember1; outMember2 ] msg1 None |> ignore
    // Register a message with default-In- and default-Out-Pipeline
    rntMngr.RegisterMessage msg2 None |> ignore
    // Register a message with an Incoming-Pipeline
    rntMngr.RegisterMessageInc [ onServerResponse ] serverResponse None |> ignore
    let client = rntMngr.GetProtClient()
    client.ConnectAsync("127.0.0.1", 8080).Wait()
    client.ListenAsync() |> ignore
    while true do
        let msg = System.Console.ReadLine()
        client.SendAsync(msg)
        |> Async.AwaitTask
        |> ignore
    System.Console.ReadLine() |> ignore
    0

```
**C# Client-side**
``` csharp
 // Pipeline member
public class OutMember1 : IPipelineMember
{
    public string Proceed(string input) => input + "XX";
}

public class OutMember2 : IPipelineMember
{
    public string Proceed(string input) => "XX" + input;
}

public class OnServerResponse : IPipelineMember
{
    public string Proceed(string input)
    {
        Console.WriteLine("ServerResponse: " + input);
        return input;
    }
}

// Messags
public class Msg1 : IProtMessage
{
    public bool Validate(string message) => message[0] == '1';
}

public class Msg2 : IProtMessage
{
    public bool Validate(string message) => message[0] == '2';
}

public class ServerResponse : IProtMessage
{
    public bool Validate(string message) => message[0] == 'S';
}


class Program
{
    static void Main(string[] args)
    {
        var msg1OutPipe = Helper.castToFsharpList(new List<IPipelineMember> { new OutMember1(), new OutMember2() });
        var serverResponsePipe = Helper.castToFsharpList(new List<IPipelineMember> { new OnServerResponse() });
        var nonResponder = FSharpOption<IPipelineResponder>.None;

        var rntMngr = new EasyProt.Runtime.RuntimeManager();
        // Register a message with an OutGoing-Pipeline
        rntMngr.RegisterMessageOut(msg1OutPipe, new Msg1(), nonResponder);
        // Register a message with default-In- and default-Out-Pipeline
        rntMngr.RegisterMessage(new Msg2(), nonResponder);
        // Register a message with an Incoming-Pipeline
        rntMngr.RegisterMessageInc(serverResponsePipe, new ServerResponse(), nonResponder);

        var client = rntMngr.GetProtClient();
        client.ConnectAsync("127.0.0.1", 8080).Wait();
        client.ListenAsync();

        while (true)
        {
            var msg = System.Console.ReadLine();
            client.SendAsync(msg).Wait();
        }
    }
}
```
**C# Server-side**
``` csharp
public class IncMsg1 : IProtMessage
{
    public bool Validate(string message) => message[0] == 'X';
}

public class IncMsg2 : IProtMessage
{
    public bool Validate(string message) => message[0] == '2';
}

public class Log : IPipelineMember
{
    public string Proceed(string input)
    {
        Console.WriteLine(input);
        return input;
    }
}

public class outMsgHandler : IPipelineMember
{
    public string Proceed(string input) => "S " + input + " got it";
}

public class PipeResponder : IPipelineResponder
{
    public FSharpAsync<Unit> ResponseAsync(string res, StreamWriter writer)
    {
        var t1 = writer.WriteLineAsync(res);
        var t2 = writer.FlushAsync();

        var voidTasks = Helper.combineVoidTasks(new List<System.Threading.Tasks.Task> { t1, t2 });

        return voidTasks;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var outPipe = Helper.castToFsharpList(new List<IPipelineMember> { new outMsgHandler(), new Log() });
        var responder = new FSharpOption<IPipelineResponder>(new PipeResponder());

        var mngr = new RuntimeManager();
        mngr.RegisterMessageOut(outPipe, new IncMsg1(), responder);
        mngr.RegisterMessageOut(outPipe, new IncMsg2(), responder);
        var server = mngr.GetProtServer();

        server.OnClientConnected += (s, a) =>
        {
            var networkStream = a.ClientStream as System.Net.Sockets.NetworkStream;
            // do something with the networkstream
            Console.WriteLine("inc con");
        };

        server.ListenForClientsAsync(8080);
        Console.ReadLine();
    }
}
```
**Sending messages:**
``` fsharp
// ...
// F#
client.SendAsync("Hey, dude!") |> ignore
```
``` csharp
// ...
// C#
client.SendAsync("Hey, dude!")
```

You can find all these samples in [here](https://github.com/Jallah/EasyProt/tree/master/tests). Following you will see a screenshot of the including test client(s) (left side) and test server(s) (right side):

![alt tag](https://github.com/Jallah/EasyProt/blob/master/docs/files/img/ClientServer.jpg)

Some more instructions will follow soon ...

## How to build

Just run build.cmd for Windows or build.sh for Linux.

A NuGet-Package is planned.

## TODO
- Make it possible to register a default handler for unknown messages
- Make it possible to reRegister a message
- Shift the piplines and responder to the IPropMessage type
- Add a flow chart image for the communication

## ISSUES
- FAKE does not support NUNIT >=3 yet: https://github.com/fsharp/FAKE/issues/1010




  