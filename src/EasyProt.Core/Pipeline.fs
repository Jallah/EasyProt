module EasyProt.Core.Pipeline

///This interface ist just for convenience.
///Its a nice API for other .NET languages like C#.
type IPipelineMember =
    abstract member ProceedAsync : string -> string


type IPipeline =
   ///Runs the Protocol pipeline.
   abstract member RunAsync : IPipelineMember list -> (string -> Async<string>)

