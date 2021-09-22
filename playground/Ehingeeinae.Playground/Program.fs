module Ehingeeinae.Playground.Program

open System.Diagnostics
open System.Threading
open Ehingeeinae.Ecs.Hosting
open Ehingeeinae.Ecs.Scheduling
open Ehingeeinae.Ecs.Systems

#nowarn "9"

open System
open System.Numerics
open System.Threading.Tasks
open System.Collections.Generic
open System.Runtime.CompilerServices

open FSharp.NativeInterop

open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Playground


let inline ( ^ ) f x = f x

// module Example2 =
//
//     open Ehingeeinae.Ecs.Systems
//
//     [<Struct>]
//     type Position =
//         { Position: Vector3 }
//
//     [<Struct>]
//     type Player = struct end
//
//     [<Struct>]
//     type Entity = struct end
//
//     [<Struct>]
//     type ToUnload = struct end
//
//     type FooSystem(entityManager: IEcsWorldEntityManager, queryExecutor: EcsWorldQueryExecutor) =
//         interface IEcsSystem with
//             member this.Update(ctx) =
//                 let criticalLength = 10.f
//                 let players = EcsQuery.query<Position cread * Player cread> |> queryExecutor.ExecuteQuery
//                 let entities = EcsQuery.query<EcsEntityId * Position cread * Entity cread> |> queryExecutor.ExecuteQuery
//                 for eid, entityPosition, _ in entities do
//                     let isEnoughFarForUnload =
//                         players
//                         |> Seq.map fst
//                         |> Seq.exists ^fun playerPosition ->
//                             Vector3.Distance(playerPosition.Value.Position, entityPosition.Value.Position) > criticalLength
//                     if isEnoughFarForUnload then
//                         entityManager.AddComponent(eid, ToUnload())

[<Struct>]
type Position =
    { Position: Vector2 }

[<Struct>]
type Velocity =
    { Velocity: Vector2 }

[<Struct>]
type Named = { Name: string }

[<Struct>]
type StaticBody = struct end

// ----

type MovementSystemFactory(queryExecutor: EcsWorldQueryExecutor) =
    interface IEcsSystemFactory with
        member _.CreateSystem(queryFactory) =
            let query = queryFactory.CreateQuery<{| Position: Position cwrite; Velocity: Velocity cread |}>()
            EcsSystem.create ^fun ctx ->
                let comps = queryExecutor.ExecuteQuery(query)
                for comp in comps do
                    comp.Position.Value <- { Position = comp.Position.Value.Position + comp.Velocity.Value.Velocity}

// type MovementSystem2(query: IEcsQuery<Position cwrite>, queryExecutor: EcsWorldQueryExecutor) =
//     interface IEcsSystem with
//         member this.Update(ctx) =
//             let comps = queryExecutor.ExecuteQuery(query)
//             for position in comps do
//                 position.Value <- { Position = position.Value.Position - Vector2.UnitY }
//
// type PrintingSystem(query: IEcsQuery<Position cread * Named cread>, queryExecutor: EcsWorldQueryExecutor) =
//     interface IEcsSystem with
//         member this.Update(ctx) =
//             printfn "PrintSystem.Update"
//             let comps = queryExecutor.ExecuteQuery(query)
//             for pos, named in comps do
//                 let pos = pos.Value.Position
//                 let name = named.Value.Name
//                 printf $"{name}({pos.X}, {pos.Y}) "
//             printfn ""

let printingSystemFactory (services: IServiceProvider) =
    EcsSystemFactory.create ^fun queryFactory ->
        let query = queryFactory.CreateQuery<Position cread * Named cread>()
        let queryExecutor = services.GetRequiredService<EcsWorldQueryExecutor>()
        let stopwatch = Stopwatch()
        stopwatch.Start()
        EcsSystem.create ^fun ctx ->
            printfn $"PrintSystem.Update; Last update: {stopwatch.Elapsed}"
            stopwatch.Restart()
            let comps = queryExecutor.ExecuteQuery(query)
            for pos, named in comps do
                let pos = pos.Value.Position
                let name = named.Value.Name
                printf $"{name}({pos.X}, {pos.Y}) "
            printfn ""


// ----

let seedWorld (worldManager: IEcsWorldEntityManager) : unit =
    worldManager.AddEntities([
        for i in 0 .. 2 do
            let i = float32 i
            ({ Position = Vector2( 2f * i,  2f) }, { Velocity = Vector2( 1f,  1f) }, { Name = $"A{i}" })
            ({ Position = Vector2(-2f, -2f * i) }, { Velocity = Vector2(-1f, -1f) }, { Name = $"B{i}" })
    ]) |> ignore
    let eid = worldManager.AddEntity({ Position = Vector2( 2f,  2f) }, { Velocity = Vector2(-1f, -1f) }, StaticBody())
    // (worldEntityManager :> IEcsWorldEntityManager).RemoveEntity(eid)
    ()

// let work (services: IServiceProvider) =
//     let logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("work")
//
//     let world = EcsWorld.createEmpty ()
//     let worldEntityManager = EcsWorldEntityManager(world, services.GetRequiredService())
//     let worldQueryExecutor = EcsWorldQueryExecutor(world)
//
//     // ----
//     // Systems
//
//     let systems =
//         SystemGroupBuilder()
//             .AddSystem("Logic", fun q -> upcast MovementSystem(q.CreateQuery(), worldQueryExecutor))
//             .AddSystem("Logic", fun q -> upcast MovementSystem2(q.CreateQuery(), worldQueryExecutor))
//             .AddSystem("Render", fun q -> upcast PrintingSystem(q.CreateQuery(), worldQueryExecutor))
//             .Build()
//
//     let scheduler = SystemGroupUpdater(systems)
//
//     // ----
//
//     logger.LogInformation($"World init: %A{world}")
//
//     seedWorld worldEntityManager
//     worldEntityManager.Commit()
//
//     logger.LogInformation($"World seeded: %A{world}")
//
//     scheduler.UpdateGroups([ "Logic"; "Render" ])
//
//     logger.LogInformation($"World result: %A{world}")
//
//     ()

// let g1s1 (services: IServiceProvider) =
//     EcsSystemFactory.create (fun queryFactory ->
//         let q = queryFactory.CreateQuery()
//         ()
//     )


// let configureEcs (ecs: EcsSchedulerBuilder) : unit =
//     let renderGroup = ecs.CreateGroup("render", Threading.ThreadPool)
//     let logicGroup = ecs.CreateGroup("logic", Threading.ThreadPool)
//     ecs.AddSystem<MovementSystemFactory>(logicGroup)
//     ecs.AddSystem(renderGroup, printingSystemFactory)
//     ecs.AddTiming(renderGroup, 500.f)
//     ecs.AddTiming(logicGroup, 1_000.f)
//     ecs.AddSeeder(seedWorld)
//
//
// let configureServices (services: IServiceCollection) : unit =
//     ()
//
// // ----
//
// let createHostBuilder args =
//     Host.CreateDefaultBuilder(args)
//         .ConfigureLogging(fun logging ->
//             logging.AddConsole() |> ignore
//         )
//         .ConfigureServices(fun services ->
//             services.AddEcs(configureEcs) |> ignore
//         )
//         .ConfigureServices(configureServices)
//         .ConfigureServices(fun services -> services.AddSingleton(services) |> ignore)

[<EntryPoint>]
let main args =
    // Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
    // let host = (createHostBuilder args).Build()
    // // let services = host.Services.GetRequiredService<IServiceCollection>() |> Seq.toArray
    // // printfn $">>> services:\n%A{services}\n<<<"
    // host.Run()

    let expr = Ehingeeinae.Ecs.Experimental.Storage.Shaping.mkAddEntities<int * string * float> (Unchecked.defaultof<_>)
    let exprStr = ExprToCode.ExprDisplay.display expr

    printfn $"<<<<\n{exprStr}\n>>>>"

    0
