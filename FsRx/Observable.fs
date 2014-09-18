// ----------------------------------------------------------------------------
// F# async extensions (Observable.fs)
// Original (c) Tomas Petricek, Phil Trelford, and Ryan Riley, 2011-2012, Available under Apache 2.0 license.
// Modified by  Jared Hester, 2014
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
        /// Creates an observable sequence from the provided subscribe function.
        static member Create (subscribe: IObserver<'T> -> unit -> unit) =
            Observable.Create( Func<_,_>(fun o -> Action(subscribe o)))

        [<Extension>]
        /// Creates an observable sequence from the provided subscribe function.
        static member Create subscribe =
            Observable.Create( Func<_,IDisposable> subscribe )




    let aggregate f seed source = Observable.Aggregate(source, seed, Func<_,_,_> f)
    //Observable.fo

    /// Determines whether all elements of and observable satisfy a predicate
    let all pred source =
        Observable.All(source, pred )



    /// Returns the observable sequence that reacts first
    let amb second first = Observable.Amb(first, second)


    /// Determines whether an observable sequence contains any elements
    let any  (source:IObservable<'Source>) : IObservable<bool> = 
        Observable.Any(source)




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





    /// Binds an observable to generate a subsequent observable.
    let bind (f: 'T -> IObservable<'TNext>) (m: IObservable<'T>) = m.SelectMany(Func<_,_> f)


    /// Lifts the values of f and m and applies f to m, returning an IObservable of the result.
    let apply f m = f |> bind (fun f' -> m |> bind (fun m' -> Observable.Return(f' m')))
 
 



    /// Matches when both observable sequences have an available value
    let both second first = Observable.And(first, second)   

    
    /// Converts the elements of the sequence to the specified type
    let cast<'CastType> (source) =
        Observable.Cast<'CastType>(source)


    /// Continues an observable sequence that is terminated
    /// by an exception with the next observable sequence.
    let catch (second: IObservable<'T>) first =
        Observable.Catch(first, second) 


    /// Produces an enumerable sequence of consequtive (possibly empty) chunks of the source observable
    let chunkify<'Source> source : IEnumerable<IList<'Source>> = 
        Observable.Chunkify<'Source>( source )


///--------------------------------------------------

//// TODO IMPLEMENT ALL 18 OF THE OVERLOADS
 ///  Observable.CombineLatest()

 ///---------------------------------------------

    /// Concats (flattens) an observable of observables into an observable
    /// ===> Observable.SelectMany(observable, Func<_,_>(fun (x:IObservable<'T>) -> x))
    let concat (second: IObservable<'T>) (first: IObservable<'T>) = Observable.Concat(first, second)


    /// Produces and enumerable sequence that returns elements collected/aggregated 
    /// from the source sequence between consecutive iterations 
    let collect  newCollector merge source = 
        Observable.Collect(source, newCollector, merge )


    /// Connects the observable wrapper to its source. All subscribed
    /// observers will recieve values from the underlying observable
    /// sequence as long as the connection is established.    
    /// ( publish an Observable to get a ConnectableObservable )
    let connect ( source:Subjects.IConnectableObservable<_> )=    
        source.Connect()


    /// Counts the elements
    let count source = Observable.Count(source)


    /// Creates an observable sequence from the specified Subscribe method implementation.
    let create (f: IObserver<'T> -> (unit -> unit)) = Observable.Create f

///-----------------------------------------------

// Implement 10 CREATE overloads

//////////////-------------------


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


    /// Returns an observable sequence that only contains distinct elements 
    let distinct source = 
        Observable.Distinct(source)


    /// Returns an observable sequence that only contains distinct contiguous elements 
    let distinctUntilChanged source = 
        Observable.DistinctUntilChanged(source)


    let dematerialize source = 
        Observable.Dematerialize(source)



    /// Generates an empty observable
    let empty<'T> = Observable.Empty<'T>()


    let error e =
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) =
                observer.OnError e
                { new IDisposable with member this.Dispose() = () }
        }


    /// Determines whether an observable sequence contains a specified value
    /// which satisfies the given predicate
    let exists predicate source = 
        Observable.Any(source, predicate)




    /// Filters the elements of an observable sequence based on a predicate
    let filter  (predicate:'T->bool) (source:IObservable<'T>) = 
        Observable.Where( source, predicate )


    /// Returns the first element of an observable sequence
    let firstAsync  = 
        Observable.FirstAsync




    /// Folds the observable
    let fold f seed source = Observable.Aggregate(source, seed, Func<_,_,_> f)


/// TODO FOR AND FOREACH ASYNC
    //    let forEachAsync source = 
//        Observable.ForEachAsync(source)



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
    let fromEventPattern<'T> eventName  (target:obj) =
        Observable.FromEventPattern( target, eventName )



    let generate initialstate condition iterator selector = 
        Observable.Generate( initialstate, condition, iterator, selector )


///---------TODO - IMPLEMENT THE 7 OVERLOADS
    let groupBy source keySelector = 
        Observable.GroupBy( source, keySelector )

//    let groupUntil source = 
//        Observable.GroupByUntil(source)


    /// Correlates the elements of two sequences based on overlapping 
    /// durations and groups the results
    let groupJoin   ( left:IObservable<'Left>) (right:IObservable<'Right> )  
                    ( leftselect  : 'Left -> IObservable<'a> ) 
                    ( rightselect : 'Right-> IObservable<'b> ) 
                    ( resultselect: 'Left -> IObservable<'Right>->'Result )  = 
        Observable.GroupJoin(   left, right, 
                                Func<'Left , IObservable<'a>>            leftselect  , 
                                Func<'Right, IObservable<'b>>            rightselect , 
                                Func<'Left , IObservable<'Right>,'Result>resultselect)


    /// Creates an observable that calls the specified function (each time)
    /// after an observer is attached to the observable. This is useful to 
    /// make sure that events triggered by the function are handled. 
    let guard f (source:IObservable<'Args>) =  
        {   
            new IObservable<'Args> with  
                member x.Subscribe( observer ) =  
                    let rm = source.Subscribe( observer ) in f() 
                    ( rm )
        } 


    /// Takes the head of the elements
    let head obs = Observable.FirstAsync(obs)


    /// Returns and observable sequence that produces a value after each period
    let interval period = 
        Observable.Interval( period )


    /// Determines whether the given observable is empty 
    let isEmpty source = source = Observable.Empty()


    /// Joins together the results from several patterns
    let joinWhen (plans:IEnumerable<Joins.Plan<'T>>): IObservable<'T>= 
        Observable.When( plans )


    let latest source = 
        Observable.Latest( source )


    /// Returns an observable sequence containing a int64 that represents 
    /// the total number of elements in an observable sequence 
    let longCount source = 
        Observable.LongCount(source)






    /// Maps the given observable with the given function
    let map f source = Observable.Select(source, Func<_,_>(f))   

    /// Maps the given observable with the given function and the 
    /// index of the element
    let mapi (f:int -> 'TSource -> 'TResult) (source:IObservable<'TSource>) =
        source 
        |> Observable.scan ( fun (i,_) x -> (i+1,Some(x))) (-1,None)
        |> Observable.map 
            (   function
                | i, Some(x) -> f i x
                | _, None    -> invalidOp "Invalid state"   )


    /// Maps two observables to the specified function.
    let map2 f a b = apply (apply f a) b


    /// TODO ADD MORE MERGES

    /// Merges the two observables
    let merge (second: IObservable<'T>) (first: IObservable<'T>) = Observable.Merge(first, second)






    /// Materializes the implicit notifications of an observable sequence as
    /// explicit notification values
    let materialize source = 
        Observable.Materialize( source )


    /// TODO IMPLEMENT 19 OVERLOADS of MAX
    let maxOf (source:IObservable<'T>) = 
        Observable.Max( source )



//    let maxBy source = 
//        Observable.MaxBy(source)
//
//    let minOf source = 
//        Observable.Min(source)
//
//    let minBy source = 
//        Observable.MinBy(source)
//
//    let using  source = 
//        Observable.Using(source)


    /// Returns an enumerable sequence whose sequence whose enumeration returns the 
    /// most recently observed element in the source observable sequence, using 
    /// the specified 
    let mostRecent initialVal source = 
        Observable.MostRecent( source, initialVal )









    /// Returns an observable sequence whose enumeration blocks until the next
    /// element in the source observable sequence becomes available. 
    /// Enumerators  on the resulting sequence will block until the next
    /// element becomes available.
    let next source = 
        Observable.Next( source ) 
 



        
 




    let onErrorResumeNext sources : IObservable<'Source> = 
        Observable.OnErrorResumeNext(sources)


    /// Returns the sequence as an observable
    let ofSeq<'Item>(items:'Item seq) : IObservable<'Item> =
        {   
            new IObservable<_> with
                member __.Subscribe( observer:IObserver<_> ) =
                    for item in items do observer.OnNext item      
                    observer.OnCompleted()     
                    {   new IDisposable with member __.Dispose() = ()   }
        }

    /// Filters the elements of an observable sequence based on the specified type
    let ofType source = 
        Observable.OfType( source )


    let pairwise (source:IObservable<'a>) : IObservable<'a*'a> = 
        Observable.pairwise( source )


    let partition predicate t = 
        Observable.partition( predicate t )



    /// Iterates through the observable and performs the given side-effect
    let perform f source =
        let inner x = f x
        Observable.Do(source, inner)
     

    /// Invokes the finally action after source observable sequence terminates normally or by an exception.
    let performFinally f source = Observable.Finally(source, Action f)




    /// Returns a connectable observable sequence (IConnectableObsevable) that shares
    /// a single subscription to the underlying sequence. This operator is a 
    /// specialization of Multicast using a regular Subject
    let publish source = 
        Observable.Publish( source )


    /// Returns a connectable observable sequence (IConnectableObsevable) that shares
    /// a single subscription to the underlying sequence and starts with the value
    /// initial. This operator is a specialization of Multicast using a regular Subject
    let publishInitial (initial:'Source) (source:IObservable<'Source>) = 
        Observable.Publish( source, initial )


    /// Returns an observable sequence that is the result of invoking 
    /// the selector on a connectable observable sequence that shares a
    /// a single subscription to the underlying sequence. This operator is a 
    /// specialization of Multicast using a regular Subject
    let publishMap ( map:IObservable<'Source> -> IObservable<'Result> ) 
                   ( source  :IObservable<'Source>            ) = 
        Observable.Publish( source, Func<IObservable<'Source>,IObservable<'Result>> map )


    /// Returns an observable sequence that is the result of 
    /// the map on a connectable observable sequence that shares a
    /// a single subscription to the underlying sequence. This operator is a 
    /// specialization of Multicast using a regular Subject
    let publishInitialMap ( initial : 'Source  )
                               ( map: IObservable<'Source> -> IObservable<'Result> ) 
                               ( source  : IObservable<'Source> ) = 
        Observable.Publish( source, Func<IObservable<'Source>,IObservable<'Result>> map, initial )


    /// Returns an observable sequence that is the result of invoking 
    /// the selector on a connectable observable sequence containing 
    /// only the last notification This operator is a 
    /// specialization of Multicast using a regular Subject
    let publishLast source = 
        Observable.PublishLast( source )


    /// Returns an observable sequence that is the result of invoking 
    /// the selector on a connectable observable sequence that shares a
    /// a single subscription to the underlying sequence. This operator is a 
    /// specialization of Multicast using a regular Subject
    let publishLastMap ( map: IObservable<'Source> -> IObservable<'Result> ) source  = 
        Observable.PublishLast( source , Func<IObservable<'Source>,IObservable<'Result>> map )



    /// Creates a range as an observable
    let range start count = Observable.Range(start, count)


    /// Reduces the observable
    let reduce f source = Observable.Aggregate(source, Func<_,_,_> f)

 
    /// Returns an observable that remains connected to the source as long
    /// as there is at least one subscription to the observable sequence 
    /// ( publish an Observable to get a ConnectableObservable )
    let refCount ( source )=
        Observable.RefCount ( source )   


    let result x : IObservable<_>=
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) =
                observer.OnNext x
                observer.OnCompleted()
                { new IDisposable with member this.Dispose() = () }
        }

        




        
    /// Samples the observable at the given interval
    let sample (interval: TimeSpan) source =
        Observable.Sample(source, interval)


    /// Applies an accumulator function over an observable sequence
    /// and returns each intermediate result. The specified seed value 
    /// is used as the initial accumulator value. For aggreagation behavior
    /// without intermediate results use 'aggregate'
    let scan (collector:'a->'b->'a) state source =
        Observable.Scan(source,  state, Func<'a,'b,'a>collector )




    let selectIf condition thenSource =
        Observable.If( Func<bool> condition, thenSource )



    let selectIfElse condition ( elseSource : IObservable<'Result>) 
                               ( thenSource : IObservable<'Result>) =
        Observable.If( Func<bool> condition, thenSource, elseSource )



    let selectIfScheduler condition ( scheduler  : Concurrency.IScheduler) 
                                    ( thenSource : IObservable<'Result>) =
        Observable.If( Func<bool> condition , thenSource, scheduler )

    /// Skips n elements
    let skip (n: int) source = Observable.Skip(source, n)
     

    /// Skips elements while the predicate is satisfied
    let skipWhile f source = Observable.SkipWhile(source, Func<_,_> f)
 



    let startWith source param = 
        Observable.StartWith( source, param )



    /// Subscribes to the Observable with a next fuction.
    let subscribe(onNext: 'T -> unit) (observable: IObservable<'T>) =
          observable.Subscribe(Action<_> onNext)


    /// Subscribes to the Observable with a next and an error-function.
    let subscribeWithError  ( onNext     : 'T   -> unit     ) 
                            ( onError    : exn  -> unit     ) 
                            ( observable : IObservable<'T>  ) =
        observable.Subscribe( Action<_> onNext, Action<exn> onError )
    
     
    /// Subscribes to the Observable with a next and a completion callback.
    let subscribeWithCompletion (onNext: 'T -> unit) (onCompleted: unit -> unit) (observable: IObservable<'T>) =
            observable.Subscribe(Action<_> onNext, Action onCompleted)
    



    /// Subscribes to the observable with all three callbacks
    let subscribeWithCallbacks onNext onError onCompleted (observable: IObservable<'T>) =
        observable.Subscribe(Observer.Create(Action<_> onNext, Action<_> onError, Action onCompleted))


    /// Subscribes to the observable with the given observer
    let subscribeObserver observer (observable: IObservable<'T>) =
        observable.Subscribe observer


    /// Transforms an observable sequence of observable sequences into an 
    /// observable sequence producing values only from the most recent 
    /// observable sequence.Each time a new inner observable sequnce is recieved,
    /// unsubscribe from the previous inner sequence
    let switch (sources:IObservable<IObservable<'Source>>) : IObservable<'Source>= 
        Observable.Switch(sources)


    /// Synchronizes the observable sequence so that notifications cannot be delivered concurrently
    /// this voerload is useful to "fix" and observable sequence that exhibits concurrent 
    /// callbacks on individual observers, which is invalid behavior for the query processor
    let synchronizeFix  source = 
        Observable.Synchronize( source )


    /// Takes n elements
    let take (n: int) source = Observable.Take(source, n)    


    /// Returns a specified number of contiguous elements from the end of an obserable sequence
    let takeLast (count:int) source = 
        Observable.TakeLast(source, count)



    /// Returns the elements from the source observable sequence until the other produces and element
    let takeUntil<'Other,'Source> other source =
        Observable.TakeUntil<'Source,'Other>(source , other )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTime<'Source> (endtime:DateTimeOffset) source =
        Observable.TakeUntil<'Source>(source , endtime )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTimer<'Source> (endtime:DateTimeOffset) scheduler source =
        Observable.TakeUntil<'Source>(source , endtime, scheduler )



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


    /// matches when the observable sequence has an available element and 
    /// applies the map
    let thenMap map source = 
        Observable.Then( source, Func<'Source,'Result> map )


    let takeLastBuffer (count:int) source = 
        Observable.TakeLastBuffer( source, count )  


    /// Converts a seq into an observable
    let toObservable (source: seq<'T>) = Observable.ToObservable(source)
    

    /// Converts an observable into a seq
    let toEnumerable (source: IObservable<'T>) = Observable.ToEnumerable(source)
    /// Creates an array from an observable sequence


    let toArray  source = 
        Observable.ToArray(source)


    /// Creates a list from an observable sequence
    let toList source = 
        Observable.ToList(source)


    /// Exposes and observable sequence as an object with an Action based .NET event
    let toEvent source = 
        Observable.ToEvent(source)

//    let toAsync source = 
//        Observable.ToAsync(source)


    /// Creates an observable sequence according to a specified key selector function
    let toDictionary keySelector source = 
        Observable.ToDictionary(source, keySelector)


    /// Creates an observable sequence according to a specified key selector function
    /// and an a comparer
    let toDictionaryComparer (keySelector:'Source->'Key) (comparer:'Key) (source:'Source) =
        Observable.ToDictionary( source, keySelector, comparer )
    

    /// Creates an observable sequence according to a specified key selector function
    let toDictionaryElements (keySelector:'Source->'Key )(elementSelector:'Source->'Elm) (source:'Source) =
        Observable.ToDictionary(source, keySelector, elementSelector)    


    /// Creates an observable sequence according to a specified key selector function
    let toDictionaryCompareElements ( keySelector    : 'Source -> 'Key  )
                                    ( elementSelector: 'Source ->' Elm  ) 
                                    ( comparer:'Key ) ( source:'Source  ) =
        Observable.ToDictionary(    source                              , 
                                    Func<'Source,'Key> keySelector      , 
                                    Func<'Source,'Elm> elementSelector  , 
                                    comparer                            ) 


    /// waits for the observable sequence to complete and returns the last
    /// element of the sequence. If the sequence terminates with OnError
    /// notification, the exception is thrown
    let wait  source = 
        Observable.Wait( source )


    /// Filters the observable elements of a sequence based on a predicate 
    let where  (predicate:'T->bool) (source:IObservable<'T>) = 
        Observable.Where( source, predicate )


    /// Filters the observable elements of a sequence based on a predicate by 
    /// incorporating the element's index
    let wherei (predicate:'T->int->bool) (source:IObservable<'T>)  = 
        Observable.Where( source, predicate )


/// TODO THE 6 OVERLOADS FOR WINDOW
//    Observable.Window

    /// Returns an observable that yields sliding windows of 
    /// containing elements drawn from the input observable. 
    /// Each window is returned as a fresh array.



    //// Add in the ZIP function with all the overloads


