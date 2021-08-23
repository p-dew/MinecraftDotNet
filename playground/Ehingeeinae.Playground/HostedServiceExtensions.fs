[<AutoOpen>]
module Ehingeeinae.Playground.HostedServiceExtensions

open System
open System.Threading.Tasks

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging


type HostedServiceFactory<'THostedService>
    when 'THostedService :> IHostedService and 'THostedService : not struct
    = IServiceProvider -> 'THostedService

type WorkerHostedService(work: Async<unit>) =
    inherit BackgroundService()
    override this.ExecuteAsync(ct) = Async.StartAsTask(work, cancellationToken=ct) :> Task

type SingleWorkHostedService(work: Async<unit>, lifetime: IHostApplicationLifetime, logger: ILogger<SingleWorkHostedService>) =
    inherit BackgroundService()
    let work' = async {
        try
            try
                do! work
            with e ->
                logger.LogError(e, "")
        finally
            do lifetime.StopApplication()
    }
    override this.ExecuteAsync(ct) =
        Async.StartAsTask(work', cancellationToken=ct) :> Task

module HostedService =
    ()
//    let createOfAsync (asyncStart: Async<unit>) (asyncStop: Async<unit>) : HostedServiceFactory =
//        fun services ->
//            { new IHostedService with
//                member this.StartAsync(ct) = Async.StartAsTask(asyncStart, cancellationToken=ct) :> Task
//                member this.StopAsync(ct) = Async.StartAsTask(asyncStop, cancellationToken=ct) :> Task }



//    let worker (work: Async<unit>) : HostedServiceFactory =
//        fun services ->
//            upcast { new BackgroundService() with
//                override this.ExecuteAsync(ct) = Async.StartAsTask(work, cancellationToken=ct) :> Task
//            }
//
//    let singleWork (work: Async<unit>) (onFinished: unit -> unit) : HostedServiceFactory =
//        let work' = async {
//            try
//                do! work
//            finally
//                do onFinished ()
//        }
//        worker work'
//
//    let singleWorkAndShutdown (work: Async<unit>) : HostedServiceFactory =
//        fun services ->
//            let lifetime = services.GetRequiredService<IHostApplicationLifetime>()
//            (singleWork work lifetime.StopApplication) services
