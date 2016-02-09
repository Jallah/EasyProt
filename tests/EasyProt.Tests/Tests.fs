module EasyProt.Tests

open NUnit.Framework
open EasyProt.Runtime.Pipe
open EasyProt.Core


[<Test>]
let ``should return XXhelloXX`` () =
  let member1 = {new IPipelineMember<string, string> with
                    member this.Proceed input = input + "XX"}

  let member2 = {new IPipelineMember<string, string> with
                    member this.Proceed input = "XX" + input}

  let pipe = member1.Then(member2)

  let pipe = pipe.Proceed "hello"
  let result = pipe 
  printfn "%s" result
  Assert.AreEqual(result , "XXhelloXX")
