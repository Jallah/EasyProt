using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyProt.Core;
using EasyProt.Runtime;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace EasyProt.CSharpInterOpTest
{
    public class Responder : IPipelineResponder
    {
        public FSharpAsync<Unit> Response(string res, StreamWriter writer)
        {
            Helper.awaitTaskVoid(writer.WriteLineAsync("S " + res + " got it"));
            Helper.awaitTaskVoid(writer.FlushAsync());

            return null;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
