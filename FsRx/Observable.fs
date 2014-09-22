//|--------------------------------------//
//|  Wrapped by Jared Hester, 2014      //
//|------------------------------------//

namespace FSharp.Control



open System
open System.Threading
open System.Reactive
open System.Reactive.Linq
open System.Collections.Generic
open System.Runtime.CompilerServices
open Microsoft.FSharp.Core


module Observable =


    /// Applies an accumulator function over an observable sequence, returning the 
    /// result of the aggregation as a single element in the result sequence
    let aggregate accumulator source = 
        Observable.Aggregate(source, Func<_,_,_> accumulator )


    /// Determines whether all elements of and observable satisfy a predicate
    let all pred source =
        Observable.All(source, pred )


    /// Returns the observable sequence that reacts first
    let amb second first = Observable.Amb(first, second)


    /// Propagates the observable sequence that reacts first
    let ambSeq (source:seq<IObservable<'T>>) = Observable.Amb( source )


    /// Propagates the observable sequence that reacts first
    let ambArray (source:IObservable<'T>[]) = Observable.Amb( source  )


    /// Determines whether an observable sequence contains any elements
    let any  (source:IObservable<'Source>) : IObservable<bool> = 
        Observable.Any(source)


    /// Hides the identy of an observable sequence 
    let asObservable source : IObservable<'Source>=
        Observable.AsObservable( source )


    /// Binds an observable to generate a subsequent observable.
    let bind (f: 'T -> IObservable<'TNext>) (m: IObservable<'T>) = m.SelectMany(Func<_,_> f)


    /// Lifts the values of f and m and applies f to m, returning an IObservable of the result.
    let apply f m = f |> bind (fun f' -> m |> bind (fun m' -> Observable.Return(f' m')))
 
 

    /// Matches when both observable sequences have an available value
    let both second first = Observable.And(first, second)   


    // #region Buffers


    let buffer (bufferClosingSelector:IObservable<'BufferClosing>) source = 
        Observable.Buffer(source, bufferClosingSelector)

    
    /// Projects each element of an observable sequence into 
    /// consequtive non-overlapping buffers based on a sequence of boundary markers
    let bufferBounded (boundaries:IObservable<'BufferClosing>) source : IObservable<IList<'T>>= 
        Observable.Buffer(source, boundaries)


    /// Projects each element of an observable sequence into 
    /// consequtive non-overlapping buffers produced based on count information
    let bufferCount (count:int) source = 
        Observable.Buffer(source, count)


    /// Projects each element of an observable sequence into zero or more buffers
    /// which are produced based on element count information
    let bufferCountSkip (count:int) (skip:int) source = 
        Observable.Buffer(source,count, skip)


    /// Projects each element of an observable sequence into 
    /// consequtive non-overlapping buffers produced based on timing information
    let bufferSpan (timeSpan:TimeSpan) source = 
        Observable.Buffer(source, timeSpan)


    /// Projects each element of an observable sequence into a buffer that goes
    /// sent out when either it's full or a specific amount of time has elapsed
    /// Analogy - A boat that departs when it's full or at its scheduled time to leave
    let bufferSpanCount (timeSpan:TimeSpan) (count:int) source = 
        Observable.Buffer(source, timeSpan, count)




    /// Projects each element of an observable sequence into zero of more buffers. 
    /// bufferOpenings - observable sequence whose elements denote the opening of each produced buffer
    /// bufferClosing - observable sequence whose elements denote the closing of each produced buffer
    let bufferFork  ( bufferOpenings:IObservable<'BufferOpening>) 
                    ( bufferClosingSelector: 'BufferOpening ->IObservable<'T> ) source = 
        Observable.Buffer( source, bufferOpenings,Func<_,_> bufferClosingSelector)


    /// Projects each element of an observable sequence into 
    /// zero or more buffers produced based on timing information
    let bufferSpanShift (timeSpan:TimeSpan) (timeShift:TimeSpan) source = 
        Observable.Buffer(source, timeSpan, timeShift)


    // #endregion



    
    /// Converts the elements of the sequence to the specified type
    let cast<'CastType> (source) =
        Observable.Cast<'CastType>(source)



    /// Uses selector to determine which source in sources to return,
    /// choosing an empty sequence if no match is found
    let case selector sources =
        Observable.Case( Func<_> selector, sources )


    /// Uses selector to determine which source in sources to return,
    /// choosing defaultSource if no match is found
    let caseDefault selector (defaultSource:IObservable<'Result>) (sources:IDictionary<'Value,IObservable<'Result>>) =
        Observable.Case( Func<'Value> selector, sources, defaultSource )


    /// Continues an observable sequence that is terminated
    /// by an exception with the next observable sequence.
    let catch (second: IObservable<'T>) first =
        Observable.Catch(first, second) 


    /// Continues an observable sequence that is terminated by an exception of
    /// the specified type with the observable sequence produced by the handler.
    let catchWith handler source =
        Observable.Catch( source,Func<_,_> handler )


    /// Continues an observable sequence that is terminated by an exception with the next observable sequence.
    let catchSeq (sources:seq<IObservable<'T>>) =
        Observable.Catch(sources)


    /// Continues an observable sequence that is terminated by an exception with the next observable sequence.
    let catchArray (sources:IObservable<'T>[]) =
        Observable.Catch(sources)


    /// Produces an enumerable sequence of consequtive (possibly empty) chunks of the source observable
    let chunkify<'Source> source : seq<IList<'Source>> = 
        Observable.Chunkify<'Source>( source )


    /// Produces an enumerable sequence of consecutive (possibly empty) chunks of the source sequence.
    let collect source newCollector merge = 
        Observable.Collect( source, Func<_> newCollector,Func<_,_,_> merge )


    /// Produces an enumerable sequence that returns elements 
    /// collected/aggregated from the source sequence between consecutive iterations.
    let collect' getInitialCollector merge getNewCollector source =
        Observable.Collect( source           , Func<_> getInitialCollector  , 
                            Func<_,_,_> merge, Func<_,_> getNewCollector    )


    // #region CombineLatest Functions

    /// Merges the specified observable sequences into one observable sequence by 
    /// emmiting a list with the latest source elements of whenever any of the 
    /// observable sequences produces and element.
    let combineLatest (source :seq<IObservable<'T>> ) : IObservable<IList<'T>> =
        Observable.CombineLatest( source )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestArray (source :IObservable<'T>[] )  =
        Observable.CombineLatest( source )        


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap ( map : IList<'T>-> 'Result  )(source :seq<IObservable<'T>> )  =
        Observable.CombineLatest( source, Func<IList<'T>,'Result> map )


    /// Concatenates the second observable sequence to the first observable sequence
    /// upn the successful termination of the first 
    let concat (second: IObservable<'T>) (first: IObservable<'T>) =
        Observable.Concat(first, second)
    

    /// Concatenates all observable sequences within the sequence as long as
    /// the previous observable sequence terminated successfully 
    let concatSeq (sources:seq<IObservable<'T>>) : IObservable<'T> =
        Observable.Concat(sources)


    /// Concatenates all of the specified  observable sequences as long as
    /// the previous observable sequence terminated successfully 
    let concatArray (sources:IObservable<'T>[]) =
        Observable.Concat(sources)


    /// Concatenates all of the inner observable sequences as long as
    /// the previous observable sequence terminated successfully 
    let concatInner (sources: IObservable<IObservable<'T>>) =
        Observable.Concat( sources )
    

    /// Concatenates all task results as long as
    /// the previous taskterminated successfully
    let concatTasks(sources: IObservable<Tasks.Task<'T>>) =
        Observable.Concat( sources )


    /// Connects the observable wrapper to its source. All subscribed
    /// observers will recieve values from the underlying observable
    /// sequence as long as the connection is established.    
    /// ( publish an Observable to get a ConnectableObservable )
    let connect ( source:Subjects.IConnectableObservable<_> ) =    
        source.Connect()


    /// Determines whether an observable sequence contains a specified 
    /// element by using the default equality comparer.
    let contains value source =
        Observable.Contains( source, value )


    /// Determines whether an observable sequence contains a 
    /// specified element by using a specified EqualityComparer
    let containsCompare comparer value source =
        Observable.Contains( source, value, comparer )


    /// Counts the elements
    let count source = 
        Observable.Count(source)


    /// Returns an observable sequence containing an int that represents how many elements 
    /// in the specified observable sequence satisfy a condition.
    let countSatisfy predicate source = 
        Observable.Count( source, predicate )

    ///  Creates an observable sequence from a specified Subscribe method implementation.
    let create subscribe =
        Observable.Create(Func<IObserver<'Result>,Action> subscribe)


    /// Creates an observable sequence from a specified Subscribe method implementation.
    let createWithDisposable subscribe =
        Observable.Create(Func<IObserver<'Result>,IDisposable> subscribe)


    /// Returns the elements of the specified sequence or the type parameter's default value 
    /// in a singleton sequence if the sequence is empty.
    let defaultIfEmpty    ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.DefaultIfEmpty( source )


    /// Returns the elements of the specified sequence or the specified value in a singleton sequence if the sequence is empty.
    let defaultIfEmptySeed (defaultValue:'TSource )( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.DefaultIfEmpty( source, defaultValue )
    

    /// Returns an observable sequence that invokes the specified factory function whenever a new observer subscribes.    
    let defer ( observableFactory: unit -> IObservable<'TResult> ): IObservable<'TResult> =
        Observable.Defer(Func<IObservable<'TResult>> observableFactory )


    /// Time shifts the observable sequence by the specified relative time duration.
    /// The relative time intervals between the values are preserved.
    let delay ( dueTime:TimeSpan ) ( source:IObservable<'TSource> ): IObservable<'TSource>=
        Observable.Delay(source, dueTime)


    /// Time shifts the observable sequence to start propagating notifications at the specified absolute time.
    /// The relative time intervals between the values are preserved.
    let delayUnitl  ( dueTime:DateTimeOffset ) ( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Delay(source, dueTime )
        

    /// Time shifts the observable sequence based on a delay selector function for each element.
    let delayMap ( delayDurationSelector:'TSource -> IObservable<'TDelay> )  ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Delay( source, Func<'TSource,IObservable<'TDelay>> delayDurationSelector)


    /// Time shifts the observable sequence based on a subscription delay and a delay selector function for each element.
    let delayMapFilter  ( delayDurationSelector:'TSource -> IObservable<'TDelay>)
                        ( subscriptionDelay:IObservable<'TDelay>)  
                        ( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Delay(source, subscriptionDelay, Func<'TSource, IObservable<'TDelay>> delayDurationSelector)


    /// Time shifts the observable sequence by delaying the subscription with the specified relative time duration.
    let delaySubscription ( dueTime:TimeSpan) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.DelaySubscription( source, dueTime )


    /// Time shifts the observable sequence by delaying the subscription to the specified absolute time.
    let delaySubscriptionUntil ( dueTime:DateTimeOffset) ( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.DelaySubscription( source, dueTime )


    /// Dematerializes the explicit notification values of an observable sequence as implicit notifications.
    let dematerialize source = 
        Observable.Dematerialize(source)


    /// Returns an observable sequence that only contains distinct elements 
    let distinct ( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Distinct( source )


    /// Returns an observable sequence that contains only distinct elements according to the keySelector.
    let distinctKey ( keySelector:Func<'TSource,'TKey> )( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Distinct( source, keySelector)


    /// Returns an observable sequence that contains only distinct elements according to the comparer.
    let distinctCompare ( comparer:IEqualityComparer<'TSource> )( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Distinct( source, comparer )


    /// Returns an observable sequence that contains only distinct elements according to the keySelector and the comparer.
    let distinctKeyCompare ( keySelector:Func<'TSource,'TKey> )( comparer:IEqualityComparer<'TKey>)( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Distinct( source, keySelector, comparer )
       

    /// Returns an observable sequence that only contains distinct contiguous elements 
    let distinctUntilChanged ( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.DistinctUntilChanged(source)


    /// Returns an observable sequence that contains only distinct contiguous elements according to the keySelector.
    let distinctUntilChangedKey ( keySelector:Func<'TSource,'TKey> )( source:IObservable<'TSource> )  : IObservable<'TSource> =
        Observable.DistinctUntilChanged( source, keySelector )
        

    /// Returns an observable sequence that contains only distinct contiguous elements according to the comparer.
    let distinctUntilChangedCompare ( comparer:IEqualityComparer<'Source> )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.DistinctUntilChanged( source, comparer )


    /// Returns an observable sequence that contains only distinct contiguous elements according to the keySelector and the comparer.
    let distinctUntilChangedKeyCompare  ( keySelector:Func<'TSource,'TKey> )( comparer:IEqualityComparer<'TKey> )( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.DistinctUntilChanged( source, keySelector, comparer )






    let elementAt  ( index:int ) ( source:IObservable<'TSource>): IObservable<'TSource> =


    let elementAtOrDefault ( index:int )( source:IObservable<'TSource> ) : IObservable<'TSource> =


    /// Returns an empty observable
    let empty<'T> = Observable.Empty<'T>()


    /// Returns an empty Observable sequence
    let emptyWitness<'T>(witness:'T) :IObservable<'T> =
             Observable.Empty( witness )


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


    /// Invokes a specified action after the source observable sequence
    /// terminates gracefully of exceptionally
    let finallyDo  finallyAction  source  =
        Observable.Finally( source, Action finallyAction ) 


    /// Returns the first element of an observable sequence
    let firstAsync (source:IObservable<'T>)  = 
        source.FirstAsync()


    /// Returns the first element of an observable sequence
    /// if it satisfies the predicate
    let firstAsyncIf predicate (source:IObservable<'T>) =
        source.FirstAsync( predicate )

//
//    static member First : source:IObservable<'TSource> -> 'TSource
//    static member First : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> 'TSource



//    static member FirstOrDefault : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> 'TSource
//    static member FirstOrDefault : source:IObservable<'TSource> -> 'TSource



    /// Applies an accumulator function over an observable sequence, returning the 
    /// result of the fold as a single element in the result sequence
    /// seed is the initial accumulator value
    let fold accumulator seed source =
        Observable.Aggregate(source, seed, Func<_,_,_> accumulator)


    /// Applies an accumulator function over an observable sequence, returning the 
    /// result of the fold as a single element in the result sequence
    /// Seed is the initial accumulator value, map is performed after the fold
    let foldMap accumulator seed map source =
        Observable.Aggregate(source, seed,Func<_,_,_> accumulator,Func<_,_>  map )


    let  forOb   ( resultSelector )( source:seq<'TSource> ) : IObservable<'TResult> =
        Observable.For( source, Func<'TSource, IObservable<'TResult>> resultSelector )

//
//    let  forEach ( source:IObservable<'TSource> )( onNext:Action<'TSource> ) : unit =
//        Observable.ForEachAsync( source, onNext )
//
//
//    let  forEach ( source:IObservable<'TSource> )( onNext:Action<'TSource,int>) : unit =
//







    let fromEvent ( addHandler )( removeHandler ) : IObservable<unit> =
        Observable.FromEvent( Action<'Delegate> addHandler, Action<'Delegate> removeHandler )


    let fromEventGeneric ( addHandler    : ('TEventArgs -> unit ) -> unit )
                         ( removeHandler : ('TEventArgs -> unit ) -> unit ) : IObservable<'TEventArgs> =
        Observable.FromEvent(Action<'TEventArgs->unit> addHandler, Action<'TEventArgs->unit> removeHandler )



// Couldn't figure out the wrap
//    let fromEventConversion   ( conversion: ('EventArgs->unit)->'Delegate ) ( addHandler ) ( removeHandler ) = 
//        Observable.FromEvent ( Func<Action<_>,_> conversion, Action<'Delegate> addHandler, Action<'Delegate> removeHandler )

    let fromEventConversion<'EventArgs, 'Delegate when 'EventArgs:> EventArgs>
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
//

    let generate initialstate condition iterator selector = 
        Observable.Generate( initialstate, condition, iterator, selector )


    let generate ( initialState:'TState )( condition:Func<'TState,bool> )( iterate:Func<'TState,'TState> )( resultSelector:Func<'TState,'TResult> : IObservable<'TResult> =
        Observable.Generate( initialstate, condition, iterator, selector )


    let generate ( initialState:'TState )( condition )( iterate )( resultSelector )( timeSelector ) : IObservable<'TResult> =
        Observable.Generate( initialstate, Func<'TState,bool> condition,Func<'TState,'TState> iterate, Func<'TState,'TResult>resultSelector, Func<'TState,TimeSpan>timeSelector )


    let generate    ( initialState:'TState  )
                    ( condition             )
                    ( iterate               )
                    ( resultSelector        )
                    ( timeSelector          ) : IObservable<'TResult> =
        Observable.Generate(                                initialstate    , 
                                Func<'TState,bool>          condition       , 
                                Func<'TState,'TState>       iterate         , 
                                Func<'TState,'TResult>      resultselector  ,
                                Func<'TState,DateTimeOffset>timeSelector    )


    let getEnumerator ( source:IObservable<'TSource> ) : seq<'TSource> =


    let groupBy keySelector source = 
        Observable.GroupBy( source, keySelector )


    let groupBy ( keySelector     : Func<'TSource,'TKey>     )
                ( elementSelector : Func<'TSource,'TElement> ) 
                ( capacity        : int                      )
                ( comparer        : IEqualityComparer<'TKey> ) 
                ( source          : IObservable<'TSource>    ) : IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupBy( source, keySelector, elementSelector, capacity, comparer )


    let groupBy ( keySelector     : Func<'TSource,'TKey>     )
                ( elementSelector : Func<'TSource,'TElement> ) 
                ( source          : IObservable<'TSource>    ) : IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupBy( source, keySelector, elementSelector )
        

    let groupBy ( keySelector : Func<'TSource,'TKey>     )
                ( comparer    : IEqualityComparer<'TKey> )
                ( source      : IObservable<'TSource>    ) : IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupBy( source, keySelector, elementSelector )



    let groupBy ( keySelector : Func<'TSource,'TKey>    )
                ( source      : IObservable<'TSource>   ) :  IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupBy( source, keySelector, elementSelector )


    let groupBy ( keySelector:Func<'TSource,'TKey> )( capacity:int )( source:IObservable<'TSource> ) : IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupBy( source, keySelector, elementSelector )


    let groupBy ( keySelector:Func<'TSource,'TKey> )( capacity:int )comparer:IEqualityComparer<'TKey> )( source:IObservable<'TSource> ): IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupBy( source, keySelector, elementSelector )


    let groupBy ( keySelector:Func<'TSource,'TKey> )( elementSelector:Func<'TSource,'TElement> )( comparer:IEqualityComparer<'TKey>) ( source:IObservable<'TSource> ): IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupBy( source, keySelector, elementSelector )


    let groupBy ( keySelector:Func<'TSource,'TKey> )( elementSelector:Func<'TSource,'TElement> )( capacity:int )( source:IObservable<'TSource> ): IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupBy( source, keySelector, elementSelector )


    let groupByUntil    ( keySelector     : Func<'TSource,'TKey> )
                        ( elementSelector : Func<'TSource,'TElement> )
                        ( durationSelector: Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>>)
                        ( capacity        : int )                   
                        ( source          : IObservable<'TSource> ) : IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )




    let groupByUntil    ( keySelector      )
                        ( durationSelector ) 
                        ( capacity : int   ) 
                        ( comparer : IEqualityComparer<'TKey> )
                        ( source   : IObservable<'TSource>    ) : IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupByUntil(    source, 
                                    Func<'TSource,'TKey> keySelector, 
                                    Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> durationSelector, 
                                    capacity, 
                                    comparer                    )


    let groupByUntil( keySelector     : Func<'TSource,'TKey> )
                    ( durationSelector: Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> )
                    ( capacity        : int )  
                    ( source          : IObservable<'TSource> ): IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )


    let groupByUntil( keySelector:Func<'TSource,'TKey> )
                    ( durationSelector:Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> )
                    ( comparer: IEqualityComparer<'TKey> ) 
                    ( source:IObservable<'TSource> ) : IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )


    let groupByUntil( keySelector:Func<'TSource,'TKey> )
                    ( elementSelector:Func<'TSource,'TElement> )
                    ( durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> )
                    ( source:IObservable<'TSource> ): IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )


    let groupByUntil( keySelector:Func<'TSource,'TKey> )
                    ( durationSelector:Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> )
                    ( source:IObservable<'TSource> ) : IObservable<IGroupedObservable<'TKey,'TSource>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )


    let groupByUntil( keySelector:Func<'TSource,'TKey> )
                    ( elementSelector:Func<'TSource,'TElement> )
                    ( durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> )
                    ( capacity:int )
                    ( comparer:IEqualityComparer<'TKey>)
                    ( source:IObservable<'TSource> ) : IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )


    let groupByUntil( keySelector:Func<'TSource,'TKey> )
                    ( elementSelector:Func<'TSource,'TElement> )
                    ( durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> )
                    ( comparer:Collections.Generic.IEqualityComparer<'TKey>) 
                    ( source:IObservable<'TSource> ) : IObservable<IGroupedObservable<'TKey,'TElement>> =
        Observable.GroupByUntil( source, keySelector, elementSelector )


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


    /// Invokes an action for each element in the observable sequence, and propagates all observer 
    /// messages through the result sequence. This method can be used for debugging, logging, etc. of query 
    /// behavior by intercepting the message stream to run arbitrary actions for messages on the pipeline.
    let iter ( onNext ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Do( source, Action<'TSource> onNext )
   

    /// Invokes an action for each element in the observable sequence and invokes an action 
    /// upon graceful termination of the observable sequence. This method can be used for debugging,
    ///  logging, etc. of query behavior by intercepting the message stream to run arbitrary
    /// actions for messages on the pipeline.
    let iterEnd ( onNext )( onCompleted ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Do( source, Action<'TSource> onNext, Action onCompleted )   


    /// Invokes an action for each element in the observable sequence and invokes an action upon 
    /// exceptional termination of the observable sequence. This method can be used for debugging, 
    /// logging, etc. of query behavior by intercepting the message stream to run arbitrary 
    /// actions for messages on the pipeline.
    let iterError ( onNext)( onError ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Do( source, Action<'TSource> onNext, Action<exn> onError )   
    

    /// Invokes an action for each element in the observable sequence and invokes an action 
    /// upon graceful or exceptional termination of the observable sequence.
    /// This method can be used for debugging, logging, etc. of query behavior by intercepting 
    /// the message stream to run arbitrary actions for messages on the pipeline.
    let iterErrorEnd ( onNext )( onError ) ( onCompleted ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Do( source, Action<'TSource> onNext, Action<exn> onError, Action onCompleted )   
        
    /// Invokes the observer's methods for each message in the source sequence.
    /// This method can be used for debugging, logging, etc. of query behavior by intercepting 
    /// the message stream to run arbitrary actions for messages on the pipeline.
    let iterObserver ( observer:IObserver<'TSource> ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Do( source,observer )   





    /// Joins together the results from several patterns
    let joinWhen (plans:seq<Joins.Plan<'T>>): IObservable<'T> = 
        Observable.When( plans )


//    let last  ( predicate ) ( source:IObservable<'TSource> ) : 'TSource =
//        Observable.LastAsync( source, Func<'TSource,bool> predicate )


//    let last ( source:IObservable<'TSource>) : 'TSource =
//        Observable.LastAsync( source )



//    let lastOrDefault  ( predicate:Func<'TSource,bool> )( source:IObservable<'TSource> ) : 'TSource =
//    let lastOrDefault ( source:IObservable<'TSource> ) : 'TSource =

////////////////////////////////////////////////


    let latest source = 
        Observable.Latest( source )


    /// Returns an observable sequence containing a int64 that represents 
    /// the total number of elements in an observable sequence 
    let longCount source = 
        Observable.LongCount(source)


    /// Returns an observable sequence containing an int that represents how many elements 
    /// in the specified observable sequence satisfy a condition.
    let longCountSatisfy predicate source = 
        Observable.LongCount(source, predicate)    


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


    /// Materializes the implicit notifications of an observable sequence as
    /// explicit notification values
    let materialize source = 
        Observable.Materialize( source )

    
    /// Merges the two observables
    let merge (second: IObservable<'T>) (first: IObservable<'T>) = Observable.Merge(first, second)


    /// Merges all the observable sequences into a single observable sequence.
    let mergeArray (sources:IObservable<'T>[]) =
        Observable.Merge(sources)


    /// Merges elements from all inner observable sequences 
    /// into a single  observable sequence.
    let mergeInner (sources:IObservable<IObservable<'T>>) =
        Observable.Merge(sources)


    /// Merges elements from all inner observable sequences 
    /// into a single  observable sequence limiting the number of concurrent 
    /// subscriptions to inner sequences
    let mergeInnerMax (maxConcurrent:int) (sources:IObservable<IObservable<'T>>) =
        Observable.Merge(sources, maxConcurrent)


    /// Merges an enumerable sequence of observable sequences into a single observable sequence.
    let mergeSeq (sources:seq<IObservable<'T>>) =
        Observable.Merge(sources)


    /// Merges an enumerable sequence of observable sequences into an observable sequence,
    ///  limiting the number of concurrent subscriptions to inner sequences.
    let mergeSeqMax (maxConcurrent:int)(sources:seq<IObservable<'T>>) =
        Observable.Merge(sources, maxConcurrent)


    /// Merge results from all source tasks into a single observable sequence
    let mergeTasks (sources:IObservable<Tasks.Task<'T>>) =
        Observable.Merge(sources)


    /// Returns the maximum element in an observable sequence.
    let maxOf (source:IObservable<'T>) = 
        Observable.Max( source )


    /// Returns an enumerable sequence whose sequence whose enumeration returns the 
    /// most recently observed element in the source observable sequence, using 
    /// the specified 
    let mostRecent initialVal source = 
        Observable.MostRecent( source, initialVal )


    /// Multicasts the source sequence notifications through the specified subject to 
    /// the resulting connectable observable. Upon connection of the connectable 
    /// observable, the subject is subscribed to the source exactly one, and messages
    /// are forwarded to the observers registered with the connectable observable. 
    /// For specializations with fixed subject types, see Publish, PublishLast, and Replay.
    let multicast subject source =
        Observable.Multicast(source, subject)


    /// Multicasts the source sequence notifications through an instantiated subject into
    /// all uses of the sequence within a selector function. Each subscription to the 
    /// resulting sequence causes a separate multicast invocation, exposing the sequence
    /// resulting from the selector function's invocation. For specializations with fixed
    /// subject types, see Publish, PublishLast, and Replay.
    let multicastMap subjectSelector selector source  =
        Observable.Multicast(source,subjectSelector, selector)


    /// Returns a non-terminating observable sequence, which can 
    /// be used to denote an infinite duration (e.g. when using reactive joins).
    let never() =
        Observable.Never()


    /// Returns a non-terminating observable sequence, which can be 
    /// used to denote an infinite duration (e.g. when using reactive joins).
    let neverWitness( witness ) =
        Observable.Never( witness )


    /// Returns an observable sequence whose enumeration blocks until the next
    /// element in the source observable sequence becomes available. 
    /// Enumerators  on the resulting sequence will block until the next
    /// element becomes available.
    let next source = 
        Observable.Next( source ) 
 

    /// Returns the sequence as an observable
    let ofSeq<'Item>(items:'Item seq) : IObservable<'Item> =
        {   
            new IObservable<_> with
                member __.Subscribe( observer:IObserver<_> ) =
                    for item in items do observer.OnNext item      
                    observer.OnCompleted()     
                    {   new IDisposable with member __.Dispose() = ()   }
        }


    /// Wraps the source sequence in order to run its observer callbacks on the specified scheduler.
    let observeOn (scheduler:Concurrency.IScheduler) source =
        Observable.ObserveOn( source, scheduler )


    /// Wraps the source sequence in order to run its observer callbacks 
    /// on the specified synchronization context.
    let observeOnContext (context:SynchronizationContext) source =
        Observable.ObserveOn( source, context )


    /// Filters the elements of an observable sequence based on the specified type
    let ofType source = 
        Observable.OfType( source )


    let onErrorResumeNext sources : IObservable<'Source> = 
        Observable.OnErrorResumeNext(sources)


    let onErrorResumeNext ( second:IObservable<'TSource> ) ( first:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.OnErrorResumeNext( first, second )


    let onErrorResumeNext ( sources:IObservable<'TSource> [] ) : IObservable<'TSource> =
        Observable.OnErrorResumeNext( sources )


    let onErrorResumeNext ( sources:seq<IObservable<'TSource>> ) : IObservable<'TSource> =
        Observable.OnErrorResumeNext( sources )
        

    let pairwise (source:IObservable<'a>) : IObservable<'a*'a> = 
        Observable.pairwise( source )


    let partition predicate (source:IObservable<'T>) = 
        Observable.partition( predicate source )



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
    let publishInitialMap  ( initial : 'Source  )
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


 
    let repeat ( value:'TResult )( repeatCount:int ) : IObservable<'TResult> =


    let repeat ( source:IObservable<'TSource> ) : IObservable<'TSource> =


    let repeat ( repeatCount:int ) ( source:IObservable<'TSource> ) : IObservable<'TSource> =


    let repeat ( value:'TResult ) : IObservable<'TResult> =


    let repeatWhile ( condition)( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.DoWhile( source, Func<bool> condition)


    let replay  ( selector:Func<IObservable<'TSource>,IObservable<'TResult>>)( bufferSize:int ) ( window:TimeSpan ) ( source:IObservable<'TSource>): IObservable<'TResult> =


    let replay  ( bufferSize:int )( window:TimeSpan )( source:IObservable<'TSource>) : Subjects.IConnectableObservable<'TSource> =


    let replay  ( selector:Func<IObservable<'TSource>,IObservable<'TResult>> )( window:TimeSpan )( source:IObservable<'TSource>) : IObservable<'TResult> =


    let replay  ( window:TimeSpan ) ( source:IObservable<'TSource>): Subjects.IConnectableObservable<'TSource> =


    let replay ( selector:Func<IObservable<'TSource>,IObservable<'TResult>> )( source:IObservable<'TSource>)  : IObservable<'TResult> =


    let replay ( source:IObservable<'TSource>) : Subjects.IConnectableObservable<'TSource> =                    
    
                                  
    let replay ( selector:Func<IObservable<'TSource>,IObservable<'TResult>> ) ( bufferSize:int )( source:IObservable<'TSource>) : IObservable<'TResult> =


    let replay ( bufferSize:int )( source:IObservable<'TSource>)  : Subjects.IConnectableObservable<'TSource> =





    let retry (retryCount:int) ( source:IObservable<'TSource>) : IObservable<'TSource> =


    let retry ( source:IObservable<'TSource>) : IObservable<'TSource> =



        
//
//
//    static member Return : value:'TResult -> IObservable<'TResult>


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

//        
//    static member Sample : source:IObservable<'TSource> * interval:TimeSpan -> IObservable<'TSource>
//    static member Sample : source:IObservable<'TSource> * sampler:IObservable<'TSample> -> IObservable<'TSource>



    /// Applies an accumulator function over an observable sequence
    /// and returns each intermediate result. The specified seed value 
    /// is used as the initial accumulator value. For aggreagation behavior
    /// without intermediate results use 'aggregate'
    let scan (collector:'a->'b->'a) state source =
        Observable.Scan(source,  state, Func<'a,'b,'a>collector )

//    static member Scan : source:IObservable<'TSource> * accumulator:Func<'TSource,'TSource,'TSource> -> IObservable<'TSource>
//    static member Scan : source:IObservable<'TSource> * seed:'TAccumulate * accumulator:Func<'TAccumulate,'TSource,'TAccumulate> -> IObservable<'TAccumulate>

    let selectMany selector source = 
        Observable.SelectMany(source,Func<'Source,int,seq<'Result>> selector )


    let selectMany1 selector source = 
        Observable.SelectMany(source,Func<'Sourec,int,IObservable<'Result>> selector )

    let selectMany2 selector source = 
        Observable.SelectMany(source,Func<'Source,int,CancellationToken,Tasks.Task<'Result>> selector)

    let selectMany3 selector source = 
        Observable.SelectMany(source,Func<'Source,int,Tasks.Task<'Result>> selector)


    let selectMany4 selector source = 
        Observable.SelectMany(source, Func<'Source,seq<'Result>> selector)


    let selectMany5 selector source = 
        Observable.SelectMany(source, Func<'S,IObservable<'R>> selector)

    let selectMany6 selector source = 
        Observable.SelectMany(source, Func<'S,CancellationToken,Tasks.Task<'R>> selector )
//
//
//    let selectMany    ( source:IObservable<'TSource> )( taskSelector:Func<'TSource,Threading.CancellationToken,Threading.Tasks.Task<'TTaskResult>> )( resultSelector:Func<'TSource,'TTaskResult,'TResult> ): IObservable<'TResult> =
//        Observable.SelectMany    ( source:IObservable<'TSource>, Func<'TSource,Threading.CancellationToken,Threading.Tasks.Task<'TTaskResult>> taskSelector, resultSelector:Func<'TSource,'TTaskResult,'TResult> ): IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( taskSelector:Func<'TSource,int,Threading.Tasks.Task<'TTaskResult>>)( resultSelector:Func<'TSource,int,'TTaskResult,'TResult> ) : IObservable<'TResult> =
//        Observable.SelectMany    ( source:IObservable<'TSource> )( taskSelector:Func<'TSource,int,Threading.Tasks.Task<'TTaskResult>>)( resultSelector:Func<'TSource,int,'TTaskResult,'TResult> ) : IObservable<'TResult> =
//

//    let selectMany    ( source:IObservable<'TSource> )( taskSelector:Func<'TSource,Threading.Tasks.Task<'TTaskResult>> * resultSelector:Func<'TSource,'TTaskResult,'TResult> ) IObservable<'TResult> =
//        Observable.SelectMany    ( source:IObservable<'TSource> )( taskSelector:Func<'TSource,Threading.Tasks.Task<'TTaskResult>> * resultSelector:Func<'TSource,'TTaskResult,'TResult> ) IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( collectionSelector:Func<'TSource,int,IObservable<'TCollection>> * resultSelector:Func<'TSource,int,'TCollection,int,'TResult> ) : IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( collectionSelector:Func<'TSource,int,IObservable<'TCollection>> * resultSelector:Func<'TSource,int,'TCollection,int,'TResult> ) : IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( onNext:Func<'TSource,int,IObservable<'TResult>> * onError:Func<exn,IObservable<'TResult>> * onCompleted:Func<IObservable<'TResult>> ) : IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( onNext:Func<'TSource,int,IObservable<'TResult>> * onError:Func<exn,IObservable<'TResult>> * onCompleted:Func<IObservable<'TResult>> ) : IObservable<'TResult> =


//    let selectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,Threading.CancellationToken,Threading.Tasks.Task<'TResult>>): IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( selector:Func<'TSource,Threading.CancellationToken,Threading.Tasks.Task<'TResult>>): IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,int,Threading.CancellationToken,Threading.Tasks.Task<'TResult>>): IObservable<'TResult> =
//        Observable.SelectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,int,Threading.CancellationToken,Threading.Tasks.Task<'TResult>>): IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( other:IObservable<'TOther> ): IObservable<'TOther> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( other:IObservable<'TOther> ): IObservable<'TOther> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,IObservable<'TResult>>): IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( selector:Func<'TSource,IObservable<'TResult>>): IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,int,IObservable<'TResult>>) : IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( selector:Func<'TSource,int,IObservable<'TResult>>) : IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,Threading.Tasks.Task<'TResult>> ) : IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( selector:Func<'TSource,Threading.Tasks.Task<'TResult>> ) : IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( selector:Func<'TSource,int,Threading.Tasks.Task<'TResult>>) : IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( selector:Func<'TSource,int,Threading.Tasks.Task<'TResult>>) : IObservable<'TResult> =
//
//
//    let selectMany    ( source:IObservable<'TSource> )( collectionSelector:Func<'TSource,IObservable<'TCollection>> )( resultSelector:Func<'TSource,'TCollection,'TResult>) : IObservable<'TResult> =
//        Observable.SelectMany     ( source:IObservable<'TSource> )( collectionSelector:Func<'TSource,IObservable<'TCollection>> )( resultSelector:Func<'TSource,'TCollection,'TResult>) : IObservable<'TResult> =


    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> ) : IObservable<bool> =
    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> ) : IObservable<bool> =


    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> )( comparer:IEqualityComparer<'TSource>) : IObservable<bool> =
    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> )( comparer:IEqualityComparer<'TSource>) : IObservable<bool> =

    let sequenceEqual ( first:IObservable<'TSource>  )( second:seq<'TSource>) : IObservable<bool> =
    let sequenceEqual ( first:IObservable<'TSource>  )( second:seq<'TSource>) : IObservable<bool> =


    let sequenceEqual ( first:IObservable<'TSource>  )( second:seq<'TSource> )( comparer:IEqualityComparer<'TSource> ) : IObservable<bool> =
    let sequenceEqual ( first:IObservable<'TSource>  )( second:seq<'TSource> )( comparer:IEqualityComparer<'TSource> ) : IObservable<bool> =



    let single               ( source:IObservable<'TSource>) (predicate:Func<'TSource,bool> ) : 'TSource =
    let single               ( source:IObservable<'TSource>) : 'TSource =

    let singleOrDefault      ( source:IObservable<'TSource>) : 'TSource =
    let singleOrDefault      ( source:IObservable<'TSource>) (predicate:Func<'TSource,bool>) : 'TSource =




    let selectIf condition thenSource =
        Observable.If( Func<bool> condition, thenSource )


    let selectIfElse condition ( elseSource : IObservable<'Result>) 
                               ( thenSource : IObservable<'Result>) =
        Observable.If( Func<bool> condition, thenSource, elseSource )


  
    let skip (count:int) (source:IObservable<'TSource>)  : IObservable<'TSource> =
        Observable.Skip(source , count)


    let skipSpan  (duration:TimeSpan ) (source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Skip(source, duration)


    let skipLast  (count:int ) ( source:IObservable<'TSource> ): IObservable<'TSource> = 
        Observable.SkipLast (source, count )


    let skipLastSpan (duration:TimeSpan ) ( source:IObservable<'TSource>) : IObservable<'TSource> =
        Observable.SkipLast ( source, duration)


    let skipUntil ( other:IObservable<'TOther> )  ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.SkipUntil(source, other )


    let skipUntilTime (startTime:DateTimeOffset ) ( source:IObservable<'TSource> )  : IObservable<'TSource> =
        Observable.SkipUntil(source, startTime )


    let skipWhile ( predicate:'TSource -> bool ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.SkipWhile ( source, Func<'TSource,bool> predicate ) 


    let skipWhilei ( predicate:'TSource -> int -> bool)( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.SkipWhile ( source, Func<'TSource,int,bool> predicate)


    let startWith  param  source = 
        Observable.StartWith( source, param )


      /// Prepends a sequence of values to an observable sequence.   
//    static member StartWith : source:IObservable<'TSource> * values:Collections.Generic.IEnumerable<'TSource> -> IObservable<'TSource>
//    static member StartWith : source:IObservable<'TSource> * values:Collections.Generic.IEnumerable<'TSource> -> IObservable<'TSource>



      /// Prepends a sequence of values to an observable sequence.   
//    static member StartWith : source:IObservable<'TSource> * values:'TSource [] -> IObservable<'TSource>
//    static member StartWith : source:IObservable<'TSource> * values:'TSource [] -> IObservable<'TSource>


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


    /// Wraps the source sequence in order to run its subscription and unsubscription logic 
    /// on the specified scheduler. This operation is not commonly used;  This only performs 
    /// the side-effects of subscription and unsubscription on the specified scheduler.
    ///  In order to invoke observer callbacks on a scheduler, use 'observeOn'
    let subscribeOn (context:Threading.SynchronizationContext) (source:IObservable<'TSource>) : IObservable<'TSource> =
        Observable.SubscribeOn( source, context )


    /// Transforms an observable sequence of observable sequences into an 
    /// observable sequence producing values only from the most recent 
    /// observable sequence.Each time a new inner observable sequnce is recieved,
    /// unsubscribe from the previous inner sequence
    let switch (sources:IObservable<IObservable<'Source>>) : IObservable<'Source>= 
        Observable.Switch(sources)

 //   static member Switch : sources:IObservable<Threading.Tasks.Task<'TSource>> -> IObservable<'TSource>



    /// Synchronizes the observable sequence so that notifications cannot be delivered concurrently
    /// this voerload is useful to "fix" and observable sequence that exhibits concurrent 
    /// callbacks on individual observers, which is invalid behavior for the query processor
    let synchronize  source : IObservable<'Source>= 
        Observable.Synchronize( source )


    /// Synchronizes the observable sequence such that observer notifications 
    /// cannot be delivered concurrently, using the specified gate object.This 
    /// overload is useful when writing n-ary query operators, in order to prevent 
    /// concurrent callbacks from different sources by synchronizing on a common gate object.
    let synchronizeGate (gate:obj)  (source:IObservable<'Source>): IObservable<'Source> =
        Observable.Synchronize( source, gate )



    /// Takes n elements
    let take (n: int) source = Observable.Take(source, n)    




    let take           ( count:int )( source:IObservable<'TSource> ): IObservable<'TSource> =



    let take           ( duration:TimeSpan )( source:IObservable<'TSource> ): IObservable<'TSource> =


    /// Returns a specified number of contiguous elements from the end of an obserable sequence
    let takeLast (count:int) source = 
        Observable.TakeLast(source, count)



    let takeLast      ( duration:TimeSpan ) ( source:IObservable<'TSource> ): IObservable<'TSource> =



    let takeLast      ( count:int ) ( source:IObservable<'TSource> ): IObservable<'TSource> =



    let takeLastBuffer ( duration:TimeSpan )( source:IObservable<'TSource> ): IObservable<Collections.Generic.IList<'TSource>> =



    let takeLastBuffer ( count:int )( source:IObservable<'TSource> ): IObservable<Collections.Generic.IList<'TSource>> =



    /// Returns the elements from the source observable sequence until the other produces and element
    let takeUntil<'Other,'Source> other source =
        Observable.TakeUntil<'Source,'Other>(source , other )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTime<'Source> (endtime:DateTimeOffset) source =
        Observable.TakeUntil<'Source>(source , endtime )


 
    let takeWhile : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
    let takeWhile : source:IObservable<'TSource> * predicate:Func<'TSource,int,bool> -> IObservable<'TSource>







    let throttle : source:IObservable<'TSource> * dueTime:TimeSpan -> IObservable<'TSource>
    let throttle : source:IObservable<'TSource> * throttleDurationSelector:Func<'TSource,IObservable<'TThrottle>> -> IObservable<'TSource>






    let throw ( exception:exn ) -> IObservable<'TResult>
    let throw ( exception:exn ) witness:'TResult -> IObservable<'TResult>





    /// matches when the observable sequence has an available element and 
    /// applies the map
    let thenMap map source = 
        Observable.Then( source, Func<'Source,'Result> map )


    let timeInterval ( source:IObservable<'TSource>) : IObservable<TimeInterval<'TSource>> =
        Observable.TimeInterval( source )


    let timeout ( durationSelector:Func<'TSource,IObservable<'TTimeout>>  )( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Timeout( source, )


    let timeout ( firstTimeout:IObservable<'TTimeout> )
                ( durationSelector:Func<'TSource,IObservable<'TTimeout>> )
                ( other:IObservable<'TSource>)
                ( source:IObservable<'TSource>  )                            : IObservable<'TSource> =
        Observable.Timeout( source, )


    let timeout ( dueTime:DateTimeOffset )( other:IObservable<'TSource> )( source:IObservable<'TSource> ) : IObservable<'TSource>=
        Observable.Timeout( source, )


    let timeout ( dueTime:DateTimeOffset ) ( source:IObservable<'TSource> ): IObservable<'TSource>=
        Observable.Timeout( source, )


    let timeout ( timeoutDurationSelector:Func<'TSource,IObservable<'TTimeout>> )( other:IObservable<'TSource> )( source:IObservable<'TSource> ) : IObservable<'TSource>=
        Observable.Timeout( source, )


    let timeout ( firstTimeout:IObservable<'TTimeout> )( timeoutDurationSelector:Func<'TSource,IObservable<'TTimeout>> )( source:IObservable<'TSource> ) : IObservable<'TSource> =
        Observable.Timeout( source, )


    let timeout ( dueTime:TimeSpan )  ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Timeout( source, )


    let timeout ( dueTime:TimeSpan ) ( other:IObservable<'TSource> ) ( source:IObservable<'TSource> ): IObservable<'TSource> =
        Observable.Timeout( source, )




    let timer ( dueTime:DateTimeOffset )( period:TimeSpan ) : IObservable<int64> = 
        Observable.Timer( dueTime, period )


    let timer ( dueTime:TimeSpan )( period:TimeSpan ) : IObservable<int64> = 
        Observable.Timer( dueTime, period )


    let timer ( dueTime:DateTimeOffset ) : IObservable<int64> = 
        Observable.Timer( dueTime )


    let timer ( dueTime:TimeSpan ) : IObservable<int64> =   
        Observable.Timer( dueTime )


    let takeLastBuffer (count:int) source = 
        Observable.TakeLastBuffer( source, count )  


    let timestamp ( source:IObservable<'TSource> ) : IObservable<Timestamped<'TSource>> =
        Observable.Timestamp( source )


    /// Converts an observable into a seq
    let toEnumerable (source: IObservable<'T>) = Observable.ToEnumerable(source)
    /// Creates an array from an observable sequence


    let toArray  source = 
        Observable.ToArray(source)


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
    

    /// Exposes and observable sequence as an object with an Action based .NET event
    let toEvent source = 
        Observable.ToEvent(source)
//
//    static member ToEvent : source:IObservable<'TSource> -> IEventSource<'TSource>
//    static member ToEvent : source:IObservable<Unit> -> IEventSource<Unit>
    
    /// Creates a list from an observable sequence
    let toList source = 
        Observable.ToList(source)



    let toLookup ( keySelector )( source:IObservable<'TSource> ) : IObservable<Linq.ILookup<'TKey,'TSource>> =
       Observable.ToLookup( source, Func<'TSource,'TKey> keySelector )


    let toLookup ( keySelector ) ( comparer:IEqualityComparer<'TKey> )( source:IObservable<'TSource> ) : IObservable<Linq.ILookup<'TKey,'TSource>> =
       Observable.ToLookup( source,Func<'TSource,'TKey> keySelector, comparer)


    let toLookup ( keySelector ) ( elementSelector )( source:IObservable<'TSource> ) : IObservable<Linq.ILookup<'TKey,'TElement>> =
       Observable.ToLookup( source,Func<'TSource,'TKey>  keySelector, Func<'TSource,'TElement> elementSelector )


    let toLookup ( keySelector ) ( elementSelector ) ( comparer:IEqualityComparer<'TKey>)( source:IObservable<'TSource> ) : IObservable<Linq.ILookup<'TKey,'TElement>>=
       Observable.ToLookup( source, Func<'TSource,'TKey> keySelector, Func<'TSource,'TElement> elementSelector, comparer )


    /// Converts a seq into an observable
    let toObservable ( source: seq<'T> ) = Observable.ToObservable(source)



    /// Constructs an observable sequence that depends on a resource object, whose 
    /// lifetime is tied to the resulting observable sequence's lifetime.
    let using ( resourceFactory: unit ->'TResource ) (observableFactory: 'TResource -> IObservable<'TResult> ) : IObservable<'TResult> =
        Observable.Using ( Func<_> resourceFactory, Func<_,_> observableFactory )


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


    /// Repeats the given function as long as the specified condition holds
    /// where the condition is evaluated before each repeated source is 
    /// subscribed to
    let whileLoop condition source = 
        Observable.While( Func<bool> condition, source ) 
        

    /// Projects each element of an observable sequence into consecutive non-overlapping windows.
    /// windowClosingSelector - A function invoked to define the boundaries of the produced windows. 
    /// A new window is started when the previous one is closed
    let window ( windowClosingSelector ) ( source:IObservable<'TSource> ) : IObservable<IObservable<'TSource>> =
        Observable.Window( source, Func<IObservable<'TWindowClosing>> windowClosingSelector)


    /// Projects each element of an observable sequence into consecutive non-overlapping windows 
    /// which are produced based on timing information.
    let windowTimeSpan ( timeSpan:TimeSpan )( source:IObservable<'TSource> ) : IObservable<IObservable<'TSource>> =
        Observable.Window( source, timeSpan )


    /// Projects each element of an observable sequence into zero or more windows.
    /// windowOpenings - Observable sequence whose elements denote the creation of new windows.
    /// windowClosingSelector - A function invoked to define the closing of each produced window.
    let windowOpenClose ( windowOpenings        : IObservable<'WinOpen>             )
                        ( windowClosingSelector : 'WinOpen->IObservable<'WinClose>  )
                        ( source                : IObservable<'Source>              ) : IObservable<IObservable<'Source>> =
        Observable.Window(source, windowOpenings, Func<_,_> windowClosingSelector)


    /// Projects each element of an observable sequence into consecutive non-overlapping windows.
    /// windowBoundaries - Sequence of window boundary markers. The current window is closed and a new window is opened upon receiving a boundary marker.
    let windowTimeShift ( timeSpan:TimeSpan )( timeShift:TimeSpan )( source:IObservable<'TSource> ) : IObservable<IObservable<'TSource>> =
        Observable.Window( source, timeSpan, timeShift )



    /// Projects each element of an observable sequence into consecutive non-overlapping windows
    /// windowBoundaries - Sequence of window boundary markers. The current window is closed 
    /// and a new window is opened upon receiving a boundary marker
    let windowBounded    ( windowBoundaries:IObservable<'TWindowBoundary> )( source:IObservable<'TSource> ) : IObservable<IObservable<'TSource>> =
        Observable.Window( source, windowBoundaries )


    /// Projects each element of an observable sequence into zero or more windows which are produced based on element count information
    let windowCountSkip ( count:int )( skip:int ) ( source:IObservable<'TSource> ): IObservable<IObservable<'TSource>> =
        Observable.Window( source, count, skip )


    /// Projects each element of an observable sequence into consecutive non-overlapping windows 
    /// which are produced based on element count information.
    let windowCount ( count:int )( source:IObservable<'TSource> ) : IObservable<IObservable<'TSource>> =
        Observable.Window( source, count )


    /// Projects each element of an observable sequence into a window that is completed when either it's full or 
    /// a given amount of time has elapsed.
    /// A useful real-world analogy of this overload is the behavior of a ferry leaving the dock when all seats are 
    /// taken, or at the scheduled time of departure, whichever event occurs first
    let windowTimeCount ( timeSpan:TimeSpan ) (count:int) ( source:IObservable<'TSource> ): IObservable<IObservable<'TSource>> =
        Observable.Window( source, timeSpan, count )


    /// Merges two observable sequences into one observable sequence by combining their elements in a pairwise fashion.
    let zip ( first:IObservable<'TSource1>)( second:IObservable<'TSource2>) ( resultSelector:'TSource1 -> 'TSource2 -> 'TResult) : IObservable<'TResult> =
        Observable.Zip( first, second, Func<'TSource1,'TSource2,'TResult> resultSelector)


    /// Merges the specified observable sequences into one observable sequence by emitting a
    ///  list with the elements of the observable sequences at corresponding indexes.
    let zipSeq ( sources:seq<IObservable<'TSource>>) : IObservable<IList<'TSource>> = 
        Observable.Zip( sources )


    /// Merges the specified observable sequences into one observable sequence by emitting 
    /// a list with the elements of the observable sequences at corresponding indexe
    let zipArray ( sources:IObservable<'TSource> []) : IObservable<IList<'TSource>> =
        Observable.Zip( sources )
 

    /// Merges the specified observable sequences into one observable sequence by using 
    /// the selector function whenever all of the observable sequences have produced an 
    /// element at a corresponding index.
    let zipSeqMap ( resultSelector: IList<'S> ->'R) ( sources: seq<IObservable<'S>>)  : IObservable<'R> =
        Observable.Zip( sources, Func<IList<'S>,'R> resultSelector)
 

 
    /// Merges an observable sequence and an enumerable sequence into one 
    /// observable sequence by using the selector function.
    let zipWithSeq ( resultSelector: 'TSource1 -> 'TSource2 -> 'TResult   )
                   ( second        : seq<'TSource2>                       )
                   ( first         : IObservable<'TSource1>               ) : IObservable<'TResult> =
        Observable.Zip(first, second, Func<_,_,_> resultSelector )
 



           
