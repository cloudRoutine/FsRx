// ----------------------------------------------------------------------------
// (AsyncWorker) First version copied from the F# Power Pack --  https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/AsyncOperations.fs
// (c) Microsoft Corporation 2005-2009.
// (AsyncResult) First version copied from the F# Power Pack -- https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/AsyncWorker.fs
// (c) Microsoft Corporation 2005-2009. 
// | F# async extensions |
//  Original (c) Tomas Petricek, David Thomas 2012, Available under Apache 2.0 license.
//  Modified  by Jared Hester, 2014
// ----------------------------------------------------------------------------
namespace FSharp.Control
open System
open System.Threading

// ----------------------------------------------------------------------------

[<AutoOpen>]
module AsyncExtensions = 

    type Microsoft.FSharp.Control.Async with 

      /// Creates an asynchronous workflow that runs the asynchronous workflow
      /// given as an argument at most once. When the returned workflow is 
      /// started for the second time, it reuses the result of the 
      /// previous execution.
      static member Cache (input:Async<'T>) = 
          let agent = Agent<AsyncReplyChannel<_>>.Start(fun agent -> async {
              let! repl = agent.Receive()
              let! res = input
              repl.Reply(res)
              while true do
                  let! repl = agent.Receive()
                  repl.Reply(res) })

          async { return! agent.PostAndAsyncReply(id) }

      /// Starts the specified operation using a new CancellationToken and returns
      /// IDisposable object that cancels the computation. This method can be used
      /// when implementing the Subscribe method of IObservable interface.
      static member StartDisposable(op:Async<unit>) =
          let ct = new System.Threading.CancellationTokenSource()
          Async.Start(op, ct.Token)
          { new IDisposable with 
              member x.Dispose() = ct.Cancel() }



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
    

