namespace Ehingeeinae.Ecs.Hosting

open System
open System.Runtime.InteropServices
open System.Threading
open System.Threading.Tasks
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Scheduling
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


// type EcsSchedulerBuilder(services: IServiceCollection) =
//
//     let nextGroupId =
//         let mutable lastGroupId = 0uL
//         fun () ->
//             let gid = lastGroupId
//             lastGroupId <- lastGroupId + 1uL
//             gid
//
//     let createSchedulerSystem (groupInfo: SystemLoop) (systemFactory: IEcsSystemFactory) (queryFactory: IEcsQueryFactory) : SchedulerSystem =
//         let compTypes: ResizeArray<Type * bool> = ResizeArray()
//         let mutable factoryExecuted = false
//         let queryFactory' = { new IEcsQueryFactory with
//             member _.CreateQuery<'q>() =
//                 // Guard
//                 if factoryExecuted then invalidOp "Cannot call query factory after system factory is executed"
//
//                 let query = queryFactory.CreateQuery<'q>()
//                 let compTypes' = QueryArgument.ofShape shapeof<'q> |> QueryArgument.getCompTypes
//                 compTypes.AddRange(compTypes')
//                 query
//         }
//         let system = systemFactory.CreateSystem(queryFactory')
//         factoryExecuted <- true
//         let schedulerSystem =
//             let usingComponents = compTypes |> Seq.map (fun (compType, isMutable) -> { Type = compType; IsMutable = isMutable }) |> Seq.toList
//             let conflictSystems
//             { System = system
//               ConflictSystems = conflictSystems
//               SystemLoop = groupInfo }
//         schedulerSystem
//
//     member this.CreateGroup(name, threadingFactory: IServiceProvider -> Threading) =
//         let groupId = SystemLoopId <| nextGroupId ()
//         fun sp -> { Id = groupId; Name = name; Threading = threadingFactory sp }
//
//     member this.AddSystem(groupInfoFactory: IServiceProvider -> SystemLoop, systemFactoryFactory: IServiceProvider -> IEcsSystemFactory): unit =
//         services
//             .AddSingleton<IEcsSystemFactory>(systemFactoryFactory)
//             .AddSingleton<SchedulerSystem>(fun services ->
//                 let systemFactory = systemFactoryFactory services
//                 let queryFactory = services.GetRequiredService<IEcsQueryFactory>()
//                 let schedulerSystem = createSchedulerSystem (groupInfoFactory services) systemFactory queryFactory
//                 schedulerSystem
//             )
//         |> ignore
//
//     member this.AddSystem(groupInfo: SystemLoop, systemFactoryFactory) = this.AddSystem((fun _ -> groupInfo), systemFactoryFactory)
//
//     [<RequiresExplicitTypeArguments>]
//     member this.AddSystem<'TSystemFactory when 'TSystemFactory :> IEcsSystemFactory and 'TSystemFactory : not struct>
//             (groupInfoFactory: IServiceProvider -> SystemLoop) =
//         services
//             .AddSingleton<'TSystemFactory>()
//             .AddSingleton<IEcsSystemFactory, 'TSystemFactory>()
//             .AddSingleton<SchedulerSystem>(fun services ->
//                 let queryFactory = services.GetRequiredService<IEcsQueryFactory>()
//                 let systemFactory = services.GetRequiredService<'TSystemFactory>()
//                 let schedulerSystem = createSchedulerSystem (groupInfoFactory services) systemFactory queryFactory
//                 schedulerSystem
//             )
//         |> ignore
//
//     member this.AddSystem<'TSystemFactory when 'TSystemFactory :> IEcsSystemFactory and 'TSystemFactory : not struct>
//         (groupInfo: SystemLoop) = this.AddSystem<'TSystemFactory>(fun _ -> groupInfo)
//
//     member this.AddTiming(groupInfoFactory: IServiceProvider -> SystemLoop, interval) =
//         services.AddSingleton<IntervalGroup>(fun services ->
//             let groupInfo = groupInfoFactory services
//             { GroupId = groupInfo.Id; IntervalMs = interval }
//         ) |> ignore
//
//     member this.AddSeeder(seedWorld) =
//         let seeder = { new IEcsWorldSeeder with member _.Seed(em) = seedWorld em }
//         services.AddSingleton<IEcsWorldSeeder>(seeder) |> ignore


[<AutoOpen>]
module EcsServiceCollectionExtensions =

    open Ehingeeinae.Ecs.Querying.RuntimeCompilation
    open Ehingeeinae.Ecs.Scheduling.SystemChainBuilding

    // type IServiceCollection with
    //     member this.AddEcs(configureEcs: EcsSchedulerBuilder -> unit) =
    //         this.AddSingleton<IEcsQueryFactory, CompiledEcsQueryFactory>(fun services ->
    //             CompiledEcsQueryFactory()
    //         ) |> ignore
    //         this.AddSingleton<TimedScheduler>() |> ignore
    //         this.AddSingleton<SystemLoopUpdater>() |> ignore
    //
    //         this
    //             .AddSingleton<_, _>(fun _ -> EcsWorld.createEmpty ())
    //             .AddSingleton<IEcsWorldEntityManager, EcsWorldEntityManager>()
    //             .AddSingleton<EcsWorldQueryExecutor>()
    //         |> ignore
    //
    //         let ecsBuilder = EcsSchedulerBuilder(this)
    //         configureEcs ecsBuilder
    //
    //         this.AddHostedService<EcsHostedService>() |> ignore
    //
    //         this

    type IServiceCollection with
        member this.AddSystemChain(configure: IServiceProvider -> SystemChainBuilder -> unit) =
            this
                .AddSingleton<_, _>(fun _ -> EcsWorld.createEmpty ())
                .AddSingleton<IEcsWorldEntityManager, EcsWorldEntityManager>()
                .AddSingleton<EcsWorldQueryExecutor>()
            |> ignore
            this.AddSingleton<IEcsQueryFactory, CompiledEcsQueryFactory>(fun _ -> CompiledEcsQueryFactory()) |> ignore
            this.AddSingleton<SystemChain, SystemChain>(fun services ->
                let queryFactory = services.GetRequiredService<IEcsQueryFactory>()
                let builder = SystemChainBuilder(queryFactory)
                configure services builder
                let chain = builder.Build()
                chain
            ) |> ignore
            this.AddSingleton<TimedScheduler>() |> ignore
            this.AddSingleton<SystemLoopUpdater>() |> ignore
            this.AddHostedService<EcsHostedService>()

(*

let f = services.AddRendering(fun rendering ->
    ()
)

services.AddEcs(fun (ecs: IEcsBuilder) f ->
    let groupRender = ecs.CreateGroup("render", Threading.Thread f.Sender)
    let groupLogic = ecs.CreateGroup("logic", Threading.ThreadPool)

    ecs.AddSystem(groupRender, blockRenderSystemFactory)
    ecs.AddSystem(groupRender, entityRenderSystemFactory)

    ecs.AddSystem(groupLogic, entityMovementSystemFactory)
    ecs.AddSystem(groupLogic, entityPhysicsSystemFactory)
    ecs.AddSystem(groupLogic, chunkLoadingSystemFactory)
    ecs.AddSystem(groupLogic, blockUpdateSystemFactory)

    ecs.AddTiming(groupRender, 1000L / 60L)
    ecs.AddTiming(groupLogic, 1000L / 20L)
)

*)
