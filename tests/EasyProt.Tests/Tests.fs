module EasyProt.Tests

open NUnit.Framework
open EasyProt.Runtime
open EasyProt.Core


[<Test>]
let ``should return XXhalloXX`` () = /// Fails Missing Methode Exception ?!
  let pipeline = new Pipeline()
  let member1 = {new IPipelineMember with
                    member this.ProceedAsync input = input + "XX"}

  let member2 = {new IPipelineMember with
                    member this.ProceedAsync input = "XX" + input}

  let pipeMembers = [member1; member2]

  let pipe = ((pipeline :> IPipeline).RunAsync pipeMembers) "hallo"
  let result = pipe |> Async.RunSynchronously
  printfn "%s" result
  Assert.AreEqual(result , "XXhalloXX")
