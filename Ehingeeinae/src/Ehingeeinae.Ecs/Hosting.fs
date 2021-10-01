namespace Ehingeeinae.Ecs.Hosting

open System
open System.Runtime.InteropServices
open System.Threading
open System.Threading.Tasks
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Scheduling
open Ehingeeinae.Ecs.Scheduling.SystemChainBuilding
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Ecs.Worlds
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FSharp.Control.Tasks
open Microsoft.Extensions.Logging
open TypeShape.Core


type IEcsWorldSeeder =
    abstract Seed: entityManager: IEcsWorldEntityManager -> unit

type EcsHostedService(logger: ILogger<EcsHostedService>, runner: TimedScheduler, worldEntityManager: IEcsWorldEntityManager, seeder: IEcsWorldSeeder) =
    let mutable schedulerCts: CancellationTokenSource = null
    let mutable runningTask = null

    let runAsync () = unitTask {
        try
            do! Task.Run(fun () ->
                // seeder |> Option.iter (fun seeder -> seeder.Seed(worldEntityManager))
                seeder.Seed(worldEntityManager)
                (worldEntityManager :?> EcsWorldEntityManager).Commit() // FIXME: Remove downcast ugly hack
                runner.Run(schedulerCts.Token)
            )
        with e ->
            logger.LogError(e, "Error while running scheduler")
    }

    interface IHostedService with
        member this.StartAsync(cancellationToken) =
            let cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            schedulerCts <- cts
            runningTask <- runAsync ()
            Task.CompletedTask
        member this.StopAsync(cancellationToken) =
            schedulerCts.Cancel()
            Task.CompletedTask

type EcsResourceRegistrationBuilder(resourceProvider: IEcsResourceProvider) =
    member this.Register<'T>(initialValue: 'T): unit =
        resourceProvider.RegisterResource<'T>(initialValue)

type EcsConfigurationContext =
    { SystemChain: SystemChainBuilder
      Resources: EcsResourceRegistrationBuilder }

[<AutoOpen>]
module EcsServiceCollectionExtensions =

    open Ehingeeinae.Ecs.Querying.RuntimeCompilation

    type IServiceCollection with
        member this.AddEcs(configureEcs: IServiceProvider -> EcsConfigurationContext -> unit) =
            this
                .AddSingleton<EcsWorld>(fun _ -> EcsWorld.createEmpty ())
                .AddSingleton<IEcsWorldEntityManager, EcsWorldEntityManager>()
                .AddSingleton<EcsWorldQueryExecutor>()
            |> ignore

            this.AddSingleton<IEcsQueryFactory, CompiledEcsQueryFactory>(fun _ -> CompiledEcsQueryFactory()) |> ignore
            this.AddSingleton<IEcsResourceProvider, ResourceStorage>(fun _ -> ResourceStorage()) |> ignore

            this.AddSingleton<SystemChain, SystemChain>(fun services ->
                let resourceProvider = services.GetRequiredService<IEcsResourceProvider>()
                let systemChainBuilder =
                    let queryFactory = services.GetRequiredService<IEcsQueryFactory>()
                    SystemChainBuilder(queryFactory, resourceProvider)
                let resourceRegistrationBuilder = EcsResourceRegistrationBuilder(resourceProvider)
                let context =
                    { SystemChain = systemChainBuilder
                      Resources = resourceRegistrationBuilder }
                configureEcs services context
                let chain = systemChainBuilder.Build()
                chain
            ) |> ignore

            this.AddSingleton<TimedScheduler>() |> ignore
            this.AddSingleton<SystemLoopUpdater>() |> ignore
            this.AddHostedService<EcsHostedService>()
