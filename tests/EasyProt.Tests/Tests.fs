module EasyProt.Tests

open NUnit.Framework
open EasyProt.Runtime.Pipe
open EasyProt.Core

[<Test>]
let ``should return XXhelloXX``() = 
    let member1 = 
        { new IPipelineMember<string, string> with
              member this.Proceed input = input + "XX" }
    
    let member2 = 
        { new IPipelineMember<string, string> with
              member this.Proceed input = "XX" + input }
    
    let pipe = member1
                .Then(member2)

    let pipe = pipe.Proceed "hello"
    let result = pipe
    printfn "%s" result
    Assert.AreEqual(result, "XXhelloXX")

[<Test>]
let ``sould return 2X as string``() = 
    let member1 = 
        { new IPipelineMember<string, int> with
              member this.Proceed input = System.Int32.Parse(input) }
    
    let member2 = 
        { new IPipelineMember<int, int> with
              member this.Proceed input = input + 1 }
    
    let member3 = 
        { new IPipelineMember<int, string> with
              member this.Proceed input = input.ToString() + "X" }
    
    let pipe = member1
                .Then(member2)
                .Then(member3)

    let result = pipe.Proceed "1"
    printfn "%s" result
    Assert.AreEqual(result, "2X")


[<Test>]
let ``Test if the pipe runs correctly``() =

    let mutable value = "";

    let member1 = 
        { new IPipelineMember<string, int> with
              member this.Proceed input = System.Int32.Parse(input) }
    
    let member2 = 
        { new IPipelineMember<int, int> with
              member this.Proceed input = input + 1 }
    
    let member3 = 
        { new IPipelineMember<int, string> with
              member this.Proceed input = value <- (input.ToString() + "X")
                                          value }
    
    let pipe = member1
                .Then(member2)
                .Then(member3)
                .CreatePipe()

    pipe.Run "1" null
    printfn "%s" value
    Assert.AreEqual(value, "2X")