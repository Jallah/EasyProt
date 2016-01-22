module EasyProt.Tests

open NUnit.Framework
open EasyProt.Runtime
open EasyProt.Core


[<Test>]
let ``should return XXhelloXX`` () =
  let pipeline = new Pipeline()
  let member1 = {new IPipelineMember with
                    member this.Proceed input = input + "XX"}

  let member2 = {new IPipelineMember with
                    member this.Proceed input = "XX" + input}

  let pipeMembers = [member1; member2]

  let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hello"
  let result = pipe |> Async.RunSynchronously
  printfn "%s" result
  Assert.AreEqual(result , "XXhelloXX")
