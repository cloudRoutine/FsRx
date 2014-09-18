#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\FSharp.Control.Observable.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.Core.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.Interfaces.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.Linq.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.PlatformServices.dll"
#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.Collections"

open FSharp.Control.Observable
open System.Reactive
open System




let createTimerAndObservable timerInterval runtime =
    // setup a timer
    let timer = new System.Timers.Timer(float timerInterval)
    timer.AutoReset <- true
    // events are automatically IObservable
    let observable = timer.Elapsed  
    // return an async task
    let task  = 
      async {   timer.Start()
                do! Async.Sleep runtime
                timer.Stop()
            }
    // return a async task and the observable
    (task,observable)


let Timer1 , Stream1 = createTimerAndObservable 1000 18000
let Timer2 , Stream2 = createTimerAndObservable 1000 18000
let Timer3 , Stream3 = createTimerAndObservable 1000 18000
let Timer4 , Stream4 = createTimerAndObservable 1000 18000


//Stream1 
//    |> subscribe ( fun _ -> printf " |S1 :: tick %A| " DateTime.Now.Second )
//
//
//
//Stream2 
//    |> map       ( fun x -> x.SignalTime.Second * 1000 )
//    |> map       ( fun x -> " |S2 :: amped " + x.ToString() + "| "   )
//    |> subscribe ( fun x -> printf "%A" x )
//
//Stream2 
//    |> subscribe ( fun x -> printf " |S2 :: ?? |" )
//
//Stream2 
//    |> subscribe ( fun x -> printf "\n" )
//
//Stream1 
//    |> filter    ( fun x -> 0 = x.SignalTime.Second % 3 )
//    |> subscribe ( fun _ -> printf " <<S1 :: filtered %A>> " DateTime.Now.Second )


let one =  Stream1
        |> map   ( fun x -> "Earth")
    

let two = Stream1
        |> filter   ( fun x -> 0L = x.SignalTime.Ticks % 2L )
        |> map      ( fun x -> "Fire" )

let tri = Stream1
        |> filter  ( fun x -> 0L = x.SignalTime.Ticks % 3L )
        |> map     ( fun x -> "Wind" )

let qua = Stream1
        |> filter  ( fun x -> 0L = x.SignalTime.Ticks % 4L )
        |> map     ( fun x -> "Water" )

let out = (fun x -> printf "%A" x)
let outn = (fun x -> printfn "%A" x)
//
//one |> subscribe out
//two |> subscribe out
//tri |> subscribe outn
//qua |> subscribe outn

//
//let left    = concat two one
//let right   = concat qua tri
//let power   = concat left right

let left    = merge two one
let right   = merge qua tri
let power   = merge right left



            //|> scan ( fun (x:string) (y:string) -> x+y ) ""
            
Stream1 |> subscribe ( fun x -> printfn "" )
power  |> subscribe out
//power|> subscribe ( fun x -> printfn "%A \nFuck Heart, Captain Murphy is BACK " x)

//power
//|> filter    ( fun x -> x.Contains("Earth") ) 
//|> filter    ( fun x -> x.Contains("Fire")  ) 
//|> filter    ( fun x -> x.Contains("Wind")  ) 
//|> filter    ( fun x -> x.Contains("Water") ) 
//|> subscribe ( fun x -> printfn "Fuck Heart, Captain Murphy is BACK ")
//let combinedStream = merge timerEventStream1 timerEventStream2


;;
//[ Timer4; Timer3; Timer2; Timer1 ]
Timer1
//|> Async.Parallel
|> Async.RunSynchronously
