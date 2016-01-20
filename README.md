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

The result of these pipeline members (member1, member2) will be a string with leading and trailing **"XX"** (see screenshot below). The third one (onServerResponse) just writes the response to the console. Some real world example could be a member which logs the message somewhere. Or a member could act as an insult filter wich detects bad words and converts it into **$%+!?#&** or whatever. Some other implementation could convert from one format to another e.g. from XML to Json. It's a very flexible way to do some stuff with your outgoing messages with no limits being set to your imagination :).

**IPipelineResponder**

If you want to write a response after an icoming message you can write an ``IPipelineResponder``. The containing ``Response`` methode gets the pipeline result and a ``StreamWriter``.
Let's look at a simple implementation. Also have a look at the ``EasyProt.TestServer`` project to see it in action.

``` fsharp
let pipeResponder =
        { new IPipelineResponder with
          member __.Response pipeResult writer =
            let response = "S " + pipeResult + " got it"
            async { 
                do! writer.WriteLineAsync(response) |> awaitTaskVoid
                do! writer.FlushAsync() |> awaitTaskVoid
                }}
```

A C# example will follow.

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
server.OnClientConnected.AddHandler(fun _ a ->

  System.Console.WriteLine("inc con: " + a.Client.Client.RemoteEndPoint.ToString());
  let reader = new System.IO.StreamReader(a.Client.GetStream());
  let writer = new System.IO.StreamWriter(a.Client.GetStream());
 
   while true do
      let msg = reader.ReadLine();
      System.Console.WriteLine(msg);
      writer.WriteLine("S " + msg + " got it")
      writer.Flush())

server.ListenForClientsAsync(8080)
```
**F# Client-side**
``` fsharp
client.ConnectAsync("127.0.0.1", 8080).Wait()
client.ListenAsync() |> ignore
```
**C# Server-side**
``` csharp
server.OnClientConnected += (sender, args) =>
{
    System.Console.WriteLine("inc con: " + a.Client.Client.RemoteEndPoint.ToString());
    var reader = new System.IO.StreamReader(a.Client.GetStream());
    var writer = new System.IO.StreamWriter(a.Client.GetStream());
 
    while(true)
    {
        var msg = reader.ReadLine();
        System.Console.WriteLine(msg);
        writer.WriteLine("S " + msg + " got it")
        writer.Flush())
    }
}
server.ListenForClientsAsync(8080)
```
**C# Client-Side**
``` csharp
client.ConnectAsync("127.0.0.1", 8080).Wait()
client.ListenAsync();
```

**Sending messages:**

``` fsharp
// ...
// C# client.SendAsync("Hey, dude!")
client.SendAsync("Hey, dude!") |> ignore
```

You can find this samples in EasyProt/tests/EasyProt.ConsoleTest/Program.fs (Client) and  EasyProt/tests/EasyProt.TestServer/Program.fs (Server). Following you will see a screenshot of the including test client (left side) and test server (right side):

![alt tag](https://github.com/Jallah/EasyProt/blob/master/docs/files/img/ClientServer.jpg)

Some more instructions will follow soon ...

## How to build

Just run build.cmd for Windows or build.sh for Linux.

A NuGet-Package is planned.

## TODO
- Make it possible to register messages on the server side
- Make it possible to register a default handler for unknown messages
- IPipelineMemberInc: (Stream -> string -> string) so you can respond
- Make it possible to reregister a message

## ISSUES
- FAKE does not support NUNIT >=3 yet: https://github.com/fsharp/FAKE/issues/1010




  