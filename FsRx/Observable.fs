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
 
 
//    let Average : source:IObservable<Nullable<int>> -> IObservable<Nullable<float>>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,decimal> -> IObservable<decimal>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,float> -> IObservable<float>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,float32> -> IObservable<float32>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,int> -> IObservable<float>
//    let Average : source:IObservable<Nullable<int64>> -> IObservable<Nullable<float>>
//    let Average : source:IObservable<Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float>> -> IObservable<Nullable<float>>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float32>> -> IObservable<Nullable<float32>>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int>> -> IObservable<Nullable<float>>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,int64> -> IObservable<float>
//    let Average : source:IObservable<Nullable<float32>> -> IObservable<Nullable<float32>>
//    let Average : source:IObservable<int> -> IObservable<float>
//    let Average : source:IObservable<int64> -> IObservable<float>
//    let Average : source:IObservable<float> -> IObservable<float>
//    let Average : source:IObservable<Nullable<float>> -> IObservable<Nullable<float>>
//    let Average : source:IObservable<float32> -> IObservable<float32>
//    let Average : source:IObservable<decimal> -> IObservable<decimal>
//    let Average : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int64>> -> IObservable<Nullable<float>>

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


    let create subscribe =
        Observable.Create(Func<IObserver<'Result>,Action> subscribe)


    let create' subscribe =
        Observable.Create(Func<IObserver<'Result>,IDisposable> subscribe)

/////////////////////////////
/// TODO Go back and check reference to make new async creation methods that are unified
/// with F#'s async workflow

//    let create'' subscribe =
//        Observable.Create()
//
//    let create''' subscribe =
//        Observable.Create
//
//    let create'''' subscribe =
//        Observable.Create
//    static member Create : subscribe:Func<IObserver<'TResult>,Action> -> IObservable<'TResult>
//    static member Create : subscribeAsync:Func<IObserver<'TResult>,Threading.Tasks.Task<Action>> -> IObservable<'TResult>
//    static member Create : subscribeAsync:Func<IObserver<'TResult>,Threading.CancellationToken,Threading.Tasks.Task<Action>> -> IObservable<'TResult>
//    static member Create : subscribeAsync:Func<IObserver<'TResult>,Threading.Tasks.Task<IDisposable>> -> IObservable<'TResult>
//    static member Create : subscribeAsync:Func<IObserver<'TResult>,Threading.CancellationToken,Threading.Tasks.Task<IDisposable>> -> IObservable<'TResult>
//    static member Create : subscribeAsync:Func<IObserver<'TResult>,Threading.Tasks.Task> -> IObservable<'TResult>
//    static member Create : subscribeAsync:Func<IObserver<'TResult>,Threading.CancellationToken,Threading.Tasks.Task> -> IObservable<'TResult>
//    static member Create : subscribe:Func<IObserver<'TResult>,IDisposable> -> IObservable<'TResult>
////////////////////////////////////////////////------------------


    let createWithDisposable f =
        { new IObservable<_> with
            member this.Subscribe(observer:IObserver<_>) = f observer
        }


//    static member DefaultIfEmpty : source:IObservable<'TSource> * defaultValue:'TSource -> IObservable<'TSource>
//    static member DefaultIfEmpty : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member Defer : observableFactory:Func<IObservable<'TResult>> -> IObservable<'TResult>
//    static member Defer : observableFactoryAsync:Func<Threading.Tasks.Task<IObservable<'TResult>>> -> IObservable<'TResult>
//    static member DeferAsync : observableFactoryAsync:Func<Threading.CancellationToken,Threading.Tasks.Task<IObservable<'TResult>>> -> IObservable<'TResult>
//    static member Delay : source:IObservable<'TSource> * dueTime:TimeSpan -> IObservable<'TSource>
//    static member Delay : source:IObservable<'TSource> * dueTime:DateTimeOffset -> IObservable<'TSource>
//    static member Delay : source:IObservable<'TSource> * dueTime:DateTimeOffset * scheduler:IScheduler -> IObservable<'TSource>
//    static member Delay : source:IObservable<'TSource> * delayDurationSelector:Func<'TSource,IObservable<'TDelay>> -> IObservable<'TSource>
//    static member Delay : source:IObservable<'TSource> * subscriptionDelay:IObservable<'TDelay> * delayDurationSelector:Func<'TSource,IObservable<'TDelay>> -> IObservable<'TSource>
//    static member Delay : source:IObservable<'TSource> * dueTime:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member DelaySubscription : source:IObservable<'TSource> * dueTime:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member DelaySubscription : source:IObservable<'TSource> * dueTime:DateTimeOffset * scheduler:IScheduler -> IObservable<'TSource>
//    static member DelaySubscription : source:IObservable<'TSource> * dueTime:DateTimeOffset -> IObservable<'TSource>
//    static member DelaySubscription : source:IObservable<'TSource> * dueTime:TimeSpan -> IObservable<'TSource>



    let dematerialize source = 
        Observable.Dematerialize(source)





    /// Returns an observable sequence that only contains distinct elements 
    let distinct source = 
        Observable.Distinct(source)


//    static member Distinct : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<'TSource>
//    static member Distinct : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> -> IObservable<'TSource>
//    static member Distinct : source:IObservable<'TSource> * comparer:Collections.Generic.IEqualityComparer<'TSource> -> IObservable<'TSource>
//    static member Distinct : source:IObservable<'TSource> -> IObservable<'TSource>

    /// Returns an observable sequence that only contains distinct contiguous elements 
    let distinctUntilChanged source = 
        Observable.DistinctUntilChanged(source)

//    static member DistinctUntilChanged : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member DistinctUntilChanged : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<'TSource>
//    static member DistinctUntilChanged : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> -> IObservable<'TSource>
//    static member Do : source:IObservable<'TSource> * onNext:Action<'TSource> * onError:Action<exn> * onCompleted:Action -> IObservable<'TSource>
//    static member Do : source:IObservable<'TSource> * onNext:Action<'TSource> * onError:Action<exn> -> IObservable<'TSource>
//    static member Do : source:IObservable<'TSource> * onNext:Action<'TSource> * onCompleted:Action -> IObservable<'TSource>
//    static member Do : source:IObservable<'TSource> * observer:IObserver<'TSource> -> IObservable<'TSource>
//    static member Do : source:IObservable<'TSource> * onNext:Action<'TSource> -> IObservable<'TSource>
//    static member DoWhile : source:IObservable<'TSource> * condition:Func<bool> -> IObservable<'TSource>
//    static member ElementAt : source:IObservable<'TSource> * index:int -> IObservable<'TSource>
//    static member ElementAtOrDefault : source:IObservable<'TSource> * index:int -> IObservable<'TSource>


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

//
//    static member First : source:IObservable<'TSource> -> 'TSource
//    static member First : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> 'TSource
//
//
//    static member FirstAsync : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
//    static member FirstAsync : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member FirstOrDefault : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> 'TSource
//    static member FirstOrDefault : source:IObservable<'TSource> -> 'TSource
//    static member FirstOrDefaultAsync : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
//    static member FirstOrDefaultAsync : source:IObservable<'TSource> -> IObservable<'TSource>
//


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


//    static member For : source:Collections.Generic.IEnumerable<'TSource> * resultSelector:Func<'TSource,IObservable<'TResult>> -> IObservable<'TResult>
//    static member ForEach : source:IObservable<'TSource> * onNext:Action<'TSource> -> unit
//    static member ForEach : source:IObservable<'TSource> * onNext:Action<'TSource,int> -> unit
//    static member ForEachAsync : source:IObservable<'TSource> * onNext:Action<'TSource,int> * cancellationToken:Threading.CancellationToken -> Threading.Tasks.Task
//    static member ForEachAsync : source:IObservable<'TSource> * onNext:Action<'TSource,int> -> Threading.Tasks.Task
//    static member ForEachAsync : source:IObservable<'TSource> * onNext:Action<'TSource> * cancellationToken:Threading.CancellationToken -> Threading.Tasks.Task
//    static member ForEachAsync : source:IObservable<'TSource> * onNext:Action<'TSource> -> Threading.Tasks.Task
//    static member FromAsync : functionAsync:Func<Threading.Tasks.Task<'TResult>> -> IObservable<'TResult>
//    static member FromAsync : functionAsync:Func<Threading.CancellationToken,Threading.Tasks.Task<'TResult>> -> IObservable<'TResult>
//    static member FromAsync : actionAsync:Func<Threading.Tasks.Task> -> IObservable<Unit>
//    static member FromAsync : actionAsync:Func<Threading.CancellationToken,Threading.Tasks.Task> -> IObservable<Unit>


//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,AsyncCallback,obj,IAsyncResult> * end:Func<IAsyncResult,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,IObservable<'TResult>>
//    static member FromAsyncPattern : begin:Func<'TArg1,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,IObservable<Unit>>
//    static member FromAsyncPattern : begin:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,AsyncCallback,obj,IAsyncResult> * end:Action<IAsyncResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,IObservable<Unit>>
//////////////////////////////////////////////////

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

//    static member FromEvent : addHandler:Action<Action> * removeHandler:Action<Action> * scheduler:IScheduler -> IObservable<Unit>
//    static member FromEvent : addHandler:Action<Action> * removeHandler:Action<Action> -> IObservable<Unit>
//    static member FromEvent : addHandler:Action<Action<'TEventArgs>> * removeHandler:Action<Action<'TEventArgs>> -> IObservable<'TEventArgs>
//    static member FromEvent : addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> * scheduler:IScheduler -> IObservable<'TEventArgs>
//    static member FromEvent : addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> -> IObservable<'TEventArgs>
//    static member FromEvent : conversion:Func<Action<'TEventArgs>,'TDelegate> * addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> * scheduler:IScheduler -> IObservable<'TEventArgs>
//    static member FromEvent : conversion:Func<Action<'TEventArgs>,'TDelegate> * addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> -> IObservable<'TEventArgs>
//    static member FromEvent : addHandler:Action<Action<'TEventArgs>> * removeHandler:Action<Action<'TEventArgs>> * scheduler:IScheduler -> IObservable<'TEventArgs>

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
//    static member FromEventPattern : addHandler:Action<EventHandler> * removeHandler:Action<EventHandler> -> IObservable<EventPattern<EventArgs>>
//    static member FromEventPattern : type:Type * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<EventArgs>>
//    static member FromEventPattern : addHandler:Action<EventHandler> * removeHandler:Action<EventHandler> * scheduler:IScheduler -> IObservable<EventPattern<EventArgs>>
//    static member FromEventPattern : addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : target:obj * eventName:string -> IObservable<EventPattern<EventArgs>>
//    static member FromEventPattern : conversion:Func<EventHandler<'TEventArgs>,'TDelegate> * addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : type:Type * eventName:string -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : type:Type * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : type:Type * eventName:string -> IObservable<EventPattern<'TSender,'TEventArgs>>
//    static member FromEventPattern : type:Type * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'TSender,'TEventArgs>>
//    static member FromEventPattern : type:Type * eventName:string -> IObservable<EventPattern<EventArgs>>
//    static member FromEventPattern : target:obj * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'TSender,'TEventArgs>>
//    static member FromEventPattern : target:obj * eventName:string -> IObservable<EventPattern<'TSender,'TEventArgs>>
//    static member FromEventPattern : addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> * scheduler:IScheduler -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : target:obj * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : target:obj * eventName:string * scheduler:IScheduler -> IObservable<EventPattern<EventArgs>>
//    static member FromEventPattern : addHandler:Action<EventHandler<'TEventArgs>> * removeHandler:Action<EventHandler<'TEventArgs>> * scheduler:IScheduler -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : addHandler:Action<EventHandler<'TEventArgs>> * removeHandler:Action<EventHandler<'TEventArgs>> -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> * scheduler:IScheduler -> IObservable<EventPattern<'TSender,'TEventArgs>>
//    static member FromEventPattern : addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> -> IObservable<EventPattern<'TSender,'TEventArgs>>
//    static member FromEventPattern : conversion:Func<EventHandler<'TEventArgs>,'TDelegate> * addHandler:Action<'TDelegate> * removeHandler:Action<'TDelegate> * scheduler:IScheduler -> IObservable<EventPattern<'TEventArgs>>
//    static member FromEventPattern : target:obj * eventName:string -> IObservable<EventPattern<'TEventArgs>>

    let generate initialstate condition iterator selector = 
        Observable.Generate( initialstate, condition, iterator, selector )

//    static member Generate : initialState:'TState * condition:Func<'TState,bool> * iterate:Func<'TState,'TState> * resultSelector:Func<'TState,'TResult> * scheduler:IScheduler -> IObservable<'TResult>
//    static member Generate : initialState:'TState * condition:Func<'TState,bool> * iterate:Func<'TState,'TState> * resultSelector:Func<'TState,'TResult> * timeSelector:Func<'TState,DateTimeOffset> * scheduler:IScheduler -> IObservable<'TResult>
//    static member Generate : initialState:'TState * condition:Func<'TState,bool> * iterate:Func<'TState,'TState> * resultSelector:Func<'TState,'TResult> -> IObservable<'TResult>
//    static member Generate : initialState:'TState * condition:Func<'TState,bool> * iterate:Func<'TState,'TState> * resultSelector:Func<'TState,'TResult> * timeSelector:Func<'TState,TimeSpan> * scheduler:IScheduler -> IObservable<'TResult>
//    static member Generate : initialState:'TState * condition:Func<'TState,bool> * iterate:Func<'TState,'TState> * resultSelector:Func<'TState,'TResult> * timeSelector:Func<'TState,TimeSpan> -> IObservable<'TResult>
//    static member Generate : initialState:'TState * condition:Func<'TState,bool> * iterate:Func<'TState,'TState> * resultSelector:Func<'TState,'TResult> * timeSelector:Func<'TState,DateTimeOffset> -> IObservable<'TResult>
//    static member GetEnumerator : source:IObservable<'TSource> -> Collections.Generic.IEnumerator<'TSource>

    let groupBy source keySelector = 
        Observable.GroupBy( source, keySelector )

//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * capacity:int * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * capacity:int -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * capacity:int * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * capacity:int -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> * capacity:int -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * durationSelector:Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> * capacity:int * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * durationSelector:Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> * capacity:int -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * durationSelector:Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * durationSelector:Func<IGroupedObservable<'TKey,'TSource>,IObservable<'TDuration>> -> IObservable<IGroupedObservable<'TKey,'TSource>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> * capacity:int * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TElement>>
//    static member GroupByUntil : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * durationSelector:Func<IGroupedObservable<'TKey,'TElement>,IObservable<'TDuration>> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<IGroupedObservable<'TKey,'TElement>>

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


//    static member Last : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> 'TSource
//    static member Last : source:IObservable<'TSource> -> 'TSource

//    static member LastAsync : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member LastAsync : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
//    static member LastOrDefault : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> 'TSource
//    static member LastOrDefault : source:IObservable<'TSource> -> 'TSource
//    static member LastOrDefaultAsync : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member LastOrDefaultAsync : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
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


//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> * comparer:Collections.Generic.IComparer<'TResult> -> IObservable<'TResult>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> -> IObservable<'TResult>


//        static member Max : source:IObservable<'TSource> * comparer:Collections.Generic.IComparer<'TSource> -> IObservable<'TSource>
//    static member Max : source:IObservable<'TSource> -> IObservable<'TSource>

//
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,decimal> -> IObservable<decimal>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,int> -> IObservable<int>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int64>> -> IObservable<Nullable<int64>>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int>> -> IObservable<Nullable<int>>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float32>> -> IObservable<Nullable<float32>>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,int64> -> IObservable<int64>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,float> -> IObservable<float>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> * comparer:Collections.Generic.IComparer<'TResult> -> IObservable<'TResult>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> -> IObservable<'TResult>
//    static member Max : source:IObservable<Nullable<int64>> -> IObservable<Nullable<int64>>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,float32> -> IObservable<float32>
//    static member Max : source:IObservable<Nullable<int>> -> IObservable<Nullable<int>>
//    static member Max : source:IObservable<Nullable<float32>> -> IObservable<Nullable<float32>>
//    static member Max : source:IObservable<Nullable<float>> -> IObservable<Nullable<float>>
//    static member Max : source:IObservable<int64> -> IObservable<int64>
//    static member Max : source:IObservable<int> -> IObservable<int>
//    static member Max : source:IObservable<decimal> -> IObservable<decimal>
//    static member Max : source:IObservable<float32> -> IObservable<float32>
//    static member Max : source:IObservable<float> -> IObservable<float>
//    static member Max : source:IObservable<'TSource> * comparer:Collections.Generic.IComparer<'TSource> -> IObservable<'TSource>
//    static member Max : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member Max : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float>> -> IObservable<Nullable<float>>


//        static member MaxBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> -> IObservable<Collections.Generic.IList<'TSource>>
//    static member MaxBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * comparer:Collections.Generic.IComparer<'TKey> -> IObservable<Collections.Generic.IList<'TSource>>


    
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


    let maxOf (source:IObservable<'T>) = 
        Observable.Max( source )





//
//        static member Min : source:IObservable<'TSource> * selector:Func<'TSource,decimal> -> IObservable<decimal>
//
//            static member Min : source:IObservable<'TSource> * comparer:Collections.Generic.IComparer<'TSource> -> IObservable<'TSource>
//        // Min : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> -> IObservable<'TResult>
//           static member Min : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,decimal> -> IObservable<decimal>
//    static member Min : source:IObservable<Nullable<int>> -> IObservable<Nullable<int>>
//    static member Min : source:IObservable<Nullable<float32>> -> IObservable<Nullable<float32>>
//    static member Min : source:IObservable<Nullable<float>> -> IObservable<Nullable<float>>
//    static member Min : source:IObservable<int64> -> IObservable<int64>
//    static member Min : source:IObservable<int> -> IObservable<int>
//    static member Min : source:IObservable<decimal> -> IObservable<decimal>
//    static member Min : source:IObservable<float32> -> IObservable<float32>
//    static member Min : source:IObservable<float> -> IObservable<float>
//    static member Min : source:IObservable<'TSource> * comparer:Collections.Generic.IComparer<'TSource> -> IObservable<'TSource>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> -> IObservable<'TResult>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,'TResult> * comparer:Collections.Generic.IComparer<'TResult> -> IObservable<'TResult>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,float> -> IObservable<float>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,float32> -> IObservable<float32>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,int> -> IObservable<int>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,int64> -> IObservable<int64>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float>> -> IObservable<Nullable<float>>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float32>> -> IObservable<Nullable<float32>>
//    static member Min : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int>> -> IObservable<Nullable<int>>
//    static member Min : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int64>> -> IObservable<Nullable<int64>>
//    static member Min : source:IObservable<Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    static member Min : source:IObservable<Nullable<int64>> -> IObservable<Nullable<int64>>



//    static member MinBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> -> IObservable<Collections.Generic.IList<'TSource>>
//    static member MinBy : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * comparer:Collections.Generic.IComparer<'TKey> -> IObservable<Collections.Generic.IList<'TSource>>

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


//    static member OnErrorResumeNext : first:IObservable<'TSource> * second:IObservable<'TSource> -> IObservable<'TSource>
//    static member OnErrorResumeNext : sources:IObservable<'TSource> [] -> IObservable<'TSource>
//    static member OnErrorResumeNext : sources:Collections.Generic.IEnumerable<IObservable<'TSource>> -> IObservable<'TSource>
//

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


    /// Generates an observable sequence of integer nunbers whithin a specified range 
    /// using the specified scheduler to send out observer messages
    let rangeScheduled start count scheduler  = Observable.Range(start, count, scheduler )


    /// Reduces the observable
    let reduce f source = Observable.Aggregate(source, Func<_,_,_> f)

 
    /// Returns an observable that remains connected to the source as long
    /// as there is at least one subscription to the observable sequence 
    /// ( publish an Observable to get a ConnectableObservable )
    let refCount ( source )=
        Observable.RefCount ( source )   


// 
//    static member Repeat : value:'TResult * repeatCount:int -> IObservable<'TResult>
//    static member Repeat : value:'TResult * scheduler:IScheduler -> IObservable<'TResult>
//    static member Repeat : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member Repeat : source:IObservable<'TSource> * repeatCount:int -> IObservable<'TSource>
//    static member Repeat : value:'TResult -> IObservable<'TResult>
//    static member Repeat : value:'TResult * repeatCount:int * scheduler:IScheduler -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * bufferSize:int * window:TimeSpan -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * bufferSize:int * scheduler:IScheduler -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> * bufferSize:int * window:TimeSpan -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * bufferSize:int * scheduler:IScheduler -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * bufferSize:int * window:TimeSpan * scheduler:IScheduler -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * bufferSize:int * window:TimeSpan * scheduler:IScheduler -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * window:TimeSpan * scheduler:IScheduler -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * window:TimeSpan * scheduler:IScheduler -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * window:TimeSpan -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * window:TimeSpan -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * scheduler:IScheduler -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * scheduler:IScheduler -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> -> Subjects.IConnectableObservable<'TSource>
//    static member Replay : source:IObservable<'TSource> * selector:Func<IObservable<'TSource>,IObservable<'TResult>> * bufferSize:int -> IObservable<'TResult>
//    static member Replay : source:IObservable<'TSource> * bufferSize:int -> Subjects.IConnectableObservable<'TSource>


//
//    static member Retry : source:IObservable<'TSource> * retryCount:int -> IObservable<'TSource>
//    static member Retry : source:IObservable<'TSource> -> IObservable<'TSource>

        
//
//
//    static member Return : value:'TResult -> IObservable<'TResult>
//    static member Return : value:'TResult * scheduler:IScheduler -> IObservable<'TResult>

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
//    static member Sample : source:IObservable<'TSource> * interval:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
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


//    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> ) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> ) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:Collections.Generic.IEnumerable<'TSource> )( comparer:Collections.Generic.IEqualityComparer<'TSource> ) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:Collections.Generic.IEnumerable<'TSource> )( comparer:Collections.Generic.IEqualityComparer<'TSource> ) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> )( comparer:Collections.Generic.IEqualityComparer<'TSource>) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:IObservable<'TSource> )( comparer:Collections.Generic.IEqualityComparer<'TSource>) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:Collections.Generic.IEnumerable<'TSource>) : IObservable<bool> =
//    let sequenceEqual ( first:IObservable<'TSource>  )( second:Collections.Generic.IEnumerable<'TSource>) : IObservable<bool> =
//
//
//    let single               ( source:IObservable<'TSource>) (predicate:Func<'TSource,bool> ) : 'TSource =
//    let single               ( source:IObservable<'TSource>) : 'TSource =
//    let singleAsync          ( source:IObservable<'TSource>) : IObservable<'TSource> =
//    let singleAsync          ( source:IObservable<'TSource>) (predicate:Func<'TSource,bool>): IObservable<'TSource> =
//    let singleOrDefault      ( source:IObservable<'TSource>) : 'TSource =
//    let singleOrDefault      ( source:IObservable<'TSource>) (predicate:Func<'TSource,bool>) : 'TSource =
//    let singleOrDefaultAsync ( source:IObservable<'TSource>) : IObservable<'TSource> =
//    let singleOrDefaultAsync ( source:IObservable<'TSource>)( predicate:Func<'TSource,bool> ): IObservable<'TSource> =




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
  
  
//    static member Skip : source:IObservable<'TSource> * count:int -> IObservable<'TSource>
//    static member Skip : source:IObservable<'TSource> * duration:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member Skip : source:IObservable<'TSource> * duration:TimeSpan -> IObservable<'TSource>
//    static member SkipLast : source:IObservable<'TSource> * duration:TimeSpan -> IObservable<'TSource>
//    static member SkipLast : source:IObservable<'TSource> * count:int -> IObservable<'TSource>
//    static member SkipLast : source:IObservable<'TSource> * duration:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member SkipUntil : source:IObservable<'TSource> * other:IObservable<'TOther> -> IObservable<'TSource>
//    static member SkipUntil : source:IObservable<'TSource> * startTime:DateTimeOffset -> IObservable<'TSource>
//    static member SkipUntil : source:IObservable<'TSource> * startTime:DateTimeOffset * scheduler:IScheduler -> IObservable<'TSource>
//    static member SkipWhile : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
//    static member SkipWhile : source:IObservable<'TSource> * predicate:Func<'TSource,int,bool> -> IObservable<'TSource>
//  
//  
     

    /// Skips elements while the predicate is satisfied
    let skipWhile f source = Observable.SkipWhile(source, Func<_,_> f)
 

 
//    static member Start : function:Func<'TResult> -> IObservable<'TResult>
//    static member Start : function:Func<'TResult> * scheduler:IScheduler -> IObservable<'TResult>
//    static member Start : action:Action -> IObservable<Unit>
//    static member Start : action:Action * scheduler:IScheduler -> IObservable<Unit>

//    static member StartAsync : functionAsync:Func<Threading.Tasks.Task<'TResult>> -> IObservable<'TResult>
//    static member StartAsync : functionAsync:Func<Threading.CancellationToken,Threading.Tasks.Task<'TResult>> -> IObservable<'TResult>
//    static member StartAsync : actionAsync:Func<Threading.Tasks.Task> -> IObservable<Unit>
//    static member StartAsync : actionAsync:Func<Threading.CancellationToken,Threading.Tasks.Task> -> IObservable<Unit>




    let startWith source param = 
        Observable.StartWith( source, param )
//        
//    static member StartWith : source:IObservable<'TSource> * values:Collections.Generic.IEnumerable<'TSource> -> IObservable<'TSource>
//    static member StartWith : source:IObservable<'TSource> * scheduler:IScheduler * values:'TSource [] -> IObservable<'TSource>
//    static member StartWith : source:IObservable<'TSource> * values:'TSource [] -> IObservable<'TSource>
//    static member StartWith : source:IObservable<'TSource> * scheduler:IScheduler * values:Collections.Generic.IEnumerable<'TSource> -> IObservable<'TSource>
//

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


//    static member SubscribeOn : source:IObservable<'TSource> * scheduler:IScheduler -> IObservable<'TSource>
//    static member SubscribeOn : source:IObservable<'TSource> * context:Threading.SynchronizationContext -> IObservable<'TSource>
//
//
//
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,int> -> IObservable<int>
//    static member Sum : source:IObservable<int64> -> IObservable<int64>


//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,int> -> IObservable<int>
//    static member Sum : source:IObservable<int64> -> IObservable<int64>
//    static member Sum : source:IObservable<Nullable<float>> -> IObservable<Nullable<float>>
//    static member Sum : source:IObservable<Nullable<float32>> -> IObservable<Nullable<float32>>
//    static member Sum : source:IObservable<Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    static member Sum : source:IObservable<Nullable<int>> -> IObservable<Nullable<int>>
//    static member Sum : source:IObservable<int> -> IObservable<int>
//    static member Sum : source:IObservable<float32> -> IObservable<float32>
//    static member Sum : source:IObservable<Nullable<int64>> -> IObservable<Nullable<int64>>
//    static member Sum : source:IObservable<decimal> -> IObservable<decimal>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,float> -> IObservable<float>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,float32> -> IObservable<float32>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,decimal> -> IObservable<decimal>
//    static member Sum : source:IObservable<float> -> IObservable<float>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int64>> -> IObservable<Nullable<int64>>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,int64> -> IObservable<int64>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float>> -> IObservable<Nullable<float>>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<float32>> -> IObservable<Nullable<float32>>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<decimal>> -> IObservable<Nullable<decimal>>
//    static member Sum : source:IObservable<'TSource> * selector:Func<'TSource,Nullable<int>> -> IObservable<Nullable<int>>

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
    let synchronize  source = 
        Observable.Synchronize( source )


//    static member Synchronize : source:IObservable<'TSource> -> IObservable<'TSource>
//    static member Synchronize : source:IObservable<'TSource> * gate:obj -> IObservable<'TSource>

    /// Takes n elements
    let take (n: int) source = Observable.Take(source, n)    


    /// Returns a specified number of contiguous elements from the end of an obserable sequence
    let takeLast (count:int) source = 
        Observable.TakeLast(source, count)

//    static member Take : source:IObservable<'TSource> * count:int -> IObservable<'TSource>
//    static member Take : source:IObservable<'TSource> * count:int * scheduler:IScheduler -> IObservable<'TSource>
//    static member Take : source:IObservable<'TSource> * duration:TimeSpan -> IObservable<'TSource>
//    static member Take : source:IObservable<'TSource> * duration:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member TakeLast : source:IObservable<'TSource> * count:int * scheduler:IScheduler -> IObservable<'TSource>
//    static member TakeLast : source:IObservable<'TSource> * duration:TimeSpan -> IObservable<'TSource>
//    static member TakeLast : source:IObservable<'TSource> * duration:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member TakeLast : source:IObservable<'TSource> * duration:TimeSpan * timerScheduler:IScheduler * loopScheduler:IScheduler -> IObservable<'TSource>
//    static member TakeLast : source:IObservable<'TSource> * count:int -> IObservable<'TSource>
//    static member TakeLastBuffer : source:IObservable<'TSource> * duration:TimeSpan -> IObservable<Collections.Generic.IList<'TSource>>
//    static member TakeLastBuffer : source:IObservable<'TSource> * count:int -> IObservable<Collections.Generic.IList<'TSource>>
//    static member TakeLastBuffer : source:IObservable<'TSource> * duration:TimeSpan * scheduler:IScheduler -> IObservable<Collections.Generic.IList<'TSource>>
//

    /// Returns the elements from the source observable sequence until the other produces and element
    let takeUntil<'Other,'Source> other source =
        Observable.TakeUntil<'Source,'Other>(source , other )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTime<'Source> (endtime:DateTimeOffset) source =
        Observable.TakeUntil<'Source>(source , endtime )


    /// Returns the elements from the source observable until the specified time
    let takeUntilTimer<'Source> (endtime:DateTimeOffset) scheduler source =
        Observable.TakeUntil<'Source>(source , endtime, scheduler )
//
//    static member TakeWhile : source:IObservable<'TSource> * predicate:Func<'TSource,bool> -> IObservable<'TSource>
//    static member TakeWhile : source:IObservable<'TSource> * predicate:Func<'TSource,int,bool> -> IObservable<'TSource>
//





//
//    static member Throttle : source:IObservable<'TSource> * dueTime:TimeSpan -> IObservable<'TSource>
//    static member Throttle : source:IObservable<'TSource> * dueTime:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member Throttle : source:IObservable<'TSource> * throttleDurationSelector:Func<'TSource,IObservable<'TThrottle>> -> IObservable<'TSource>





//
//    static member Throw : exception:exn -> IObservable<'TResult>
//    static member Throw : exception:exn * scheduler:IScheduler * witness:'TResult -> IObservable<'TResult>
//    static member Throw : exception:exn * scheduler:IScheduler -> IObservable<'TResult>
//    static member Throw : exception:exn * witness:'TResult -> IObservable<'TResult>
//




    /// matches when the observable sequence has an available element and 
    /// applies the map
    let thenMap map source = 
        Observable.Then( source, Func<'Source,'Result> map )


//    static member TimeInterval : source:IObservable<'TSource> -> IObservable<TimeInterval<'TSource>>
//    static member TimeInterval : source:IObservable<'TSource> * scheduler:IScheduler -> IObservable<TimeInterval<'TSource>>
//    static member Timeout : source:IObservable<'TSource> * dueTime:TimeSpan * other:IObservable<'TSource> * scheduler:IScheduler -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * timeoutDurationSelector:Func<'TSource,IObservable<'TTimeout>> -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:DateTimeOffset * other:IObservable<'TSource> * scheduler:IScheduler -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * firstTimeout:IObservable<'TTimeout> * timeoutDurationSelector:Func<'TSource,IObservable<'TTimeout>> * other:IObservable<'TSource> -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:DateTimeOffset * other:IObservable<'TSource> -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:DateTimeOffset * scheduler:IScheduler -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:DateTimeOffset -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * timeoutDurationSelector:Func<'TSource,IObservable<'TTimeout>> * other:IObservable<'TSource> -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * firstTimeout:IObservable<'TTimeout> * timeoutDurationSelector:Func<'TSource,IObservable<'TTimeout>> -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:TimeSpan * scheduler:IScheduler -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:TimeSpan -> IObservable<'TSource>
//    static member Timeout : source:IObservable<'TSource> * dueTime:TimeSpan * other:IObservable<'TSource> -> IObservable<'TSource>
//
//
//
//    static member Timer : dueTime:TimeSpan * scheduler:IScheduler -> IObservable<int64>
//    static member Timer : dueTime:DateTimeOffset * scheduler:IScheduler -> IObservable<int64>
//    static member Timer : dueTime:DateTimeOffset * period:TimeSpan -> IObservable<int64>
//    static member Timer : dueTime:TimeSpan * period:TimeSpan -> IObservable<int64>
//    static member Timer : dueTime:DateTimeOffset -> IObservable<int64>
//    static member Timer : dueTime:TimeSpan * period:TimeSpan * scheduler:IScheduler -> IObservable<int64>
//    static member Timer : dueTime:TimeSpan -> IObservable<int64>
//    static member Timer : dueTime:DateTimeOffset * period:TimeSpan * scheduler:IScheduler -> IObservable<int64>



    let takeLastBuffer (count:int) source = 
        Observable.TakeLastBuffer( source, count )  

//    static member Timestamp : source:IObservable<'TSource> -> IObservable<Timestamped<'TSource>>
//    static member Timestamp : source:IObservable<'TSource> * scheduler:IScheduler -> IObservable<Timestamped<'TSource>>

    /// Converts an observable into a seq
    let toEnumerable (source: IObservable<'T>) = Observable.ToEnumerable(source)
    /// Creates an array from an observable sequence


    let toArray  source = 
        Observable.ToArray(source)


//    static member ToAsync : function:Func<'TResult> -> Func<IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TResult> -> Func<'TArg1,'TArg2,'TArg3,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TResult> -> Func<'TArg1,'TArg2,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TResult> * scheduler:IScheduler -> Func<'TArg1,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TResult> -> Func<'TArg1,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TResult> * scheduler:IScheduler -> Func<IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,IObservable<'TResult>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16,IObservable<Unit>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,IObservable<'TResult>>
//    static member ToAsync : action:Action<'TArg1> * scheduler:IScheduler -> Func<'TArg1,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2> -> Func<'TArg1,'TArg2,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2> * scheduler:IScheduler -> Func<'TArg1,'TArg2,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3> -> Func<'TArg1,'TArg2,'TArg3,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,IObservable<Unit>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,IObservable<'TResult>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1> -> Func<'TArg1,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16,IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,IObservable<Unit>>
//    static member ToAsync : action:Action * scheduler:IScheduler -> Func<IObservable<Unit>>
//    static member ToAsync : action:Action<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,IObservable<Unit>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16,IObservable<'TResult>>
//    static member ToAsync : action:Action -> Func<IObservable<Unit>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16,'TResult> -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TArg16,IObservable<'TResult>>
//    static member ToAsync : function:Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,'TResult> * scheduler:IScheduler -> Func<'TArg1,'TArg2,'TArg3,'TArg4,'TArg5,'TArg6,'TArg7,'TArg8,'TArg9,'TArg10,'TArg11,'TArg12,'TArg13,'TArg14,'TArg15,IObservable<'TResult>>


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


//
//    static member ToLookup : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> -> IObservable<Linq.ILookup<'TKey,'TSource>>
//    static member ToLookup : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<Linq.ILookup<'TKey,'TElement>>
//    static member ToLookup : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * comparer:Collections.Generic.IEqualityComparer<'TKey> -> IObservable<Linq.ILookup<'TKey,'TSource>>
//    static member ToLookup : source:IObservable<'TSource> * keySelector:Func<'TSource,'TKey> * elementSelector:Func<'TSource,'TElement> -> IObservable<Linq.ILookup<'TKey,'TElement>>
//    static member ToObservable : source:Collections.Generic.IEnumerable<'TSource> * scheduler:IScheduler -> IObservable<'TSource>
//

    /// Converts a seq into an observable
    let toObservable (source: seq<'T>) = Observable.ToObservable(source)


    /// Converts a seq into an observable sequence using the provided scheduler
    /// to run the enumeration loop
    let toObservableScheduled scheduler (source: seq<'T>) = Observable.ToObservable( source, scheduler )


////////////////////////////////////////////////////////
//// TODO FIX THIS ASYNC
//    let using ( resourceFactoryAsync:Func<Threading.CancellationToken,Threading.Tasks.Task<'TResource>> )( observableFactoryAsync:Func<'TResource,Threading.CancellationToken,Threading.Tasks.Task<IObservable<'TResult>>> -> IObservable<'TResult>
///////////////////////////////////////////////


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
        









    let window  ( source                : IObservable<'Source>              )
                ( windowOpenings        : IObservable<'WinOpen>             )
                ( windowClosingSelector : 'WinOpen->IObservable<'WinClose>  ) : IObservable<IObservable<'Source>> =
        Observable.Window(source, windowOpenings, Func<_,_> windowClosingSelector)

//
//    let Window ( source:IObservable<'TSource> )( timeSpan:TimeSpan * scheduler:IScheduler -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( timeSpan:TimeSpan * timeShift:TimeSpan -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( timeSpan:TimeSpan * timeShift:TimeSpan * scheduler:IScheduler -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( windowClosingSelector:Func<IObservable<'TWindowClosing>> -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( timeSpan:TimeSpan -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( windowBoundaries:IObservable<'TWindowBoundary> -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( timeSpan:TimeSpan * count:int * scheduler:IScheduler -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( count:int * skip:int -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( count:int -> IObservable<IObservable<'TSource>>
//    let Window ( source:IObservable<'TSource> )( timeSpan:TimeSpan * count:int -> IObservable<IObservable<'TSource>>
//
//
//



 
    let zip ( sources:IObservable<'TSource> []) : IObservable<IList<'TSource>> =
        Observable.Zip( sources )
 

    let Zip ( sources       : seq<IObservable<'TSource>>    )
            ( resultSelector: IList<'TSource>->'TResult     ) : IObservable<'TResult> =
        Observable.Zip( sources, Func<IList<'TSource>,'TResult> resultSelector)
 

//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( source11:IObservable<'TSource11> )( source12:IObservable<'TSource12> )( source13:IObservable<'TSource13> )( source14:IObservable<'TSource14> )( source15:IObservable<'TSource15> )( source16:IObservable<'TSource16> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TSource11,'TSource12,'TSource13,'TSource14,'TSource15,'TSource16,'TResult> -> IObservable<'TResult>)
//        Observable.Zip
// 
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( source11:IObservable<'TSource11> )( source12:IObservable<'TSource12> )( source13:IObservable<'TSource13> )( source14:IObservable<'TSource14> )( source15:IObservable<'TSource15> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TSource11,'TSource12,'TSource13,'TSource14,'TSource15,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( source11:IObservable<'TSource11> )( source12:IObservable<'TSource12> )( source13:IObservable<'TSource13> )( source14:IObservable<'TSource14> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TSource11,'TSource12,'TSource13,'TSource14,'TResult> -> IObservable<'TResult>
//        Observable.Zip    
//    
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( source11:IObservable<'TSource11> )( source12:IObservable<'TSource12> )( source13:IObservable<'TSource13> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TSource11,'TSource12,'TSource13,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( source11:IObservable<'TSource11> )( source12:IObservable<'TSource12> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TSource11,'TSource12,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( source11:IObservable<'TSource11> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TSource11,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( source10:IObservable<'TSource10> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TSource10,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( source9:IObservable<'TSource9> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TSource9,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( source8:IObservable<'TSource8> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TSource8,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( source7:IObservable<'TSource7> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TSource7,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> )( source4:IObservable<'TSource4>)(  source5:IObservable<'TSource5> )( source6:IObservable<'TSource6> )( resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TSource6,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( first:IObservable<'TSource1> * ) (second:IObservable<'TSource2>  )( resultSelector:Func<'TSource1,'TSource2,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> * resultSelector:Func<'TSource1,'TSource2,'TSource3,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let Zip ( source1:IObservable<'TSource1> ) (source2:IObservable<'TSource2> )( source3:IObservable<'TSource3> * source4:IObservable<'TSource4> * source5:IObservable<'TSource5> * resultSelector:Func<'TSource1,'TSource2,'TSource3,'TSource4,'TSource5,'TResult> -> IObservable<'TResult>
//        Observable.Zip
// 
//
//    let zip ( resultSelector: 'TSource1 -> 'TSource2 -> 'TResult         )
//            ( second        : Collections.Generic.IEnumerable<'TSource2> )
//            ( first         : IObservable<'TSource1>                     ) : IObservable<'TResult> =
//        Observable.Zip(first, second, Func<_,_,_> resultSelector )
// 
//
//
//    let Zip ( sources:Collections.Generic.IEnumerable<IObservable<'TSource>> -> IObservable<Collections.Generic.IList<'TSource>>
//
//           
//
//
//
//
//
//






































