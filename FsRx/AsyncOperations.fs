// First version copied from the F# Power Pack 
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/AsyncOperations.fs

// (c) Microsoft Corporation 2005-2009. 
namespace Microsoft.FSharp.Control

    open System
    open System.Threading
    open Microsoft.FSharp.Control

    /// Represents the reified result of an asynchronous computation
    [<NoEquality; NoComparison>]
    type AsyncResult<'T>  =
        |   AsyncOk         of 'T
        |   AsyncException  of exn
        |   AsyncCanceled   of OperationCanceledException

        static member Commit( res:AsyncResult<'T> ) = 
            Async.FromContinuations 
                (   fun (cont,econt,ccont) -> 
                       match res with 
                       | AsyncOk        v   -> cont  v 
                       | AsyncException exn -> econt exn 
                       | AsyncCanceled  exn -> ccont exn 
                )


    [<AutoOpen>]
    module FileExtensions =

        let UnblockViaNewThread f =
            async { do! Async.SwitchToNewThread ()
                    let res = f()
                    do! Async.SwitchToThreadPool ()
                    return res }

        
        let private LinesToBytes ((lines:string array), encoder) =
            use memStrm = new System.IO.MemoryStream()
            use sWriter = new System.IO.StreamWriter(memStrm, encoder)
            lines |> Array.iter (fun line -> sWriter.WriteLine(line))
            do sWriter.Flush()
            memStrm.ToArray()


    [<AutoOpen>]
    module StreamReaderExtensions =
        type System.IO.StreamReader with

            member s.AsyncReadToEnd () = FileExtensions.UnblockViaNewThread (fun () -> s.ReadToEnd())
            member s.ReadToEndAsync () = s.AsyncReadToEnd ()


 