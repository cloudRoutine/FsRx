#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\FSharp.Control.Observable.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.Core.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.Interfaces.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.Linq.dll"
#r @"J:\J hester\Programming Projects\FsRx\FsRx\bin\Release\System.Reactive.PlatformServices.dll"
#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.Collections"

open FSharp.Control.Observable
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


let basicTimer2 , timerEventStream = createTimerAndObservable 1000 10000


timerEventStream 
|> subscribe ( fun _ -> printf " tick %A " DateTime.Now )

timerEventStream 
|> filter    ( fun x -> 0 = x.SignalTime.Second % 3)
|> subscribe ( fun _ -> printf " filtered %A " DateTime.Now)


timerEventStream 
|> map       ( fun x -> x.SignalTime.Second * 1000 )
|> map       ( fun x -> "I put " + x.ToString() + " in a string " )
|> subscribe ( fun x -> printf "%A" x )

timerEventStream 
|> subscribe ( fun x -> printf "\n" )


;;
Async.RunSynchronously basicTimer2