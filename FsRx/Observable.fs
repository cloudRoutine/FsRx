// ----------------------------------------------------------------------------
// F# async extensions (Observable.fs)
// Original (c) Tomas Petricek, Phil Trelford, and Ryan Riley, 2011-2012, Available under Apache 2.0 license.
// Modified by Jared Hester 2014
// ----------------------------------------------------------------------------
#nowarn "40"
namespace FSharp.Control

open System
open System.Collections.Generic
open System.Threading
open Microsoft.FSharp.Core
open System.Reactive
open System.Reactive.Linq
open System.Runtime.CompilerServices


// ----------------------------------------------------------------------------

/// Union type that represents different messages that can be sent to the
/// IObserver interface. The IObserver type is equivalent to a type that has
/// just OnNext method that gets 'ObservableUpdate' as an argument.
type ObservableUpdate<'T> = 
    | Next      of 'T
    | Error     of exn
    | Completed

module Observable =

    type Observer with
        [<Extension>]
        /// Creates an observer from the specified onNext function.
        static member Create( onNext:'a -> unit ) : IObserver<'a> =
            Observer.Create( Action<_> onNext )

        [<Extension>]
        /// Creates an observer from the specified onNext and onError functions.
        static member Create( onNext, onError) =
            Observer.Create( Action<_> onNext, Action<_> onError )

        [<Extension>]
        /// Creates an observer from the specified onNext and onCompleted functions.
        static member Create( onNext, onCompleted ) =
            Observer.Create( Action<_> onNext, Action onCompleted )

        [<Extension>]
        /// Creates an observer from the specified onNext, onError, and onCompleted functions.
        static member Create( onNext, onError, onCompleted ) =
            Observer.Create( Action<_> onNext, Action<_> onError, Action onCompleted )




    type Observable with
        [<Extension>]
        /// Creates an observable sequence from the specified Subscribe method implementation.
        static member Create (subscribe: IObserver<'T> -> unit -> unit) =
            Observable.Create( Func<_,_>(fun o -> Action(subscribe o)))

        [<Extension>]
        /// Creates an observable sequence from the specified Subscribe method implementation.
        static member Create subscribe =
            Observable.Create( Func<_,IDisposable> subscribe )



        
//    //type IObservable<'T> with
//        [<Extension>]
//        /// Subscribes to the Observable with just a next-function.
//        member this.Subscribe(onNext: 'T -> unit) =
//            this.Subscribe( Action<_> onNext )
//
//        [<Extension>]
//        /// Subscribes to the Observable with a next and an error-function.
//        member this.Subscribe(onNext: 'T -> unit, onError: exn -> unit) =
//            this.Subscribe( Action<_> onNext, Action<exn> onError )
//     
//        [<Extension>]
//        /// Subscribes to the Observable with a next and a completion callback.
//        member this.Subscribe(onNext: 'T -> unit, onCompleted: unit -> unit) =
//            this.Subscribe( Action<_> onNext, Action onCompleted )
//
//        [<Extension>]
//        /// Subscribes to the Observable with all 3 callbacks.
//        member this.Subscribe(onNext, onError, onCompleted) =
//            this.Subscribe( Action<_> onNext, Action<_> onError, Action onCompleted )



    /// Turns observable into an observable that only calls OnNext method of the
    /// observer, but gives it a discriminated union that represents different
    /// kinds of events (error, next, completed)
    let asUpdates (input:IObservable<'T>) = 
        { 
            new IObservable<_> with
                member x.Subscribe(observer) =
                  input.Subscribe
                   ({ 
                        new IObserver<_> with
                            member x.OnNext(v)     = observer.OnNext( Next v    )
                            member x.OnCompleted() = observer.OnNext( Completed ) 
                            member x.OnError(e)    = observer.OnNext( Error e   ) 
                    }) 
        }


    /// Returns the observable sequence that reacts first
    let amb second first = Observable.Amb(first, second)



    /// Binds an observable to generate a subsequent observable.
    let bind (f: 'T -> IObservable<'TNext>) (m: IObservable<'T>) = m.SelectMany(Func<_,_> f)

    /// Lifts the values of f and m and applies f to m, returning an IObservable of the result.
    let apply f m = f |> bind (fun f' -> m |> bind (fun m' -> Observable.Return(f' m')))
 
 
    /// Matches when both observable sequences have an available value
    let both second first = Observable.And(first, second)   



    let combineLatest (left:IObservable<'TLeft>) (right:IObservable<'TRight>) =
        let left  = left  |> Observable.map( fun x -> Some x, None )
        let right = right |> Observable.map( fun x -> None, Some x )
        Observable.merge left right
        |> Observable.scan 
            ( fun ( _ ,(l',r')) ( l,r ) ->
                match l,r,l',r' with
                | Some lv, None,    _,       Some rv -> Some(lv,rv), (l,  r')
                | None,    Some rv, Some lv, _       -> Some(lv,rv), (l', r )
                | Some _,  _,       _,       None    -> None       , (l,  r')
                | _,       Some _,  None,    _       -> None       , (l', r )
                | None,    None,    _,       _       -> None       , (l', r')
                | Some _,  Some _,  _,       _ -> 
                      invalidOp "Should not receive both left and right"
            ) (None, (None,None))
        |> Observable.choose fst


    /// Concats (flattens) an observable of observables into an observable
    /// ===> Observable.SelectMany(observable, Func<_,_>(fun (x:IObservable<'T>) -> x))
    let concat (second: IObservable<'T>) (first: IObservable<'T>) = Observable.Concat(first, second)


    /// Counts the elements
    let count source = Observable.Count(source)


    /// Creates an observable sequence from the specified Subscribe method implementation.
    let create (f: IObserver<'T> -> (unit -> unit)) = Observable.Create f


//    /// Creates and observer for the provided onNext function
//    let create (onNext:IObserver<_> -> unit): IObservable<_> =
//        { new IObservable<_> with
//            member this.Subscribe(observer:IObserver<_>) =
//                let dispose = onNext observer
//                { new IDisposable with member this.Dispose() = dispose() }
//        }


    let createWithDisposable f =
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) = f observer
        }


    /// Generates an empty observable
    let empty<'T> = Observable.Empty<'T>()


    let error e =
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) =
                observer.OnError e
                { new IDisposable with member this.Dispose() = () }
        }


    let FromEvent<'EventArgs, 'TDelegate when 'EventArgs:> EventArgs>
        (   conversion      :Func<Action<'EventArgs>,'TDelegate>   ,
            addHandler      :Action<'TDelegate>                     ,
            removeHandler   :Action<'TDelegate>                     )  =
        { 
            new IObservable<'EventArgs> with
            member this.Subscribe(observer:IObserver<_>) =
                let handler = Action<_>(observer.OnNext) |> conversion.Invoke
                addHandler.Invoke handler
                let remove () = removeHandler.Invoke handler
                { new IDisposable with member this.Dispose() = remove () }
        }


    let fromEvent<'EventArgs, 'Delegate when 'EventArgs:> EventArgs>
            ( conversion   : ('EventArgs -> unit ) -> 'Delegate )
            ( addHandler   : ('Delegate  -> unit )              )
            ( removeHandler: ('Delegate  -> unit )              ) = 
        { 
          new IObservable<'EventArgs> with
            member this.Subscribe(observer:IObserver<_>) =
                let handler = observer.OnNext |> conversion
                addHandler handler
                let remove () = removeHandler handler
                { new IDisposable with member this.Dispose() = remove () }
        }



    let FromEventHandler<'EventArgs when 'EventArgs:> EventArgs>
        (addHandler:Action<EventHandler<_>>,
            removeHandler:Action<EventHandler<_>>)  =
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) =
                let handler = EventHandler<_>(fun _ x -> observer.OnNext x) 
                addHandler.Invoke handler
                let remove () = removeHandler.Invoke handler
                { new IDisposable with member this.Dispose() = remove () }
        }


    let fromEventHandler<'EventArgs when 'EventArgs:> EventArgs>
        ( addHandler    : EventHandler<_> -> unit )
        ( removeHandler : EventHandler<_> -> unit )  =
        {   
            new IObservable<_> with
                member this.Subscribe( observer:IObserver<_> ) =
                    let handler = EventHandler<_>( fun _ x -> observer.OnNext x ) 
                    addHandler handler
                    let remove () = removeHandler handler
                    {   new IDisposable with member this.Dispose() = remove ()  }
        }

    /// Generates an observable from an IEvent<_> as an EventPattern.
    let fromEventPattern<'T> (target:obj) eventName =
        Observable.FromEventPattern( target, eventName )


    /// Creates an observable that calls the specified function (each time)
    /// after an observer is attached to the observable. This is useful to 
    /// make sure that events triggered by the function are handled. 
    let guard f (e:IObservable<'Args>) =  
        {   
            new IObservable<'Args> with  
                member x.Subscribe( observer ) =  
                    let rm = e.Subscribe( observer ) in f() 
                    ( rm )
        } 


    /// Takes the head of the elements
    let head obs = Observable.FirstAsync(obs)



    /// Maps the given observable with the given function
    let map f source = Observable.Select(source, Func<_,_>(f))   


    let mapi (f:int -> 'TSource -> 'TResult) (source:IObservable<'TSource>) =
        source 
        |> Observable.scan ( fun (i,_) x -> (i+1,Some(x))) (-1,None)
        |> Observable.map 
            (   function
                | i, Some(x) -> f i x
                | _, None    -> invalidOp "Invalid state"   )


    /// Maps two observables to the specified function.
    let map2 f a b = apply (apply f a) b


    /// Merges the two observables
    let merge (second: IObservable<'T>) (first: IObservable<'T>) = Observable.Merge(first, second)


    let ofSeq<'TItem>(items:'TItem seq) =
        {   
            new IObservable<_> with
                member __.Subscribe( observer:IObserver<_> ) =
                    for item in items do observer.OnNext item      
                    observer.OnCompleted()     
                    {   new IDisposable with member __.Dispose() = ()   }
        }


        /// Iterates through the observable and performs the given side-effect
    let perform f source =
        let inner x = f x
        Observable.Do(source, inner)
     
    /// Invokes the finally action after source observable sequence terminates normally or by an exception.
    let performFinally f source = Observable.Finally(source, Action f)


    /// Creates a range as an observable
    let range start count = Observable.Range(start, count)


    /// Reduces the observable
    let reduce f source = Observable.Aggregate(source, Func<_,_,_> f)




    let result x : IObservable<_>=
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) =
                observer.OnNext x
                observer.OnCompleted()
                { new IDisposable with member this.Dispose() = () }
        }


    /// Skips n elements
    let skip (n: int) source = Observable.Skip(source, n)
     

    /// Skips elements while the predicate is satisfied
    let skipWhile f source = Observable.SkipWhile(source, Func<_,_> f)


    /// Subscribes to the Observable with a next fuction.
    let subscribe(onNext: 'T -> unit) (observable: IObservable<'T>) =
          observable.Subscribe(Action<_> onNext)


    /// Subscribes to the Observable with a next and an error-function.
    let subscribeWithError(onNext: 'T -> unit) (onError: exn -> unit) (observable: IObservable<'T>) =
          observable.Subscribe(Action<_> onNext, Action<exn> onError)
    
     
    /// Subscribes to the Observable with a next and a completion callback.
    let subscribeWithCompletion (onNext: 'T -> unit) (onCompleted: unit -> unit) (observable: IObservable<'T>) =
            observable.Subscribe(Action<_> onNext, Action onCompleted)
    



    /// Subscribes to the observable with all three callbacks
    let subscribeWithCallbacks onNext onError onCompleted (observable: IObservable<'T>) =
        observable.Subscribe(Observer.Create(Action<_> onNext, Action<_> onError, Action onCompleted))


    /// Subscribes to the observable with the given observer
    let subscribeObserver observer (observable: IObservable<'T>) =
        observable.Subscribe observer


    /// Takes n elements
    let take (n: int) source = Observable.Take(source, n)    


    let takeWhile f (source:IObservable<'TSource>) =
        {   new IObservable<_> with
                member __.Subscribe(observer:IObserver<_>) =
                    let take = ref true               
                    let d = source.Subscribe(fun item ->
                        if !take then
                            if f item then observer.OnNext item
                            else take := false; observer.OnCompleted()
                    )     
                    { new IDisposable with member __.Dispose() = d.Dispose() }
        }

    
    /// Converts a seq into an observable
    let toObservable (source: seq<'T>) = Observable.ToObservable(source)
    

    /// Converts an observable into a seq
    let toEnumerable (source: IObservable<'T>) = Observable.ToEnumerable(source)



        /// Returns an observable that yields sliding windows of 
    /// containing elements drawn from the input observable. 
    /// Each window is returned as a fresh array.
    let windowed size (input:IObservable<'T>) =
        { 
            new IObservable<'T[]> with
                member x.Subscribe(observer) =
                    // Create sliding window agent for every call
                    // and redirect batches to the observer
                    let cts     = new CancellationTokenSource()
                    let agent   = new SlidingWindowAgent<_>( size, cts.Token )
                    agent.WindowProduced.Add( observer.OnNext )

                    // Subscribe to the input and send values to the agent
                    let subscription = 
                      input.Subscribe
                        ({  
                            new IObserver<'T> with
                                member x.OnNext(v)      =   agent.Enqueue(v)
                                member x.OnCompleted()  =   cts.Cancel()
                                                            observer.OnCompleted()
                                member x.OnError(e)     =   cts.Cancel()
                                                            observer.OnError(e)   
                        })

                    // Cancel subscription & cancel the agent
                    {   new IDisposable with 
                            member x.Dispose() =
                                subscription.Dispose()
                                cts.Cancel()            } 
        }




    type internal LinkedList<'T> with
        member xs.pushBack(x:'T) = xs.AddLast(x) |> ignore
        member xs.popFront() = let x = xs.First.Value in xs.RemoveFirst(); x


    let zip (left:IObservable<'TLeft>) (right:IObservable<'TRight>) =
        let lefts, rights = LinkedList<'TLeft>(), LinkedList<'TRight>()
        let left  = left  |> Observable.map Choice1Of2
        let right = right |> Observable.map Choice2Of2
        Observable.merge left right
        |> Observable.choose (fun (c) ->
            match c with
            | Choice1Of2 l when rights.Count = 0 -> lefts.pushBack(l) 
                                                    None
            | Choice1Of2 l                       -> Some( l, rights.popFront())
            | Choice2Of2 r when lefts.Count = 0  -> rights.pushBack(r) 
                                                    None
            | Choice2Of2 r                       -> Some( lefts.popFront(), r )
        )


    let bufferWithTimeOrCount<'T> (timeSpan:TimeSpan) (count:int) (source:IObservable<'T>)=
        let timeSpan = int timeSpan.TotalMilliseconds
        let batch    = new BatchProcessingAgent<'T>(count, timeSpan) 
        { new IObservable<'T seq> with
            member this.Subscribe(observer:IObserver<'T seq>) =
                let sd = source.Subscribe(fun v -> batch.Enqueue v)
                let dd  = batch.BatchProduced.Subscribe(fun v -> observer.OnNext v)
                { new IDisposable with 
                    member this.Dispose() = 
                        sd.Dispose()
                        dd.Dispose()
                        (batch :> IDisposable).Dispose() 
                }
        }

    [<AbstractClass>]  
    type internal BasicObserver<'T>() =
        let mutable stopped = false
        abstract Next : value : 'T -> unit
        abstract Error : error : exn -> unit
        abstract Completed : unit -> unit
        interface IObserver<'T> with
            member x.OnNext value = 
                if not stopped then x.Next value
            member x.OnError e = 
                if not stopped then stopped <- true
                x.Error e
            member x.OnCompleted () = 
                if not stopped then stopped <- true
                x.Completed ()

    /// Invoke Observer function through specified function
    let invoke f (w:IObservable<_>):IObservable<'T> =
        let hook (observer:IObserver<_>) =
            { new BasicObserver<_>() with  
                member x.Next(v) = 
                    f (fun () -> observer.OnNext v)
                member x.Error(e) = 
                    f (fun () -> observer.OnError(e))
                member x.Completed() = 
                    f (fun () -> observer.OnCompleted()) 
            } 
        { new IObservable<_> with 
            member x.Subscribe(observer) =
                w.Subscribe (hook(observer))
        }
   
    /// Delay execution of Observer function
    let delay milliseconds (observable:IObservable<'T>): IObservable<'T> =
        let f g =
            async {
                do! Async.Sleep(milliseconds)
                do g ()
            } |> Async.Start
        invoke f observable

    /// Helper that can be used for writing CPS-style code that resumes
    /// on the same thread where the operation was started.
    let synchronize f = 
      let ctx = System.Threading.SynchronizationContext.Current 
      f (fun g ->
        let nctx = System.Threading.SynchronizationContext.Current 
        if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g()), null)
        else g() )


    type ObservableBuilder() =
        member this.Return(x) =
            Observable.Return x
        member this.ReturnFrom(m: IObservable<_>) = m
        member this.Bind(m: IObservable<'T>, f: 'T -> IObservable<'TNext>) =
            m.SelectMany(Func<_,_> f)
        member this.Combine(comp1: IObservable<'T>, comp2: IObservable<'T>) =
            Observable.Concat(comp1, comp2)
        member this.Delay(f) =
            Observable.Defer(f: Func<IObservable<'T>>)
        member this.Zero() =
            Observable.Empty()
        member this.TryWith(m: IObservable<_>, h: exn -> IObservable<_>) =
            Observable.Catch(m, h)
        member this.TryFinally(m: IObservable<_>, compensation: unit -> unit) =
            Observable.Finally(m, Action(compensation))
        member this.Using(res: #IDisposable, body) =
            this.TryFinally(body res, fun () -> match res with null -> () | disp -> disp.Dispose())
        member this.While(guard, m: IObservable<_>) =
            if not (guard()) then
                Observable.Empty()
            else
                m.SelectMany(Func<_,_>(fun () -> this.While(guard, m)))
        member this.For(sequence, body) =
            Observable.For(sequence, body)
        // TODO: Are these the correct implementation? Are they necessary?
        member this.Yield(x) =
            Observable.Return x
        member this.YieldFrom(m: IObservable<_>) = m

    let observe = ObservableBuilder()







    type Microsoft.FSharp.Control.Async with 

      /// Behaves like AwaitObservable, but calls the specified guarding function
      /// after a subscriber is registered with the observable.
      static member GuardedAwaitObservable (ev1:IObservable<'T1>) guardFunction =
          let removeObj : IDisposable option ref = ref None
          let removeLock = new obj()
          let setRemover r = 
              lock removeLock (fun () -> removeObj := Some r)
          let remove() =
              lock removeLock (fun () ->
                  match !removeObj with
                  | Some d -> removeObj := None
                              d.Dispose()
                  | None   -> ())
          synchronize (fun f ->
          let workflow =
              Async.FromContinuations((fun (cont,econt,ccont) ->
                  let rec finish cont value =
                      remove()
                      f (fun () -> cont value)
                  setRemover <|
                      ev1.Subscribe
                          ({ new IObserver<_> with
                              member x.OnNext(v) = finish cont v
                              member x.OnError(e) = finish econt e
                              member x.OnCompleted() =
                                  let msg = "Cancelling the workflow, because the Observable awaited using AwaitObservable has completed."
                                  finish ccont (new System.OperationCanceledException(msg)) })
                  guardFunction() ))
          async {
              let! cToken = Async.CancellationToken
              let token : CancellationToken = cToken
              #if NET40
              use registration = token.Register(fun () -> remove())
              #else
              use registration = token.Register((fun _ -> remove()), null)
              #endif
              return! workflow
          })

      /// Creates an asynchronous workflow that will be resumed when the 
      /// specified observables produces a value. The workflow will return 
      /// the value produced by the observable.
      static member AwaitObservable(observable : IObservable<'T1>) =
          let removeObj : IDisposable option ref = ref None
          let removeLock = new obj()
          let setRemover r = 
              lock removeLock (fun () -> removeObj := Some r)
          let remove() =
              lock removeLock (fun () ->
                  match !removeObj with
                  | Some d -> removeObj := None
                              d.Dispose()
                  | None   -> ())
          synchronize (fun f ->
          let workflow =
              Async.FromContinuations((fun (cont,econt,ccont) ->
                  let rec finish cont value =
                      remove()
                      f (fun () -> cont value)
                  setRemover <|
                      observable.Subscribe
                          ({ new IObserver<_> with
                              member x.OnNext(v) = finish cont v
                              member x.OnError(e) = finish econt e
                              member x.OnCompleted() =
                                  let msg = "Cancelling the workflow, because the Observable awaited using AwaitObservable has completed."
                                  finish ccont (new System.OperationCanceledException(msg)) })
                  () ))
          async {
              let! cToken = Async.CancellationToken
              let token : CancellationToken = cToken
              #if NET40
              use registration = token.Register(fun () -> remove())
              #else
              use registration = token.Register((fun _ -> remove()), null)
              #endif
              return! workflow
          })
  
      /// Creates an asynchronous workflow that will be resumed when the 
      /// first of the specified two observables produces a value. The 
      /// workflow will return a Choice value that can be used to identify
      /// the observable that produced the value.
      static member AwaitObservable(ev1:IObservable<'T1>, ev2:IObservable<'T2>) = 
        List.reduce Observable.merge 
          [ ev1 |> Observable.map Choice1Of2 
            ev2 |> Observable.map Choice2Of2 ] 
        |> Async.AwaitObservable

      /// Creates an asynchronous workflow that will be resumed when the 
      /// first of the specified three observables produces a value. The 
      /// workflow will return a Choice value that can be used to identify
      /// the observable that produced the value.
      static member AwaitObservable
          ( ev1:IObservable<'T1>, ev2:IObservable<'T2>, ev3:IObservable<'T3> ) = 
        List.reduce Observable.merge 
          [ ev1 |> Observable.map Choice1Of3 
            ev2 |> Observable.map Choice2Of3
            ev3 |> Observable.map Choice3Of3 ] 
        |> Async.AwaitObservable

      /// Creates an asynchronous workflow that will be resumed when the 
      /// first of the specified four observables produces a value. The 
      /// workflow will return a Choice value that can be used to identify
      /// the observable that produced the value.
      static member AwaitObservable( ev1:IObservable<'T1>, ev2:IObservable<'T2>, 
                                     ev3:IObservable<'T3>, ev4:IObservable<'T4> ) = 
        List.reduce Observable.merge 
          [ ev1 |> Observable.map Choice1Of4 
            ev2 |> Observable.map Choice2Of4
            ev3 |> Observable.map Choice3Of4
            ev4 |> Observable.map Choice4Of4 ] 
        |> Async.AwaitObservable

    let throttle (milliseconds:int) (source:IObservable<'T>)  =
        let relay (observer:IObserver<'T>) =
            let rec loop () = async {
                let! value = Async.AwaitObservable source
                observer.OnNext value
                do! Async.Sleep milliseconds
                return! loop() }
            loop ()
        { new IObservable<'T> with
            member this.Subscribe(observer:IObserver<'T>) =
                let cts = new System.Threading.CancellationTokenSource()
                Async.StartImmediate(relay observer, cts.Token)
                { new IDisposable with 
                    member this.Dispose() = cts.Cancel() 
                }
        }

// ----------------------------------------------------------------------------

open System.Runtime.CompilerServices

[<Extension>]
type ObservableExtensions private () =

    [<Extension>]
    static member ToObservable<'TItem>(source:IEnumerable<'TItem>) =
        source |> Observable.ofSeq

    [<Extension>]
    static member Subscribe<'TSource>(source:IObservable<'TSource>, action:Action<'TSource>) =
        source.Subscribe action.Invoke

    [<Extension>]
    static member Where<'TSource>(source:IObservable<'TSource>, predicate:Func<'TSource,bool>) =
            source |> Observable.filter predicate.Invoke

    [<Extension>]
    static member Select<'TSource,'TResult>(source:IObservable<'TSource>,selector:Func<'TSource,'TResult>) =
        source |> Observable.map selector.Invoke

    [<Extension>]
    static member Select<'TSource,'TResult>(source:IObservable<'TSource>,selector:Func<'TSource,int,'TResult>) =
        source |> Observable.mapi (fun i x -> selector.Invoke(x,i))

    [<Extension>]
    static member SelectMany<'TSource,'TCollection,'TResult>
            (   source              : IObservable<'TSource>                     ,
                collectionSelector  : Func<'TSource,IEnumerable<'TCollection>>  ,
                resultSelector      : Func<'TSource,'TCollection,'TResult>      ) =
        { new IObservable<'TResult> with
            member this.Subscribe(observer:IObserver<_>) =
                let disposable = source.Subscribe(fun s -> 
                    let cs = collectionSelector.Invoke s
                    for c in cs do
                        let r = resultSelector.Invoke(s,c)
                        observer.OnNext(r)
                )
                { new IDisposable with 
                    member this.Dispose() = disposable.Dispose() }
        }

    [<Extension>]
    static member TakeWhile<'TSource>(source:IObservable<'TSource>,f:Func<'TSource, bool>) =
        source |> Observable.takeWhile f.Invoke

    [<Extension>]
    static member Merge<'TSource>(source:IObservable<'TSource>, sources:IEnumerable<IObservable<'TSource>>) =

        let rec merge source = function
            | [] -> source
            | source'::sources' ->
                let result = Observable.merge source source'
                merge result sources'
        sources |> Seq.toList |> merge source

    [<Extension>]
    static member Merge<'TSource>(source:IObservable<'TSource>, [<ParamArray>] sources:IObservable<'TSource> []) =
        ObservableExtensions.Merge(source,sources |> Seq.ofArray)

    [<Extension>]
    static member Scan<'TSource,'TAccumulate>(source:IObservable<'TSource>, seed:'TAccumulate, f:Func<'TAccumulate,'TSource,'TAccumulate>) =
        source |> Observable.scan (fun acc x -> f.Invoke(acc,x)) seed

    [<Extension>]
    static member CombineLatest<'TLeft,'TRight,'TResult>(left:IObservable<'TLeft>, right:IObservable<'TRight>, selector:Func<'TLeft, 'TRight, 'TResult>) =
        Observable.combineLatest left right
        |> Observable.map selector.Invoke

    [<Extension>]
    static member Zip<'TLeft,'TRight,'TResult>(left:IObservable<'TLeft>,right:IObservable<'TRight>,selector:Func<'TLeft, 'TRight, 'TResult>) =
        Observable.zip left right
        |> Observable.map selector.Invoke

    [<Extension>]
    static member Delay<'TSource>(source:IObservable<'TSource>,milliseconds:int) =
        source |> Observable.delay milliseconds

    [<Extension>]
    static member BufferWithTimeOrCount<'TSource>(source:IObservable<'TSource>, timeSpan:TimeSpan, count:int) =
        source |> Observable.bufferWithTimeOrCount timeSpan count

    [<Extension>]
    static member Throttle<'TSource>(source:IObservable<'TSource>, dueTime:TimeSpan) =
        let dueTime = int dueTime.TotalMilliseconds
        source |> Observable.throttle dueTime

// ----------------------------------------------------------------------------

type private CircularBuffer<'T> (bufferSize:int) =
    let buffer = Array.zeroCreate<'T> bufferSize
    let mutable index = 0
    let mutable total = 0
    member this.Add value =
        if bufferSize > 0 then
            buffer.[index] <- value
            index <- (index + 1) % bufferSize
            total <- min (total + 1) bufferSize
    member this.Iter f =     
        let start = if total = bufferSize then index else 0
        for i = 0 to total - 1 do 
            buffer.[(start + i) % bufferSize] |> f



type private BufferAgentMessage<'T> =
    | Add       of IObserver<'T>
    | Remove    of IObserver<'T>
    | Next      of 'T
    | Completed
    | Error     of exn



module private BufferAgent =
    let start ( bufferSize:int ) =
        let subscribers = LinkedList<_>()
        let buffer      = CircularBuffer bufferSize
        Agent.Start( fun inbox ->
            let rec loop () = async {
                let! message = inbox.Receive()
                match message with
                | Add    observer   ->  subscribers.AddLast observer |> ignore
                                        buffer.Iter observer.OnNext
                                        return! loop ()            
                | Remove observer   ->  subscribers.Remove  observer |> ignore
                                        return! loop ()
                | Next   value      ->  for subscriber in subscribers do
                                            subscriber.OnNext value
                                            buffer.Add value
                                            return! loop ()
                | Error  e          ->  for subscriber in subscribers do
                                            subscriber.OnError e
                | Completed         ->  for subscriber in subscribers do
                                            subscriber.OnCompleted ()
            } loop ()
        )


[<Interface>]
type ISubject<'TIn,'TOut> =
    inherit System.IObserver<'TIn>
    inherit System.IObservable<'TOut>


type ReplaySubject<'T> (bufferSize:int) =
    let bufferSize  = max 0 bufferSize
    let agent       = BufferAgent.start bufferSize    
    let subscribe observer =
        observer |> Add |> agent.Post
        { new System.IDisposable with
            member this.Dispose () =
                observer |> Remove |> agent.Post
        }

    member this.OnNext value    = Next value    |> agent.Post
    member this.OnError error   = Error error   |> agent.Post
    member this.OnCompleted ()  = Completed     |> agent.Post    
    member this.Subscribe(observer:System.IObserver<'T>) = subscribe observer

    interface ISubject<'T,'T> with
        member this.OnNext      value    = Next value   |> agent.Post
        member this.OnError     error    = Error error  |> agent.Post
        member this.OnCompleted ()       = Completed    |> agent.Post
        member this.Subscribe   observer = subscribe observer

and Subject<'T>() = inherit ReplaySubject<'T>(0)
