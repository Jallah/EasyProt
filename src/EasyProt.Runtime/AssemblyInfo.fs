namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("EasyProt.Runtime")>]
[<assembly: AssemblyProductAttribute("EasyProt")>]
[<assembly: AssemblyDescriptionAttribute("EasyProt is an extensible runtime to easily implement your own Protocol")>]
[<assembly: AssemblyVersionAttribute("0.0.3")>]
[<assembly: AssemblyFileVersionAttribute("0.0.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.3"
