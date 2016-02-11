namespace EasyProt.Runtime
open Newtonsoft.Json
open EasyProt.Runtime.Helper

[<System.Runtime.CompilerServices.Extension>]
module Pipe = 
    
    

    let deserialize<'a> msg = 
          Newtonsoft.Json.JsonConvert.DeserializeObject<'a>(msg, serializerSettings)
    
    open EasyProt.Core
    
    // C# type extension
    [<System.Runtime.CompilerServices.Extension>]
    let Then(m : IPipelineMember<'a,'b>, t : IPipelineMember<'b, 'c>) = 
        { new IPipelineMember<'a, 'c> with
              member __.Proceed input = m.Proceed input |> t.Proceed }

    type IPipelineMember<'a,'b> with
        member this.Then(t) = Then(this, t)

    // C# type extension
    [<System.Runtime.CompilerServices.Extension>]
    let AddTypeCheck(m : IProtMessage<'a>) = 
        { new IProtMessage<string> with
              member __.Validate msgSerialized =
                  let test = msgSerialized;
                  let msgDeserialized = JsonConvert.DeserializeObject(msgSerialized, serializerSettings)
                  match msgDeserialized.GetType() = typeof<'a> with
                  | true -> m.Validate <| (msgDeserialized :?> 'a)
                  | _ -> false }

    type IProtMessage<'a> with
        member this.AddTypeCheck() = AddTypeCheck(this)

    // C# type extension
    [<System.Runtime.CompilerServices.Extension>]
    let CreatePipe(m : IPipelineMember<'a,'b>) =
        { new IPipeline with
              member __.Run input _ = 
                let msgDeserialized = JsonConvert.DeserializeObject<'a>(input, serializerSettings)
                m.Proceed msgDeserialized |> ignore}

    type IPipelineMember<'a,'b> with
        member this.CreatePipe() = CreatePipe(this)

    // C# type extension
    [<System.Runtime.CompilerServices.Extension>]
    let CreatePipeWithResponse(m : IPipelineMember<'a,'b>, r: IPipelineResponder<'b>) = 
        { new IPipeline with
              member __.Run input stream =
                input
                |> deserialize<'a>
                |> m.Proceed
                |> r.ResponseAsync (new System.IO.StreamWriter(stream))
                |> Async.Start}

    type IPipelineMember<'a,'b> with
        member this.CreatePipeWithResponse(m) = CreatePipeWithResponse(this, m)

    // C# type extension
    [<System.Runtime.CompilerServices.Extension>]
    let CreatePipeFromResponder(r: IPipelineResponder<'a>) =
        { new IPipeline with
              member __.Run input stream = 
                input
                |> deserialize<'a>
                |> r.ResponseAsync (new System.IO.StreamWriter(stream))
                |> Async.Start }

    type IPipelineResponder<'a> with
        member this.CreatePipeFromResponder() = CreatePipeFromResponder(this)