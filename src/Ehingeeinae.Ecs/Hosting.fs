namespace Ehingeeinae.Ecs.Hosting

//open System.Threading
//open System.Threading.Tasks
//open Ehingeeinae.Ecs.Scheduling
//open Microsoft.Extensions.Hosting
//
//type EcsHostedService(scheduler: EcsScheduler) =
//    let mutable schedulerCts = None
//    interface IHostedService with
//        member this.StartAsync(cancellationToken) =
//            let cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
//            schedulerCts <- Some cts
//            let schedulerRunComputation = scheduler.AsyncRun()
//            Async.Start(schedulerRunComputation, cts.Token)
//            Task.CompletedTask
//        member this.StopAsync(cancellationToken) =
//            schedulerCts |> Option.iter (fun cts -> cts.Cancel())
//            Task.CompletedTask
