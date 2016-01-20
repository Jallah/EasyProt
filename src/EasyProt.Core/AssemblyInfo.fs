namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("EasyProt.Core")>]
[<assembly: AssemblyProductAttribute("EasyProt")>]
[<assembly: AssemblyDescriptionAttribute("EasyProt is an extensible runtime to easily implement your own Protocol")>]
[<assembly: AssemblyVersionAttribute("0.0.2")>]
[<assembly: AssemblyFileVersionAttribute("0.0.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.2"
