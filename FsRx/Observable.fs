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


module Reactive =


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
    /// choosing defaulSource if no match is found
    let caseDefault selector (defaulSource:IObservable<'Result>) (sources:IDictionary<'Value,IObservable<'Result>>) =
        Observable.Case( Func<'Value> selector, sources, defaulSource )


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


    /// Concatenates the observable sequences obtained by applying the map for each element in the given enumerable 
    let collect   ( map )( source:seq<'Source> ) : IObservable<'Result> =
        Observable.For( source, Func<'Source, IObservable<'Result>> map )


  
    /// Produces an enumerable sequence that returns elements collected/aggregated from the source sequence between consecutive iterations.
    /// merge - Merges a sequence element with the current collector
    /// newCollector - Factory to create a new collector object.
    let collectMerge source newCollector merge = 
        Observable.Collect( source, Func<_> newCollector,Func<_,_,_> merge )


    /// Produces an enumerable sequence that returns elements collected/aggregated from the source sequence between consecutive iterations.
    /// merge - Merges a sequence element with the current collector
    /// getNewCollector - Factory to replace the current collector by a new collector
    /// getInitialCollector - Factory to create the initial collector object.
    let collectMergeInit getInitialCollector merge getNewCollector source =
        Observable.Collect( source           , Func<_> getInitialCollector  , 
                            Func<_,_,_> merge, Func<_,_> getNewCollector    )
//--------------------------------------------------------------------------------------------

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
    let defaultIfEmpty    ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.DefaultIfEmpty( source )


    /// Returns the elements of the specified sequence or the specified value in a singleton sequence if the sequence is empty.
    let defaultIfEmptyIs (defaultValue:'Source )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.DefaultIfEmpty( source, defaultValue )
    

    /// Returns an observable sequence that invokes the specified factory function whenever a new observer subscribes.    
    let defer ( observableFactory: unit -> IObservable<'Result> ): IObservable<'Result> =
        Observable.Defer(Func<IObservable<'Result>> observableFactory )


    /// Time shifts the observable sequence by the specified relative time duration.
    /// The relative time intervals between the values are preserved.
    let delay ( dueTime:TimeSpan ) ( source:IObservable<'Source> ): IObservable<'Source>=
        Observable.Delay(source, dueTime)


    /// Time shifts the observable sequence to start propagating notifications at the specified absolute time.
    /// The relative time intervals between the values are preserved.
    let delayUnitl  ( dueTime:DateTimeOffset ) ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Delay(source, dueTime )
        

    /// Time shifts the observable sequence based on a delay selector function for each element.
    let delayMap ( delayDurationSelector:'Source -> IObservable<'TDelay> )  ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Delay( source, Func<'Source,IObservable<'TDelay>> delayDurationSelector)


    /// Time shifts the observable sequence based on a subscription delay and a delay selector function for each element.
    let delayMapFilter  ( delayDurationSelector         : 'Source -> IObservable<'TDelay>)
                        ( subscriptionDelay             : IObservable<'TDelay>)  
                        ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Delay(source, subscriptionDelay, Func<'Source, IObservable<'TDelay>> delayDurationSelector)


    /// Time shifts the observable sequence by delaying the subscription with the specified relative time duration.
    let delaySubscription ( dueTime:TimeSpan) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.DelaySubscription( source, dueTime )


    /// Time shifts the observable sequence by delaying the subscription to the specified absolute time.
    let delaySubscriptionUntil ( dueTime:DateTimeOffset) ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.DelaySubscription( source, dueTime )


    /// Dematerializes the explicit notification values of an observable sequence as implicit notifications.
    let dematerialize source = 
        Observable.Dematerialize(source)


    /// Returns an observable sequence that only contains distinct elements 
    let distinct ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Distinct( source )


    /// Returns an observable sequence that contains only distinct elements according to the keySelector.
    let distincKey ( keySelector:Func<'Source,'Key> )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Distinct( source, keySelector)


    /// Returns an observable sequence that contains only distinct elements according to the comparer.
    let distinctCompare ( comparer:IEqualityComparer<'Source> )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Distinct( source, comparer )


    /// Returns an observable sequence that contains only distinct elements according to the keySelector and the comparer.
    let distincKeyCompare ( keySelector:Func<'Source,'Key> )( comparer:IEqualityComparer<'Key>)( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Distinct( source, keySelector, comparer )
       

    /// Returns an observable sequence that only contains distinct contiguous elements 
    let distinctUntilChanged ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.DistinctUntilChanged(source)


    /// Returns an observable sequence that contains only distinct contiguous elements according to the keySelector.
    let distinctUntilChangedKey ( keySelector:Func<'Source,'Key> )( source:IObservable<'Source> )  : IObservable<'Source> =
        Observable.DistinctUntilChanged( source, keySelector )
        

    /// Returns an observable sequence that contains only distinct contiguous elements according to the comparer.
    let distinctUntilChangedCompare ( comparer:IEqualityComparer<'Source> )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.DistinctUntilChanged( source, comparer )


    /// Returns an observable sequence that contains only distinct contiguous elements according to the keySelector and the comparer.
    let distinctUntilChangedKeyCompare  ( keySelector:Func<'Source,'Key> )( comparer:IEqualityComparer<'Key> )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.DistinctUntilChanged( source, keySelector, comparer )


    /// Returns the element at a specified index in a sequence.
    let elementAt  ( index:int ) ( source:IObservable<'Source>): IObservable<'Source> =
        Observable.ElementAt( source, index )


    /// Returns the element at a specified index in a sequence or a default value if the index is out of range
    let elementAtOrDefault ( index:int )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.ElementAt( source, index )


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


    /// Filters the observable elements of a sequence based on a predicate 
    let filter  (predicate:'T->bool) (source:IObservable<'T>) = 
        Observable.Where( source, predicate )


    /// Filters the observable elements of a sequence based on a predicate by 
    /// incorporating the element's index
    let filteri (predicate:'T->int->bool) (source:IObservable<'T>)  = 
        Observable.Where( source, predicate )


    /// Invokes a specified action after the source observable sequence
    /// terminates gracefully of exceptionally
    let finallyDo  finallyAction  source  =
        Observable.Finally( source, Action finallyAction ) 


    /// Returns the first element of an observable sequence
    let first (source:IObservable<'T>)  = 
        source.FirstAsync()


    /// Returns the first element of an observable sequence
    /// if it satisfies the predicate
    let firstIf predicate (source:IObservable<'T>) =
        source.FirstAsync( predicate )



    /// Applies an accumulator function over an observable sequence, returning the 
    /// result of the fold as a single element in the result sequence
    /// init is the initial accumulator value
    let fold accumulator init source =
        Observable.Aggregate(source, init, Func<_,_,_> accumulator)


    /// Applies an accumulator function over an observable sequence, returning the 
    /// result of the fold as a single element in the result sequence
    /// init is the initial accumulator value, map is performed after the fold
    let foldMap accumulator init map source =
        Observable.Aggregate(source, init,Func<_,_,_> accumulator,Func<_,_>  map )


    /// Converts an Action-based .NET event to an observable sequence. Each event invocation is surfaced through an OnNext message in the resulting sequence.
    /// For conversion of events conforming to the standard .NET event pattern, use any of the FromEventPattern overloads instead.
    let fromEvent ( addHandler )( removeHandler ) : IObservable<unit> =
        Observable.FromEvent( Action<'Delegate> addHandler, Action<'Delegate> removeHandler )


    /// Converts an generic Action-based .NET event to an observable sequence. Each event invocation is surfaced through an OnNext message in the resulting sequence.
    /// For conversion of events conforming to the standard .NET event pattern, use any of the FromEventPattern overloads instead.
    let fromEventGeneric ( addHandler    : ('TEventArgs -> unit ) -> unit )
                         ( removeHandler : ('TEventArgs -> unit ) -> unit ) : IObservable<'TEventArgs> =
        Observable.FromEvent(Action<'TEventArgs->unit> addHandler, Action<'TEventArgs->unit> removeHandler )



// Couldn't figure out the wrap
//    let fromEventConversion   ( conversion: ('EventArgs->unit)->'Delegate ) ( addHandler ) ( removeHandler ) = 
//        Observable.FromEvent ( Func<Action<_>,_> conversion, Action<'Delegate> addHandler, Action<'Delegate> removeHandler )


    /// Converts a .NET event to an observable sequence, using a conversion function to obtain the event delegate. 
    /// Each event invocation is surfaced through an OnNext message in the resulting sequence.
    /// For conversion of events conforming to the standard .NET event pattern, use any of the FromEventPattern functions instead.
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


    /// Converts a .NET event to an observable sequence, using a supplied event delegate type. 
    /// Each event invocation is surfaced through an OnNext message in the resulting sequence.
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



//    FromEventPattern : addHandler:Action<EventHandler> * removeHandler:Action<EventHandler> -> IObservable<EventPattern<EventArgs>>
//    FromEventPattern : type:Type * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<EventArgs>>
//    FromEventPattern : addHandler:Action<EventHandler> * removeHandler:Action<EventHandler> * scheduler:IScheduler -> IObservable<EventPattern<EventArgs>>
//    FromEventPattern : addHandler:Action<'Delegate> * removeHandler:Action<'Delegate> -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : target:obj * eventName:string -> IObservable<EventPattern<EventArgs>>
//    FromEventPattern : conversion:Func<EventHandler<'EventArgs>,'Delegate> * addHandler:Action<'Delegate> * removeHandler:Action<'Delegate> -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : type:Type * eventName:string -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : type:Type * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : type:Type * eventName:string -> IObservable<EventPattern<'TSender,'EventArgs>>
//    FromEventPattern : type:Type * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'TSender,'EventArgs>>
//    FromEventPattern : type:Type * eventName:string -> IObservable<EventPattern<EventArgs>>
//    FromEventPattern : target:obj * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'TSender,'EventArgs>>
//    FromEventPattern : target:obj * eventName:string -> IObservable<EventPattern<'TSender,'EventArgs>>
//    FromEventPattern : addHandler:Action<'Delegate> * removeHandler:Action<'Delegate> * scheduler:IScheduler -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : target:obj * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : target:obj * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<EventArgs>>
//    FromEventPattern : addHandler:Action<EventHandler<'EventArgs>> * removeHandler:Action<EventHandler<'EventArgs>> * scheduler:IScheduler -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : addHandler:Action<EventHandler<'EventArgs>> * removeHandler:Action<EventHandler<'EventArgs>> -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : addHandler:Action<'Delegate> * removeHandler:Action<'Delegate> * scheduler:IScheduler -> IObservable<EventPattern<'TSender,'EventArgs>>
//    FromEventPattern : addHandler:Action<'Delegate> * removeHandler:Action<'Delegate> -> IObservable<EventPattern<'TSender,'EventArgs>>
//    FromEventPattern : conversion:Func<EventHandler<'EventArgs>,'Delegate> * addHandler:Action<'Delegate> * removeHandler:Action<'Delegate> * scheduler:IScheduler -> IObservable<EventPattern<'EventArgs>>
//    FromEventPattern : target:obj * eventName:string -> IObservable<EventPattern<'EventArgs>>













    /// Generates an observable sequence by running a state-driven loop producing the sequence's elements.
    let generate initialState condition iterator resultMap = 
        Observable.Generate(                            initialState, 
                                Func<'State,bool>      condition   , 
                                Func<'State,'State>   iterator    , 
                                Func<'State,'Result>  resultMap   )


    /// Generates an observable sequence by running a state-driven and temporal loop producing the sequence's elements.
    let generateTimed( initialState:'State   )
                     ( condition             )
                     ( iterate               )
                     ( resultMap             )
                     ( timeSelector          ) : IObservable<'Result> =
        Observable.Generate(                                initialState   , 
                                Func<'State,bool>          condition       , 
                                Func<'State,'State>       iterate          , 
                                Func<'State,'Result>      resultMap        ,
                                Func<'State,DateTimeOffset>timeSelector    )

    /// Generates an observable sequence by running a state-driven and temporal loop producing the sequence's elements.
    let generateTimeSpan ( initialState:'State )( condition )( iterate )( resultMap )( genTime ) : IObservable<'Result> =
        Observable.Generate( initialState, condition, iterate, Func<'State,'Result>resultMap, Func<'State,TimeSpan>genTime )


    /// Returns an enumerator that enumerates all values of the observable sequence.
    let getEnumerator ( source ) : IEnumerator<_> =
        Observable.GetEnumerator( source )


    /// Groups the elements of an observable sequence according to a specified key selector function.
    let groupBy ( keySelector  )
                ( source      : IObservable<'Source>   ) :  IObservable<IGroupedObservable<'Key,'Source>> =
        Observable.GroupBy( source,Func<'Source,'Key>  keySelector )


    /// Groups the elements of an observable sequence with the specified initial 
    /// capacity according to a specified key selector function.
    let groupByCapacity ( keySelector )
                        ( capacity:int )
                        ( source:IObservable<'Source> ) : IObservable<IGroupedObservable<'Key,'Source>> =
        Observable.GroupBy( source, Func<'Source,'Key> keySelector, capacity)


    /// Groups the elements of an observable sequence according to a specified key selector function and comparer.
    let groupByCompare ( keySelector  )
                        ( comparer    : IEqualityComparer<_> )
                        ( source      : IObservable<_>    ) : IObservable<IGroupedObservable<_,_>> =
        Observable.GroupBy( source, Func<_,_> keySelector, keySelector )


    /// Groups the elements of an observable sequence and selects the resulting elements by using a specified function.
    let groupByElement  ( keySelector       )
                        ( elementSelector   ) 
                        ( source            ) : IObservable<IGroupedObservable<'Key,'Element>> =
        Observable.GroupBy( source, Func<'Source,'Key>  keySelector, Func<'Source,'Element> elementSelector )


    /// Groups the elements of an observable sequence with the specified initial capacity 
    /// according to a specified key selector function and comparer.
    let groupByCapacityCompare
                ( keySelector                           )
                ( capacity  : int                       )
                ( comparer  : IEqualityComparer<'Key>   )
                ( source    : IObservable<'Source>      )    : IObservable<IGroupedObservable<'Key,'Source>> =
        Observable.GroupBy( source, Func<'Source,'Key> keySelector, capacity, comparer )
    

    /// Groups the elements of an observable sequence with the specified initial capacity
    /// and selects the resulting elements by using a specified function.
    let groupByCapacityElement
                ( keySelector           )
                ( elementSelector       )
                ( capacity       : int  )
                ( source         : IObservable<'Source> ): IObservable<IGroupedObservable<'Key,'Element>> =
        Observable.GroupBy( source, Func<'Source,'Key> keySelector, Func<'Source,'Element>  elementSelector )


    /// Groups the elements of an observable sequence according to a specified key selector function 
    /// and comparer and selects the resulting elements by using a specified function.
    let groupByCompareElement
                ( keySelector      )
                ( elementSelector  )
                ( comparer       : IEqualityComparer<'Key>       )
                ( source         : IObservable<'Source>           ): IObservable<IGroupedObservable<'Key,'Element>> =
        Observable.GroupBy( source,Func<'Source,'Key>  keySelector, Func<'Source,'Element> elementSelector )



    /// Groups the elements of an observable sequence with the specified initial capacity according to a 
    /// specified key selector function and comparer and selects the resulting elements by using a specified function.
    let groupByCapacityCompareElement
                ( keySelector      )
                ( elementSelector  ) 
                ( capacity        : int                      )
                ( comparer        : IEqualityComparer<'Key> ) 
                ( source          : IObservable<'Source>    ) : IObservable<IGroupedObservable<'Key,'Element>> =
        Observable.GroupBy( source, Func<'Source,'Key>    keySelector, Func<'Source,'Element> elementSelector, capacity, comparer )



        








    let groupByUntil    ( keySelector     : Func<'Source,'Key> )
                        ( elementSelector : Func<'Source,'Element> )
                        ( durationSelector: Func<IGroupedObservable<'Key,'Element>,IObservable<'TDuration>>)
                        ( capacity        : int )                   
                        ( source          : IObservable<'Source> ) : IObservable<IGroupedObservable<'Key,'Element>> =
        Observable.GroupByUntil( source, keySelector, elementSelector, durationSelector, capacity )


//
//
//    let groupByUntil    ( keySelector      )
//                        ( durationSelector ) 
//                        ( capacity : int   ) 
//                        ( comparer : IEqualityComparer<'Key> )
//                        ( source   : IObservable<'Source>    ) : IObservable<IGroupedObservable<'Key,'Source>> =
//        Observable.GroupByUntil(    source, 
//                                    Func<'Source,'Key> keySelector, 
//                                    Func<IGroupedObservable<'Key,'Source>,IObservable<'TDuration>> durationSelector, 
//                                    capacity, 
//                                    comparer                    )
//
//
//    let groupByUntil( keySelector     : Func<'Source,'Key> )
//                    ( durationSelector: Func<IGroupedObservable<'Key,'Source>,IObservable<'TDuration>> )
//                    ( capacity        : int )  
//                    ( source          : IObservable<'Source> ): IObservable<IGroupedObservable<'Key,'Source>> =
//        Observable.GroupByUntil( source, keySelector, durationSelector, capacity)
//
//
//    let groupByUntil( keySelector:Func<'Source,'Key> )
//                    ( durationSelector:Func<IGroupedObservable<'Key,'Source>,IObservable<'TDuration>> )
//                    ( comparer: IEqualityComparer<'Key> ) 
//                    ( source:IObservable<'Source> ) : IObservable<IGroupedObservable<'Key,'Source>> =
//        Observable.GroupByUntil( source, keySelector, durationSelector, comparer )
//
//
//    let groupByUntil( keySelector:Func<'Source,'Key> )
//                    ( elementSelector:Func<'Source,'Element> )
//                    ( durationSelector:Func<IGroupedObservable<'Key,'Element>,IObservable<'TDuration>> )
//                    ( source:IObservable<'Source> ): IObservable<IGroupedObservable<'Key,'Element>> =
//        Observable.GroupByUntil( source, elementSelector, durationSelector )
//
//
//    let groupByUntil( keySelector:Func<'Source,'Key> )
//                    ( durationSelector:Func<IGroupedObservable<'Key,'Source>,IObservable<'TDuration>> )
//                    ( source:IObservable<'Source> ) : IObservable<IGroupedObservable<'Key,'Source>> =
//        Observable.GroupByUntil( source, keySelector, durationSelector )
//
//
//    let groupByUntil( keySelector:Func<'Source,'Key> )
//                    ( elementSelector:Func<'Source,'Element> )
//                    ( durationSelector:Func<IGroupedObservable<'Key,'Element>,IObservable<'TDuration>> )
//                    ( capacity:int )
//                    ( comparer:IEqualityComparer<'Key>)
//                    ( source:IObservable<'Source> ) : IObservable<IGroupedObservable<'Key,'Element>> =
//        Observable.GroupByUntil( source, keySelector, elementSelector, durationSelector, capacity, comparer )
//
//
//    let groupByUntil( keySelector:Func<'Source,'Key> )
//                    ( elementSelector:Func<'Source,'Element> )
//                    ( durationSelector:Func<IGroupedObservable<'Key,'Element>,IObservable<'TDuration>> )
//                    ( comparer:Collections.Generic.IEqualityComparer<'Key>) 
//                    ( source:IObservable<'Source> ) : IObservable<IGroupedObservable<'Key,'Element>> =
//        Observable.GroupByUntil( source, keySelector, elementSelector, durationSelector, comparer )


    /// Correlates the elements of two sequences based on overlapping 
    /// durations and groups the results
    let groupJoin   ( left        : IObservable<'Left>       ) 
                    ( right       : IObservable<'Right>      )  
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


    /// Takes the first element of the observable sequence
    let head obs = Observable.FirstAsync(obs)


    /// Returns and observable sequence that produces a value after each period
    let interval period = 
        Observable.Interval( period )


    /// Determines whether the given observable is empty 
    let isEmpty source = source = Observable.Empty()


    /// Invokes an action for each element in the observable sequence, and propagates all observer 
    /// messages through the result sequence. This method can be used for debugging, logging, etc. of query 
    /// behavior by intercepting the message stream to run arbitrary actions for messages on the pipeline.
    let iter ( onNext ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Do( source, Action<'Source> onNext )
   

    /// Invokes an action for each element in the observable sequence and invokes an action 
    /// upon graceful termination of the observable sequence. This method can be used for debugging,
    ///  logging, etc. of query behavior by intercepting the message stream to run arbitrary
    /// actions for messages on the pipeline.
    let iterEnd ( onNext )( onCompleted ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Do( source, Action<'Source> onNext, Action onCompleted )   


    /// Invokes an action for each element in the observable sequence and invokes an action upon 
    /// exceptional termination of the observable sequence. This method can be used for debugging, 
    /// logging, etc. of query behavior by intercepting the message stream to run arbitrary 
    /// actions for messages on the pipeline.
    let iterError ( onNext)( onError ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Do( source, Action<'Source> onNext, Action<exn> onError )   
    

    /// Invokes an action for each element in the observable sequence and invokes an action 
    /// upon graceful or exceptional termination of the observable sequence.
    /// This method can be used for debugging, logging, etc. of query behavior by intercepting 
    /// the message stream to run arbitrary actions for messages on the pipeline.
    let iterErrorEnd ( onNext )( onError ) ( onCompleted ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Do( source, Action<'Source> onNext, Action<exn> onError, Action onCompleted )   
        
    /// Invokes the observer's methods for each message in the source sequence.
    /// This method can be used for debugging, logging, etc. of query behavior by intercepting 
    /// the message stream to run arbitrary actions for messages on the pipeline.
    let iterObserver ( observer:IObserver<'Source> ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Do( source,observer )   





    /// Joins together the results from several patterns
    let joinWhen (plans:seq<Joins.Plan<'T>>): IObservable<'T> = 
        Observable.When( plans )





    /// Returns the last element of an observable sequence.
    let last ( source:IObservable<'Source>) : IObservable<'Source> =
        Observable.LastAsync( source )


    /// Returns the last element of an observable sequence that satisfies the condition in the predicate 
    let lastIf  ( predicate ) ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.LastAsync( source, Func<'Source,bool> predicate )



//    let lastOrDefault  ( predicate:Func<'Source,bool> )( source:IObservable<'Source> ) : 'Source =
//    let lastOrDefault ( source:IObservable<'Source> ) : 'Source =

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
    let mapi (f:int -> 'Source -> 'Result) (source:IObservable<'Source>) =
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


//    let onErrorResumeNext ( second:IObservable<'Source> ) ( first:IObservable<'Source> ) : IObservable<'Source> =
//        Observable.OnErrorResumeNext( first, second )
//
//
//    let onErrorResumeNext ( sources:IObservable<'Source> [] ) : IObservable<'Source> =
//        Observable.OnErrorResumeNext( sources )
//
//
//    let onErrorResumeNext ( sources:seq<IObservable<'Source>> ) : IObservable<'Source> =
//        Observable.OnErrorResumeNext( sources )
        

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


    /// Repeats the observable sequence a specified number of times.
    let repeatCount<'Result> ( value:'Result )( repeatCount:int ) : IObservable<'Result> =
        Observable.Repeat( value, repeatCount )


    /// Repeats the observable sequence indefinitely.
    let repeat<'Source> ( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Repeat<'Source>( source )

//    let repeat ( repeatCount:int ) ( source:IObservable<'Source> ) : IObservable<'Source> =
//
//
//    let repeat ( value:'Result ) : IObservable<'Result> =
//
//
//    let repeatWhile ( condition)( source:IObservable<'Source> ) : IObservable<'Source> =
//        Observable.DoWhile( source, Func<bool> condition)
//

    /// Returns a connectable observable sequence that shares a single subscription to the 
    /// underlying sequence replaying all notifications.
    /// This operator is a specialization of Multicast using a Reactive.Subject
    let replay ( source:IObservable<'Source>) : Subjects.IConnectableObservable<'Source> =        
        Observable.Replay( source )   


//    let replay  ( selector:Func<IObservable<'Source>, IObservable<'Result>>)( bufferSize:int ) ( window:TimeSpan ) ( source:IObservable<'Source>): IObservable<'Result> =
//
//
//    let replay  ( bufferSize:int )( window:TimeSpan )( source:IObservable<'Source>) : Subjects.IConnectableObservable<'Source> =
//
//
//    let replay  ( selector:Func<IObservable<'Source>,IObservable<'Result>> )( window:TimeSpan )( source:IObservable<'Source>) : IObservable<'Result> =
//
//
//    let replay  ( window:TimeSpan ) ( source:IObservable<'Source>): Subjects.IConnectableObservable<'Source> =
//
//
//    let replay ( selector:Func<IObservable<'Source>,IObservable<'Result>> )( source:IObservable<'Source>)  : IObservable<'Result> =
//
    /// Returns a connectable observable sequence that shares a single subscription 
    /// to the underlying sequence replaying all notifications.
         
//    
//                                  
//    let replay ( selector:Func<IObservable<'Source>,IObservable<'Result>> ) ( bufferSize:int )( source:IObservable<'Source>) : IObservable<'Result> =
//
//
//    let replay ( bufferSize:int )( source:IObservable<'Source>)  : Subjects.IConnectableObservable<'Source> =
//
//
    /// Repeats the source observable sequence until it successfully terminates.
    let retry ( source:IObservable<'Source>) : IObservable<'Source> =
        Observable.Retry( source )

//
//
//    let retry (retryCount:int) ( source:IObservable<'Source>) : IObservable<'Source> =
//

    ///  Returns an observable sequence that contains a single element.
    let returnObservable ( value:'Result) : IObservable<'Result> =
       Observable.Return(value)


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


    /// Samples the source observable sequence using a samper observable sequence producing sampling ticks.
    /// Upon each sampling tick, the latest element (if any) in the source sequence during the 
    /// last sampling interval is sent to the resulting sequence.
    let sampleWith   (sampler:IObservable<'Sample>) (source:IObservable<'Source>) : IObservable<'Source> =
        Observable.Sample( source, sampler )



    /// Applies an accumulator function over an observable sequence and returns each intermediate result.
    let scan (accumulator:'a->'a->'a)  source =
        Observable.Scan(source, Func<'a,'a,'a> accumulator  )


    /// Applies an accumulator function over an observable sequence and returns each intermediate result. 
    /// The specified init value is used as the initial accumulator value.
    let scanInit  (source:IObservable<'Source>) (init:'TAccumulate) (accumulator) : IObservable<'TAccumulate> =
        Observable.Scan( source, init, Func<'TAccumulate,'Source,'TAccumulate> accumulator )


    let selectMany selector source = 
        Observable.SelectMany(source,Func<'Source,int,seq<'Result>> selector )


//    let selectMany1 selector source = 
//        Observable.SelectMany(source,Func<'Sourec,int,IObservable<'Result>> selector )
//
//    let selectMany2 selector source = 
//        Observable.SelectMany(source,Func<'Source,int,CancellationToken,Tasks.Task<'Result>> selector)
//
//    let selectMany3 selector source = 
//        Observable.SelectMany(source,Func<'Source,int,Tasks.Task<'Result>> selector)
//
//
//    let selectMany4 selector source = 
//        Observable.SelectMany(source, Func<'Source,seq<'Result>> selector)
//
//
//    let selectMany5 selector source = 
//        Observable.SelectMany(source, Func<'S,IObservable<'R>> selector)
//
//    let selectMany6 selector source = 
//        Observable.SelectMany(source, Func<'S,CancellationToken,Tasks.Task<'R>> selector )
//
//
//    let selectMany    ( source:IObservable<'Source> )( taskSelector:Func<'Source,Threading.CancellationToken,Threading.Tasks.Task<'TaskResult>> )( resultSelector:Func<'Source,'TaskResult,'Result> ): IObservable<'Result> =
//        Observable.SelectMany    ( source:IObservable<'Source>, Func<'Source,Threading.CancellationToken,Threading.Tasks.Task<'TaskResult>> taskSelector, resultSelector:Func<'Source,'TaskResult,'Result> ): IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( taskSelector:Func<'Source,int,Threading.Tasks.Task<'TaskResult>>)( resultSelector:Func<'Source,int,'TaskResult,'Result> ) : IObservable<'Result> =
//        Observable.SelectMany    ( source:IObservable<'Source> )( taskSelector:Func<'Source,int,Threading.Tasks.Task<'TaskResult>>)( resultSelector:Func<'Source,int,'TaskResult,'Result> ) : IObservable<'Result> =
//

//    let selectMany    ( source:IObservable<'Source> )( taskSelector:Func<'Source,Threading.Tasks.Task<'TaskResult>> * resultSelector:Func<'Source,'TaskResult,'Result> ) IObservable<'Result> =
//        Observable.SelectMany    ( source:IObservable<'Source> )( taskSelector:Func<'Source,Threading.Tasks.Task<'TaskResult>> * resultSelector:Func<'Source,'TaskResult,'Result> ) IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( collectionSelector:Func<'Source,int,IObservable<'TCollection>> * resultSelector:Func<'Source,int,'TCollection,int,'Result> ) : IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( collectionSelector:Func<'Source,int,IObservable<'TCollection>> * resultSelector:Func<'Source,int,'TCollection,int,'Result> ) : IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( onNext:Func<'Source,int,IObservable<'Result>> * onError:Func<exn,IObservable<'Result>> * onCompleted:Func<IObservable<'Result>> ) : IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( onNext:Func<'Source,int,IObservable<'Result>> * onError:Func<exn,IObservable<'Result>> * onCompleted:Func<IObservable<'Result>> ) : IObservable<'Result> =


//    let selectMany    ( source:IObservable<'Source> )( selector:Func<'Source,Threading.CancellationToken,Threading.Tasks.Task<'Result>>): IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( selector:Func<'Source,Threading.CancellationToken,Threading.Tasks.Task<'Result>>): IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( selector:Func<'Source,int,Threading.CancellationToken,Threading.Tasks.Task<'Result>>): IObservable<'Result> =
//        Observable.SelectMany    ( source:IObservable<'Source> )( selector:Func<'Source,int,Threading.CancellationToken,Threading.Tasks.Task<'Result>>): IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( other:IObservable<'Other> ): IObservable<'Other> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( other:IObservable<'Other> ): IObservable<'Other> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( selector:Func<'Source,IObservable<'Result>>): IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( selector:Func<'Source,IObservable<'Result>>): IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( selector:Func<'Source,int,IObservable<'Result>>) : IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( selector:Func<'Source,int,IObservable<'Result>>) : IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( selector:Func<'Source,Threading.Tasks.Task<'Result>> ) : IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( selector:Func<'Source,Threading.Tasks.Task<'Result>> ) : IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( selector:Func<'Source,int,Threading.Tasks.Task<'Result>>) : IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( selector:Func<'Source,int,Threading.Tasks.Task<'Result>>) : IObservable<'Result> =
//
//
//    let selectMany    ( source:IObservable<'Source> )( collectionSelector:Func<'Source,IObservable<'TCollection>> )( resultSelector:Func<'Source,'TCollection,'Result>) : IObservable<'Result> =
//        Observable.SelectMany     ( source:IObservable<'Source> )( collectionSelector:Func<'Source,IObservable<'TCollection>> )( resultSelector:Func<'Source,'TCollection,'Result>) : IObservable<'Result> =


//    let sequenceEqual ( first:IObservable<'Source>  )( second:IObservable<'Source> ) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'Source>  )( second:IObservable<'Source> ) : IObservable<bool> =
//
//
//    let sequenceEqual ( first:IObservable<'Source>  )( second:IObservable<'Source> )( comparer:IEqualityComparer<'Source>) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'Source>  )( second:IObservable<'Source> )( comparer:IEqualityComparer<'Source>) : IObservable<bool> =
//
//    let sequenceEqual ( first:IObservable<'Source>  )( second:seq<'Source>) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'Source>  )( second:seq<'Source>) : IObservable<bool> =
//
//
//    let sequenceEqual ( first:IObservable<'Source>  )( second:seq<'Source> )( comparer:IEqualityComparer<'Source> ) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'Source>  )( second:seq<'Source> )( comparer:IEqualityComparer<'Source> ) : IObservable<bool> =
//
//
//
//    let single               ( source:IObservable<'Source>) (predicate:Func<'Source,bool> ) : 'Source =
//    let single               ( source:IObservable<'Source>) : 'Source =
//
//    let singleOrDefault      ( source:IObservable<'Source>) : 'Source =
//    let singleOrDefault      ( source:IObservable<'Source>) (predicate:Func<'Source,bool>) : 'Source =




    let selectIf condition thenSource =
        Observable.If( Func<bool> condition, thenSource )


    let selectIfElse condition ( elseSource : IObservable<'Result>) 
                               ( thenSource : IObservable<'Result>) =
        Observable.If( Func<bool> condition, thenSource, elseSource )


  
    let skip (count:int) (source:IObservable<'Source>)  : IObservable<'Source> =
        Observable.Skip(source , count)


    let skipSpan  (duration:TimeSpan ) (source:IObservable<'Source> ): IObservable<'Source> =
        Observable.Skip(source, duration)


    let skipLast  (count:int ) ( source:IObservable<'Source> ): IObservable<'Source> = 
        Observable.SkipLast (source, count )


    let skipLastSpan (duration:TimeSpan ) ( source:IObservable<'Source>) : IObservable<'Source> =
        Observable.SkipLast ( source, duration)


    let skipUntil ( other:IObservable<'Other> )  ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.SkipUntil(source, other )


    let skipUntilTime (startTime:DateTimeOffset ) ( source:IObservable<'Source> )  : IObservable<'Source> =
        Observable.SkipUntil(source, startTime )


    let skipWhile ( predicate:'Source -> bool ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.SkipWhile ( source, Func<'Source,bool> predicate ) 


    let skipWhilei ( predicate:'Source -> int -> bool)( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.SkipWhile ( source, Func<'Source,int,bool> predicate)


    /// Prepends an array of values to an observable sequence.   
    let startWith  (values:'T [])  (source: IObservable<'T>) : IObservable<'T> = 
        Observable.StartWith( source, values )


    /// Prepends a sequence of values to an observable sequence.   
    let startWithSeq (source:IObservable<'Source>) (values:seq<'Source>) : IObservable<'Source> =
        Observable.StartWith(source, values )


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
    let subscribeOn (context:Threading.SynchronizationContext) (source:IObservable<'Source>) : IObservable<'Source> =
        Observable.SubscribeOn( source, context )


    /// Transforms an observable sequence of observable sequences into an 
    /// observable sequence producing values only from the most recent 
    /// observable sequence.Each time a new inner observable sequnce is recieved,
    /// unsubscribe from the previous inner sequence
    let switch (sources:IObservable<IObservable<'Source>>) : IObservable<'Source>= 
        Observable.Switch(sources)

 //   static member Switch : sources:IObservable<Threading.Tasks.Task<'Source>> -> IObservable<'Source>



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


    /// Takes n elements (from the beginning of an observable sequence? )
    let take (n: int) source : IObservable<'Source> = 
        Observable.Take(source, n)    


    /// Returns a specified number of contiguous elements from the end of an obserable sequence
    let takeLast ( count:int ) source = 
        Observable.TakeLast(source, count)


    /// Returns elements within the specified duration from the end of the observable source sequence.
    let takeLastSpan ( duration:TimeSpan ) ( source:IObservable<'Source> ): IObservable<'Source> =
        Observable.TakeLast( source, duration )


    /// Returns a list with the elements within the specified duration from the end of the observable source sequence.
    let takeLastBuffer ( duration:TimeSpan )( source:IObservable<'Source> ): IObservable<Collections.Generic.IList<'Source>> =
        Observable.TakeLastBuffer( source, duration )


    /// Returns a list with the specified number of contiguous elements from the end of an observable sequence.
    let takeLastBufferCount ( count:int )( source:IObservable<'Source> ): IObservable<Collections.Generic.IList<'Source>> =
        Observable.TakeLastBuffer( source, count )


    /// Returns the elements from the source observable sequence until the other produces and element
    let takeUntilOther<'Other,'Source> other source =
        Observable.TakeUntil<'Source,'Other>(source , other )

//
    /// Returns the elements from the source observable until the specified time
    let takeUntilTime<'Source> (endtime:DateTimeOffset) source =
        Observable.TakeUntil<'Source>(source , endtime )


    /// Returns elements from an observable sequence as long as a specified condition is true.
    let takeWhile  (predicate) ( source:IObservable<'Source>): IObservable<'Source> =
        Observable.TakeWhile( source, Func<'Source,bool>predicate )


    /// Returns elements from an observable sequence as long as a specified condition is true.
    /// The element's index is used in the logic of the predicate functi
    let takeWhilei  ( predicate) (source:IObservable<'Source>) : IObservable<'Source> =
        Observable.TakeWhile( source, Func<'Source,int,bool> predicate )


    /// Ignores elements from an observable sequence which are followed by another element within a specified relative time duration.
    let throttle  (dueTime:TimeSpan) (source:IObservable<'Source>): IObservable<'Source> =
        Observable.Throttle( source, dueTime )


    /// Ignores elements from an observable sequence which are followed by another value within a computed throttle duration
    let throttleComputed ( source:IObservable<'Source>) (throttleDurationSelector) : IObservable<'Source> =
        Observable.Throttle( source, Func<'Source,IObservable<'Throttle>> throttleDurationSelector )


    /// Returns an observable sequence that terminates with an exception.
    let throw ( except:exn ) : IObservable<'Result> =
        Observable.Throw( except )

//    -- Unsure How to Wrap properly --
//    let throwWithWitness ( except:exn ) ( witness:obj) : IObservable<obj> =
//        Observable.Throw( except, witness )


    /// matches when the observable sequence has an available element and 
    /// applies the map
    let thenMap map source = 
        Observable.Then( source, Func<'Source,'Result> map )


    /// Records the time interval between consecutive elements in an observable sequence.
    let timeInterval ( source:IObservable<'Source>) : IObservable<TimeInterval<'Source>> =
        Observable.TimeInterval( source )


    /// Applies a timeout policy to the observable sequence based on an absolute time.
    /// If the sequence doesn't terminate before the specified absolute due time, a TimeoutException is propagated to the observer.
    let timeout ( timeout:System.DateTimeOffset ) ( source:IObservable<'Source>) =
        Observable.Timeout( source, timeout)


    /// Applies a timeout policy to the observable sequence based on an absolute time.
    /// If the sequence doesn't terminate before the specified absolute due time, the other 
    /// observable sequence is used to produce future messages from that point on.
    let timeoutOther ( timeout:System.DateTimeOffset ) ( other:IObservable<'Source>) ( source:IObservable<'Source>) =
        Observable.Timeout( source, timeout, other)


    /// Applies a timeout policy for each element in the observable sequence.
    /// If the next element isn't received within the specified timeout duration starting from its
    /// predecessor, a TimeoutException is propagated to the observer.
    let timeoutSpan ( timeout:TimeSpan ) ( source:IObservable<'Source> ) =
        Observable.Timeout( source, timeout)
        

    /// Applies a timeout policy for each element in the observable sequence.
    /// If the next element isn't received within the specified timeout duration starting from 
    /// its predecessor, the other observable sequence is used to produce future messages from that point on.
    let timeoutSpanOther( timeout:TimeSpan ) ( other:IObservable<'Source> ) ( source:IObservable<'Source> ) =
        Observable.Timeout( source, timeout, other)


    /// Applies a timeout policy to the observable sequence based on a timeout duration computed for each element.
    /// If the next element isn't received within the computed duration starting from its predecessor,
    /// a TimeoutException is propagated to the observer.
    let timeoutDuration ( durationSelector )( source:IObservable<'Source> ) : IObservable<'Source> =
        Observable.Timeout( source, Func<'Source,IObservable<'Timeout>> durationSelector   )


    /// Applies a timeout policy to the observable sequence based on an initial timeout duration 
    /// for the first element, and a timeout duration computed for each subsequent element.
    /// If the next element isn't received within the computed duration starting from its predecessor, 
    /// a TimeoutException is propagated to the observer.
    let timeout2Duration ( timeout:IObservable<'Timeout> ) 
                         ( durationSelector              ) 
                         ( source:IObservable<'Source>   ) =
        Observable.Timeout( source, Func<'Source, IObservable<'Timeout>> durationSelector)



    /// Applies a timeout policy to the observable sequence based on an initial timeout duration for the first
    /// element, and a timeout duration computed for each subsequent element.
    /// If the next element isn't received within the computed duration starting from its predecessor, 
    /// the other observable sequence is used to produce future messages from that point on.
    let timeout2DurationOther   ( timeout: IObservable<'Timeout>) 
                                ( durationSelector              )
                                ( other  : IObservable<'Source> ) 
                                ( source : IObservable<'Source> ) =
        Observable.Timeout( source, timeout, Func<'Source, IObservable<'Timeout>> durationSelector, other)


//    #endregion
    

    /// Returns an observable sequence that produces a single value at the specified absolute due time.
    let timer ( dueTime:DateTimeOffset ) : IObservable<int64> =
        Observable.Timer( dueTime )


    /// Returns an observable sequence that periodically produces a value starting at the specified initial absolute due time.
    let timerPeriod ( dueTime:DateTimeOffset) ( period:TimeSpan ) : IObservable<int64> =
        Observable.Timer( dueTime, period)


    /// Returns an observable sequence that produces a single value after the specified relative due time has elapsed.
    let timerSpan ( dueTime:TimeSpan ) : IObservable<int64> =   
        Observable.Timer( dueTime )


    /// Returns an observable sequence that periodically produces a value after the specified
    /// initial relative due time has elapsed.
    let timerSpanPeriod ( dueTime:TimeSpan, period:TimeSpan ) : IObservable<int64> =
        Observable.Timer( dueTime, period)


    /// Timestamps each element in an observable sequence using the local system clock.
    let timestamp ( source:IObservable<'Source> ) : IObservable<Timestamped<'Source>> =
        Observable.Timestamp( source )


    /// Converts an observable into a seq
    let toEnumerable (source: IObservable<'T>) = Observable.ToEnumerable(source)
    /// Creates an array from an observable sequence


    /// Creates an array from an observable sequence.
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
//    static member ToEvent : source:IObservable<'Source> -> IEvenSource<'Source>
//    static member ToEvent : source:IObservable<Unit> -> IEvenSource<Unit>
    
    /// Creates a list from an observable sequence
    let toList source = 
        Observable.ToList(source)


    /// Creates a lookup from an observable sequence according to a specified key selector function.
    let toLookup ( keySelector )( source:IObservable<'Source> ) : IObservable<Linq.ILookup<'Key,'Source>> =
       Observable.ToLookup( source, Func<'Source,'Key> keySelector )


    /// Creates a lookup from an observable sequence according to a specified key selector function, and a comparer.
    let toLookupCompare ( keySelector ) ( comparer:IEqualityComparer<'Key> )( source:IObservable<'Source> ) : IObservable<Linq.ILookup<'Key,'Source>> =
       Observable.ToLookup( source,Func<'Source,'Key> keySelector, comparer)


    /// Creates a lookup from an observable sequence according to a specified key selector function, and an element selector function.
    let toLookupElement ( keySelector ) ( elementSelector ) ( comparer:IEqualityComparer<'Key>)( source:IObservable<'Source> ) : IObservable<Linq.ILookup<'Key,'Element>>=
       Observable.ToLookup( source, Func<'Source,'Key> keySelector, Func<'Source,'Element> elementSelector, comparer )


    /// Creates a lookup from an observable sequence according to a specified key selector function, and an element selector function.
    let toLookupCompareElement ( keySelector ) ( elementSelector )( source:IObservable<'Source> ) : IObservable<Linq.ILookup<'Key,'Element>> =
       Observable.ToLookup( source,Func<'Source,'Key>  keySelector, Func<'Source,'Element> elementSelector )


    /// Converts a seq into an observable
    let toObservable ( source: seq<'T> ) = Observable.ToObservable(source)


    /// Constructs an observable sequence that depends on a resource object, whose 
    /// lifetime is tied to the resulting observable sequence's lifetime.
    let using ( resourceFactory: unit ->'TResource ) (observableFactory: 'TResource -> IObservable<'Result> ) : IObservable<'Result> =
        Observable.Using ( Func<_> resourceFactory, Func<_,_> observableFactory )


    /// waits for the observable sequence to complete and returns the last
    /// element of the sequence. If the sequence terminates with OnError
    /// notification, the exception is thrown
    let wait  source = 
        Observable.Wait( source )


    /// Repeats the given function as long as the specified condition holds
    /// where the condition is evaluated before each repeated source is 
    /// subscribed to
    let whileLoop condition source = 
        Observable.While( Func<bool> condition, source ) 
        

    /// Projects each element of an observable sequence into consecutive non-overlapping windows.
    /// windowClosingSelector - A function invoked to define the boundaries of the produced windows. 
    /// A new window is started when the previous one is closed
    let window ( windowClosingSelector ) ( source:IObservable<'Source> ) : IObservable<IObservable<'Source>> =
        Observable.Window( source, Func<IObservable<'WindowClosing>> windowClosingSelector)


    /// Projects each element of an observable sequence into consecutive non-overlapping windows 
    /// which are produced based on timing information.
    let windowTimeSpan ( timeSpan:TimeSpan )( source:IObservable<'Source> ) : IObservable<IObservable<'Source>> =
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
    let windowTimeShift ( timeSpan:TimeSpan )( timeShift:TimeSpan )( source:IObservable<'Source> ) : IObservable<IObservable<'Source>> =
        Observable.Window( source, timeSpan, timeShift )



    /// Projects each element of an observable sequence into consecutive non-overlapping windows
    /// windowBoundaries - Sequence of window boundary markers. The current window is closed 
    /// and a new window is opened upon receiving a boundary marker
    let windowBounded    ( windowBoundaries:IObservable<'WindowBoundary> )( source:IObservable<'Source> ) : IObservable<IObservable<'Source>> =
        Observable.Window( source, windowBoundaries )


    /// Projects each element of an observable sequence into zero or more windows which are produced based on element count information
    let windowCountSkip ( count:int )( skip:int ) ( source:IObservable<'Source> ): IObservable<IObservable<'Source>> =
        Observable.Window( source, count, skip )


    /// Projects each element of an observable sequence into consecutive non-overlapping windows 
    /// which are produced based on element count information.
    let windowCount ( count:int )( source:IObservable<'Source> ) : IObservable<IObservable<'Source>> =
        Observable.Window( source, count )


    /// Projects each element of an observable sequence into a window that is completed when either it's full or 
    /// a given amount of time has elapsed.
    /// A useful real-world analogy of this overload is the behavior of a ferry leaving the dock when all seats are 
    /// taken, or at the scheduled time of departure, whichever event occurs first
    let windowTimeCount ( timeSpan:TimeSpan ) (count:int) ( source:IObservable<'Source> ): IObservable<IObservable<'Source>> =
        Observable.Window( source, timeSpan, count )


    /// Merges two observable sequences into one observable sequence by combining their elements in a pairwise fashion.
    let zip ( first:IObservable<'Source1>)( second:IObservable<'Source2>) ( resultSelector:'Source1 -> 'Source2 -> 'Result) : IObservable<'Result> =
        Observable.Zip( first, second, Func<'Source1,'Source2,'Result> resultSelector)


    /// Merges the specified observable sequences into one observable sequence by emitting a
    ///  list with the elements of the observable sequences at corresponding indexes.
    let zipSeq ( sources:seq<IObservable<'Source>>) : IObservable<IList<'Source>> = 
        Observable.Zip( sources )


    /// Merges the specified observable sequences into one observable sequence by emitting 
    /// a list with the elements of the observable sequences at corresponding indexe
    let zipArray ( sources:IObservable<'Source> []) : IObservable<IList<'Source>> =
        Observable.Zip( sources )
 

    /// Merges the specified observable sequences into one observable sequence by using 
    /// the selector function whenever all of the observable sequences have produced an 
    /// element at a corresponding index.
    let zipSeqMap ( resultSelector: IList<'S> ->'R) ( sources: seq<IObservable<'S>>)  : IObservable<'R> =
        Observable.Zip( sources, Func<IList<'S>,'R> resultSelector)
 

 
    /// Merges an observable sequence and an enumerable sequence into one 
    /// observable sequence by using the selector function.
    let zipWithSeq ( resultSelector: 'Source1 -> 'Source2 -> 'Result   )
                   ( second        : seq<'Source2>                       )
                   ( first         : IObservable<'Source1>               ) : IObservable<'Result> =
        Observable.Zip(first, second, Func<_,_,_> resultSelector )
 



           
