## Build status
**Master Branch**

[![Build Status](https://travis-ci.org/Jallah/EasyProt.svg?branch=master)](https://travis-ci.org/Jallah/EasyProt)
[![Build status](https://ci.appveyor.com/api/projects/status/cyi4mev19l12jyya?svg=true)](https://ci.appveyor.com/project/Jallah/easyprot)

## Description
EasyProt is a very lightwight Framwork for implementing your own Client-Server-Based Protocol. Usually you just have to define your messages by implementing the `IProtMessage ` interface. The communication between the Clients and the Sever runs asynchronus.

## Usage
There are much cool things you can do to create your own Protocol. EasyProt is written in F# but you can write your Implementation in every .NET Language even in VB .NET (don't take it too serious VB-Lovers :P). You can define a Pipeline (see below) for handling outgoing messages. You can even write your own implementation for the Client-Server-Communication by implementing the ``IProtClient`` and/or ``IProtServer`` interface. The default implementation uses a simple ``TcpClient`` with an unencrypted stream. But you can write your own implementation wich uses a ``SslStream``.

**IPipelineMember**

This interface just consits of one method:
``` csharp
    // C# string Proceed(string message)
    abstract member Proceed : string -> string
```
Let's look at two very simple implementations:
``` csharp
let member1 = 
    { new IPipelineMember with
          member this.Proceed input = input + "XX" }

let member2 = 
    { new IPipelineMember with
          member this.Proceed input = "XX" + input }
```
The result of this pipeline will be a string with leading and trailing **"XX"** (see screenshot below). Some real world example could be a member which logs the message somewhere. Or a member could act as an insult filter wich detects bad words and converts it into **$%+!?#&** or whatever.Some other implementation could convert from one format to another e.g. from XML to Json. It's a very flexible way to do some stuff with your outgoing messages with no limits being set to your imagination :).

**IProtMessage**
``` csharp
    // C# bool Validate(string message)
    abstract member Validate : message:string -> bool
    // C# Task HandleMessageAsync(string message)
    abstract member HandleMessageAsync : message:string -> Async<unit>
```

The ``Validate()`` method ist responsible to determine the message. Let's look at a very easy implementation:
``` csharp
   let msg1 = 
    { new IProtMessage with
          member this.Validate message = message.[0] = '1'
          member this.HandleMessageAsync message = async { System.Console.WriteLine("msg1: " + message) } }
```
So every time when the first sign of an incoming message is a **_1_** the Client or Server (depending on where you register your messages) knows wich ``HandleMessageAsync()`` implementation or Pipeline should be called. This member is the counterpart to for the Pipeline.

Some more instructions will follow soon ...

## TODO
- Comments in EasyProt.Runtime.RuntimeManager(.fs)
- Complete Readme

## ISSUES
- FAKE does not support NUNIT >=3: https://github.com/fsharp/FAKE/issues/1010




  