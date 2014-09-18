// ----------------------------------------------------------------------------
// (c) Jared Hester, 2014
// ----------------------------------------------------------------------------


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
 
 
 ///////////////////////////////////////////////

///  TODO :: average 20

////////////////////////////////////////////////

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


    /// Projects each element of an observable sequence into 
    /// consequtive non-overlapping buffers produced based on timing information
    /// using the specified scheduler to run timing
    let bufferSpanScheduled (timespan:TimeSpan) (scheduler:Concurrency.IScheduler) source = 
        Observable.Buffer(source, timespan, scheduler )
    

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


    /// Projects each element of an observable sequence into a buffer that goes
    /// sent out when either it's full or a specific amount of time has elapsed
    /// using the specified scheduler to run timing
    /// Analogy - A boat that departs when it's full or at its scheduled time to leave
    let bufferSpanCountScheduled timeSpan (count:int) scheduler source = 
        Observable.Buffer(source, timeSpan, count, scheduler )


    /// Projects each element of an observable sequence into 
    /// consequtive non-overlapping buffers produced based on timing information
    let bufferSpanShiftScheduled (timeSpan:TimeSpan) (timeShift:TimeSpan) scheduler source = 
        Observable.Buffer(source, timeSpan, timeShift, scheduler )


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


    /// Uses selector to determine which source in sources to return,
    /// choosing an empty sequence on the specified scheduler if no match is found
    let caseScheduler selector (scheduler:Concurrency.IScheduler) sources =
        Observable.Case( Func<_> selector, sources, scheduler )


    /// Continues an observable sequence that is terminated
    /// by an exception with the next observable sequence.
    let catch (second: IObservable<'T>) first =
        Observable.Catch(first, second) 

///////////////////////////////////////////////

///  TODO :: catch 3

////////////////////////////////////////////////


    /// Produces an enumerable sequence of consequtive (possibly empty) chunks of the source observable
    let chunkify<'Source> source : seq<IList<'Source>> = 
        Observable.Chunkify<'Source>( source )

///////////////////////////////////////////////

///  TODO :: collect 2

////////////////////////////////////////////////
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


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap2 ( map : 'T1->'T2->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> )  =
        Observable.CombineLatest( s1,s2,Func<'T1,'T2,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap3 ( map : 'T1->'T2->'T3->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> )=
        Observable.CombineLatest( s1,s2,s3, Func<'T1,'T2,'T3,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap4 ( map : 'T1->'T2->'T3->'T4->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> ) =
        Observable.CombineLatest( s1,s2,s3,s4, Func<'T1,'T2,'T3,'T4,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap5 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->
                                            'T9->'T10->'T11->'T12->'T13->'T14->'T15->'T16->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>) (s12:IObservable<'T12>)
                        (s13:IObservable<'T13>) (s14:IObservable<'T14>) (s15:IObservable<'T15>) (s16:IObservable<'T16>) =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,s15,s16,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,
                                        'T10,'T11,'T12,'T13,'T14,'T15,'T16,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap6 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> )  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,Func<'T1,'T2,'T3,'T4,'T5,'T6,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap7 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> )  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap8 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> ) =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap9 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->'T9->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> )  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap10 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->'T9->'T10->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>)  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,
                                    Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,'T10,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap11 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->'T9->'T10->'T11->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>)  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,'T10,'T11,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap12 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->
                                    'T7->'T8->'T9->'T10->'T11->'T12->'Result )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>) (s12:IObservable<'T12>) =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,'T10,'T11,'T12,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap13 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->
                                            'T9->'T10->'T11->'T12->'T13->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>) (s12:IObservable<'T12>)
                        (s13:IObservable<'T13>)  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,'T10,'T11,'T12,'T13,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap14 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->
                                            'T9->'T10->'T11->'T12->'T13->'T14->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>) (s12:IObservable<'T12>)
                        (s13:IObservable<'T13>) (s14:IObservable<'T14>)  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,'T10,'T11,'T12,'T13,'T14,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap15 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->
                                            'T9->'T10->'T11->'T12->'T13->'T14->'T15->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>) (s12:IObservable<'T12>)
                        (s13:IObservable<'T13>) (s14:IObservable<'T14>) (s15:IObservable<'T15>)  =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,s15,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,
                                        'T10,'T11,'T12,'T13,'T14,'T15,'Result> map )


    /// Merges the specified observable sequences into one observable sequence by  applying the map
    /// whenever any of the observable sequences produces and element.
    let combineLatestMap16 ( map : 'T1->'T2->'T3->'T4->'T5->'T6->'T7->'T8->
                                            'T9->'T10->'T11->'T12->'T13->'T14->'T15->'T16->'Result  )
                        (s1 :IObservable<'T1> ) (s2 :IObservable<'T2> ) (s3 :IObservable<'T3> ) (s4 :IObservable<'T4> )
                        (s5 :IObservable<'T5> ) (s6 :IObservable<'T6> ) (s7 :IObservable<'T7> ) (s8 :IObservable<'T8> )
                        (s9 :IObservable<'T9> ) (s10:IObservable<'T10>) (s11:IObservable<'T11>) (s12:IObservable<'T12>)
                        (s13:IObservable<'T13>) (s14:IObservable<'T14>) (s15:IObservable<'T15>) (s16:IObservable<'T16>) =
        Observable.CombineLatest( s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,s15,s16,
                                  Func<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'T8,'T9,
                                        'T10,'T11,'T12,'T13,'T14,'T15,'T16,'Result> map )

    // #endregion 


    /// Concatenates the second observable sequence to the first observable sequence
    /// upn the successful termination of the first 
    let concat (second: IObservable<'T>) (first: IObservable<'T>) =
        Observable.Concat(first, second)
    

    /// Concatenates all observable sequences within the sequence as long as
    /// the previous observable sequence terminated successfully 
    let concatSeq (sources:seq<IObservable<'T>>) : IObservable<'T>=
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



    /// Produces and enumerable sequence that returns elements collected/aggregated 
    /// from the source sequence between consecutive iterations 
    let collect  newCollector merge source = 
        Observable.Collect(source, newCollector, merge )

///////////////////////////////////////////////

///  TODO :: collect

////////////////////////////////////////////////


    /// Connects the observable wrapper to its source. All subscribed
    /// observers will recieve values from the underlying observable
    /// sequence as long as the connection is established.    
    /// ( publish an Observable to get a ConnectableObservable )
    let connect ( source:Subjects.IConnectableObservable<_> )=    
        source.Connect()

///////////////////////////////////////////////

///  TODO :: contains 2

////////////////////////////////////////////////


    /// Counts the elements
    let count source = Observable.Count(source)

///////////////////////////////////////////////

///  TODO :: count

////////////////////////////////////////////////




///////////////////////////////////////////////

///  TODO :: Create 8

////////////////////////////////////////////////------------------


    let createWithDisposable f =
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) = f observer
        }


///////////////////////////////////////////////

///  TODO :: defaultIfEmpty 2

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: defer 2

////////////////////////////////////////////////

///////////////////////////////////////////////

///  TODO :: defer async

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: delay 6

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: delaySubscription 4

////////////////////////////////////////////////



    let dematerialize source = 
        Observable.Dematerialize(source)





    /// Returns an observable sequence that only contains distinct elements 
    let distinct source = 
        Observable.Distinct(source)


///////////////////////////////////////////////

///  TODO :: distinct 3

////////////////////////////////////////////////

    /// Returns an observable sequence that only contains distinct contiguous elements 
    let distinctUntilChanged source = 
        Observable.DistinctUntilChanged(source)

///////////////////////////////////////////////

///  TODO :: distinctUntilChanged 3

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: doWhile

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: elementat

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: elementOrDefault

////////////////////////////////////////////////


    /// Returns an empty observable
    let empty<'T> = Observable.Empty<'T>()


    /// Returns an empty Observable sequence
    let emptyWitness<'T>(witness:'T) :IObservable<'T> =
             Observable.Empty( witness )

    /// Returns an empty Observable sequence, using the specified 
    /// scheduler to send out the single OnCompleted message
    let emptyScheduled<'T> (scheduler:Concurrency.IScheduler) = 
            Observable.Empty<'T>(scheduler)


     /// Generates an empty observable
    let emptyScheduledWitness<'T> (scheduler:Concurrency.IScheduler) (witness:'T) =
         Observable.Empty<'T>(scheduler, witness)


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


///////////////////////////////////////////////

///  TODO :: firstOrDefault 2

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: first or default async 2

////////////////////////////////////////////////




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


///////////////////////////////////////////////

///  TODO :: for

////////////////////////////////////////////////

///////////////////////////////////////////////

///  TODO :: forEach 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: FromAsync 4

////////////////////////////////////////////////

///////////////////////////////////////////////

///  TODO :: FromAsyncPattern 30

////////////////////////////////////////////////

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

///////////////////////////////////////////////

///  TODO :: fromEVent 7

////////////////////////////////////////////////

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

///////////////////////////////////////////////

///  TODO :: fromEventPattern 21

////////////////////////////////////////////////

    let generate initialstate condition iterator selector = 
        Observable.Generate( initialstate, condition, iterator, selector )

///////////////////////////////////////////////

///  TODO :: generate 5

////////////////////////////////////////////////



    let groupBy source keySelector = 
        Observable.GroupBy( source, keySelector )

///////////////////////////////////////////////

///  TODO :: groupBy 7

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: groupbyuntil 8

////////////////////////////////////////////////

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
    let joinWhen (plans:seq<Joins.Plan<'T>>): IObservable<'T>= 
        Observable.When( plans )


///////////////////////////////////////////////

///  TODO :: last 2

////////////////////////////////////////////////

///////////////////////////////////////////////

///  TODO :: last async 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: lastor default 2

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: lastordefault async 2

////////////////////////////////////////////////


    let latest source = 
        Observable.Latest( source )


    /// Returns an observable sequence containing a int64 that represents 
    /// the total number of elements in an observable sequence 
    let longCount source = 
        Observable.LongCount(source)


///////////////////////////////////////////////

///  TODO :: longcount

////////////////////////////////////////////////



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



///////////////////////////////////////////////

///  TODO :: max 23

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: maxby 2

////////////////////////////////////////////////
    
    
    /// Merges the two observables
    let merge (second: IObservable<'T>) (first: IObservable<'T>) = Observable.Merge(first, second)


    /// Merges elements from two observable sequences into a single observable sequence 
    /// usind the specified scheduler for enumeration and for subscriptions
    let mergeScheduled (scheduler:Concurrency.IScheduler)(second:IObservable<'T>)(first: IObservable<'T>) =
        Observable.Merge( first, second, scheduler)


    /// Merges all the observable sequences into a single observable sequence.
    let mergeArray (sources:IObservable<'T>[]) =
        Observable.Merge(sources)


    /// Merges an enumerable sequence of observable sequences into a single observable sequence.
    /// usind the specified scheduler for enumeration and for subscriptions
    let mergeArrayScheduled (scheduler:Concurrency.IScheduler)(sources:IObservable<'T>[]) =
        Observable.Merge(scheduler, sources )


    /// Merges elements from all inner observable sequences 
    /// into a single  observable sequence.
    let mergeInner (sources:IObservable<IObservable<'T>>) =
        Observable.Merge(sources)


    /// Merges elements from all inner observable sequences 
    /// into a single  observable sequence limiting the number of concurrent 
    /// subscriptions to inner sequences
    let mergeInnerScheduled (maxConcurrent:int) (sources:IObservable<IObservable<'T>>) =
        Observable.Merge(sources, maxConcurrent)


    /// Merges an enumerable sequence of observable sequences into a single observable sequence.
    let mergeSeq (sources:seq<IObservable<'T>>) =
        Observable.Merge(sources)


    /// Merges an enumerable sequence of observable sequences into an observable sequence,
    ///  limiting the number of concurrent subscriptions to inner sequences.
    let mergeSeqMax (maxConcurrent:int)(sources:seq<IObservable<'T>>) =
        Observable.Merge(sources, maxConcurrent)


    /// Merges an enumerable sequence of observable sequences into a single observable sequence.
    /// usind the specified scheduler for enumeration and for subscriptions
    let mergeSeqScheduled (scheduler:Concurrency.IScheduler) (sources:seq<IObservable<'T>>) =
        Observable.Merge(sources, scheduler )


    let mergeSeqMaxScheduled (scheduler:Concurrency.IScheduler)(maxConcurrent:int)(sources:seq<IObservable<'T>>) =
        Observable.Merge(sources,maxConcurrent, scheduler)


    /// Merge results from all source tasks into a single observable sequence
    let mergeTasks (sources:IObservable<Tasks.Task<'T>>) =
        Observable.Merge(sources)











    /// TODO IMPLEMENT 19 OVERLOADS of MAX
    let maxOf (source:IObservable<'T>) = 
        Observable.Max( source )

///////////////////////////////////////////////

///  TODO :: min 24

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: minby 2

////////////////////////////////////////////////

    /// Returns an enumerable sequence whose sequence whose enumeration returns the 
    /// most recently observed element in the source observable sequence, using 
    /// the specified 
    let mostRecent initialVal source = 
        Observable.MostRecent( source, initialVal )

///////////////////////////////////////////////

///  TODO :: multicast 2

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: never 2

////////////////////////////////////////////////



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

///////////////////////////////////////////////

///  TODO :: observeOn 2

////////////////////////////////////////////////

    /// Filters the elements of an observable sequence based on the specified type
    let ofType source = 
        Observable.OfType( source )
        
 




    let onErrorResumeNext sources : IObservable<'Source> = 
        Observable.OnErrorResumeNext(sources)

///////////////////////////////////////////////

///  TODO ::  onerrorresumeNext 2

////////////////////////////////////////////////




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

    
///////////////////////////////////////////////

///  TODO :: range 1

////////////////////////////////////////////////


    /// Reduces the observable
    let reduce f source = Observable.Aggregate(source, Func<_,_,_> f)

 
    /// Returns an observable that remains connected to the source as long
    /// as there is at least one subscription to the observable sequence 
    /// ( publish an Observable to get a ConnectableObservable )
    let refCount ( source )=
        Observable.RefCount ( source )   


 
///////////////////////////////////////////////

///  TODO :: repeat 6

////////////////////////////////////////////////





///////////////////////////////////////////////

///  TODO :: replay 16

////////////////////////////////////////////////





///////////////////////////////////////////////

///  TODO :: retry  2

////////////////////////////////////////////////


        


///////////////////////////////////////////////

///  TODO :: return 2

////////////////////////////////////////////////


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

        
///////////////////////////////////////////////

///  TODO :: sample 2

////////////////////////////////////////////////





    /// Applies an accumulator function over an observable sequence
    /// and returns each intermediate result. The specified seed value 
    /// is used as the initial accumulator value. For aggreagation behavior
    /// without intermediate results use 'aggregate'
    let scan (collector:'a->'b->'a) state source =
        Observable.Scan(source,  state, Func<'a,'b,'a>collector )

        
///////////////////////////////////////////////

///  TODO :: scan overload

////////////////////////////////////////////////




///////////////////////////////////////////////

///  TODO :: select many 19

////////////////////////////////////////////////



///////////////////////////////////////////////

///  TODO :: sequence equal 4

////////////////////////////////////////////////






///////////////////////////////////////////////

///  TODO :: single 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: singleasync 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: singleordefault 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: singleOrDefaultAsync 2

////////////////////////////////////////////////




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
  
  
///////////////////////////////////////////////

///  TODO ::skip 2

////////////////////////////////////////////////


  
///////////////////////////////////////////////

///  TODO :: skip last 3

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: skip until 6

////////////////////////////////////////////////


  
  
     

    /// Skips elements while the predicate is satisfied
    let skipWhile f source = Observable.SkipWhile(source, Func<_,_> f)
 

 
///////////////////////////////////////////////

///  TODO :: Start 4

////////////////////////////////////////////////




 
///////////////////////////////////////////////

///  TODO :: start async 4

////////////////////////////////////////////////




    let startWith source param = 
        Observable.StartWith( source, param )
        
///////////////////////////////////////////////

///  TODO :: startwith 3

////////////////////////////////////////////////


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

///////////////////////////////////////////////

///  TODO :: subscribeOn 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: sum 20

////////////////////////////////////////////////



    /// Transforms an observable sequence of observable sequences into an 
    /// observable sequence producing values only from the most recent 
    /// observable sequence.Each time a new inner observable sequnce is recieved,
    /// unsubscribe from the previous inner sequence
    let switch (sources:IObservable<IObservable<'Source>>) : IObservable<'Source>= 
        Observable.Switch(sources)

///////////////////////////////////////////////

///  TODO :: switch

////////////////////////////////////////////////

    /// Synchronizes the observable sequence so that notifications cannot be delivered concurrently
    /// this voerload is useful to "fix" and observable sequence that exhibits concurrent 
    /// callbacks on individual observers, which is invalid behavior for the query processor
    let synchronizeFix  source = 
        Observable.Synchronize( source )


///////////////////////////////////////////////

///  TODO :: synchronize 2

////////////////////////////////////////////////

    /// Takes n elements
    let take (n: int) source = Observable.Take(source, n)    


    /// Returns a specified number of contiguous elements from the end of an obserable sequence
    let takeLast (count:int) source = 
        Observable.TakeLast(source, count)

///////////////////////////////////////////////

///  TODO :: takelast 4

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: takelastbuffer 3

////////////////////////////////////////////////



    /// Returns the elements from the source observable sequence until the other produces and element
    let takeUntil<'Other,'Source> other source =
        Observable.TakeUntil<'Source,'Other>(source , other )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTime<'Source> (endtime:DateTimeOffset) source =
        Observable.TakeUntil<'Source>(source , endtime )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTimer<'Source> (endtime:DateTimeOffset) scheduler source =
        Observable.TakeUntil<'Source>(source , endtime, scheduler )


///////////////////////////////////////////////

///  TODO :: takewhile 2

////////////////////////////////////////////////






///////////////////////////////////////////////

///  TODO :: throttle 3

////////////////////////////////////////////////






///////////////////////////////////////////////

///  TODO :: throw 4

////////////////////////////////////////////////



    /// matches when the observable sequence has an available element and 
    /// applies the map
    let thenMap map source = 
        Observable.Then( source, Func<'Source,'Result> map )

///////////////////////////////////////////////

///  TODO :: timeinterval 2

////////////////////////////////////////////////


///////////////////////////////////////////////

///  TODO :: timer 8

////////////////////////////////////////////////



    let takeLastBuffer (count:int) source = 
        Observable.TakeLastBuffer( source, count )  

///////////////////////////////////////////////

///  TODO :: timestamp 2

////////////////////////////////////////////////


    /// Converts an observable into a seq
    let toEnumerable (source: IObservable<'T>) = Observable.ToEnumerable(source)
    /// Creates an array from an observable sequence


    let toArray  source = 
        Observable.ToArray(source)


///////////////////////////////////////////////

///  TODO :: to Async 69

////////////////////////////////////////////////

    /// Creates a list from an observable sequence
    let toList source = 
        Observable.ToList(source)




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

///////////////////////////////////////////////

///  TODO :: to event

////////////////////////////////////////////////

    /// Converts a seq into an observable
    let toObservable (source: seq<'T>) = Observable.ToObservable(source)
    
///////////////////////////////////////////////

///  TODO :: tolookup 4

////////////////////////////////////////////////




///////////////////////////////////////////////

///  TODO :: toobservable

////////////////////////////////////////////////




///////////////////////////////////////////////

///  TODO :: using 2

////////////////////////////////////////////////

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

///////////////////////////////////////////////

///  TODO :: while

////////////////////////////////////////////////




///////////////////////////////////////////////

///  TODO :: window 11

////////////////////////////////////////////////

    /// Returns an observable that yields sliding windows of 
    /// containing elements drawn from the input observable. 
    /// Each window is returned as a fresh array.



///////////////////////////////////////////////

///  TODO :: zip 19

////////////////////////////////////////////////