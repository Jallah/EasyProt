module EasyProt.Tests

open NUnit.Framework
open EasyProt.Runtime.Pipeline


[<Test>]
let ``simple test`` () =
  let result = 42
  printfn "simple test" 
  Assert.AreEqual(42,result)
