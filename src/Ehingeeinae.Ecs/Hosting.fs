namespace Ehingeeinae.Ecs.Hosting

open System
open System.Threading
open System.Threading.Tasks
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Scheduling
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Ecs.Worlds
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FSharp.Control.Tasks
open TypeShape.Core

// type _Void<'T> =
//     [<DefaultValue>] static val mutable private Value: 'T
//     static member Absorb(x: 'T) = _Void<_>.Value <- x

type IEcsWorldSeeder =
    abstract Seed: entityManager: IEcsWorldEntityManager -> unit

type EcsHostedService(runner: TimedScheduler, seeder: IEcsWorldSeeder, worldEntityManager: IEcsWorldEntityManager) =
    let mutable schedulerCts: CancellationTokenSource = null
    let mutable runningTask = null

    let runAsync () =
        Task.Run(fun () ->
            seeder.Seed(worldEntityManager)
            (worldEntityManager :?> EcsWorldEntityManager).Commit() // FIXME: Remove downcast ugly hack
            runner.Run(schedulerCts.Token)
        )

    interface IHostedService with
        member this.StartAsync(cancellationToken) =
            let cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            schedulerCts <- cts
            runningTask <- runAsync ()
            Task.CompletedTask
        member this.StopAsync(cancellationToken) =
            schedulerCts.Cancel()
            Task.CompletedTask


type IEcsSystemFactory =
    abstract CreateSystem: queryFactory: IEcsQueryFactory -> IEcsSystem

module EcsSystemFactory =
    let inline create createSystem = { new IEcsSystemFactory with member _.CreateSystem(qf) = createSystem qf }

type EcsSchedulerBuilder(services: IServiceCollection) =

    let createSchedulerSystem (groupInfo: GroupInfo) (systemFactory: IEcsSystemFactory) (queryFactory: IEcsQueryFactory) : SchedulerSystem =
        let compTypes: ResizeArray<Type * bool> = ResizeArray()
        let mutable factoryExecuted = false
        let queryFactory' = { new IEcsQueryFactory with
            member _.CreateQuery<'q>() =
                // Guard
                if factoryExecuted then invalidOp "Cannot call query factory after system factory is executed"

                let query = queryFactory.CreateQuery<'q>()
                let compTypes' = QueryArgument.ofShape shapeof<'q> |> QueryArgument.getCompTypes
                compTypes.AddRange(compTypes')
                query
        }
        let system = systemFactory.CreateSystem(queryFactory')
        factoryExecuted <- true
        let schedulerSystem =
            let usingComponents = compTypes |> Seq.map (fun (compType, isMutable) -> { Type = compType; IsMutable = isMutable }) |> Seq.toList
            { System = system
              UsingComponents = Some usingComponents
              GroupInfo = groupInfo }
        schedulerSystem

    member this.CreateGroup(name, threading) =
        let groupId = GroupId <| Guid.NewGuid()
        { Id = groupId; Name = name; Threading = threading }

    member this.AddSystem(groupInfo: GroupInfo, systemFactoryFactory: IServiceProvider -> IEcsSystemFactory): unit =
        services
            .AddSingleton<IEcsSystemFactory>(systemFactoryFactory)
            .AddSingleton<SchedulerSystem>(fun services ->
                let systemFactory = systemFactoryFactory services
                let queryFactory = services.GetRequiredService<IEcsQueryFactory>()
                let schedulerSystem = createSchedulerSystem groupInfo systemFactory queryFactory
                schedulerSystem
            )
        |> ignore

    [<RequiresExplicitTypeArguments>]
    member this.AddSystem<'TSystemFactory when 'TSystemFactory :> IEcsSystemFactory and 'TSystemFactory : not struct>
            (groupInfo: GroupInfo) =
        services
            .AddSingleton<'TSystemFactory>()
            .AddSingleton<IEcsSystemFactory, 'TSystemFactory>()
            .AddSingleton<SchedulerSystem>(fun services ->
                let queryFactory = services.GetRequiredService<IEcsQueryFactory>()
                let systemFactory = services.GetRequiredService<'TSystemFactory>()
                let schedulerSystem = createSchedulerSystem groupInfo systemFactory queryFactory
                schedulerSystem
            )
        |> ignore

    member this.AddTiming(groupInfo: GroupInfo, interval) =
        let intervalGroup = { GroupId = groupInfo.Id; IntervalMs = interval }
        services.AddSingleton(intervalGroup) |> ignore

    member this.AddSeeder(seedWorld) =
        let seeder = { new IEcsWorldSeeder with member _.Seed(em) = seedWorld em }
        services.AddSingleton<IEcsWorldSeeder>(seeder) |> ignore


[<AutoOpen>]
module EcsServiceCollectionExtensions =

    type IServiceCollection with
        member this.AddEcs(configureEcs: EcsSchedulerBuilder -> unit) =
            this.AddSingleton<IEcsQueryFactory, CompiledEcsQueryFactory>(fun services ->
                CompiledEcsQueryFactory()
            ) |> ignore
            this.AddSingleton<TimedScheduler>() |> ignore
            this.AddSingleton<SystemGroupUpdater>() |> ignore

            this
                .AddSingleton<_, _>(fun _ -> EcsWorld.createEmpty ())
                .AddSingleton<IEcsWorldEntityManager, EcsWorldEntityManager>()
                .AddSingleton<EcsWorldQueryExecutor>()
            |> ignore

            let ecsBuilder = EcsSchedulerBuilder(this)
            configureEcs ecsBuilder

            this.AddHostedService<EcsHostedService>() |> ignore

            this

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
