module Helper

open System.Threading.Tasks

let awaitTaskVoid (t: Task) = t |> Async.AwaitIAsyncResult |> Async.Ignore
