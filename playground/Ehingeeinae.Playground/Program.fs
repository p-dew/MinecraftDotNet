module Ehingeeinae.Playground.Program

open System.Diagnostics
open System.Drawing
open System.Threading
open Ehingeeinae.Ecs.Hosting
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Scheduling
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Graphics.Hosting
open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.Desktop

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
            // let deltaTime = resourceProvider.Get<EcsReadResource<DeltaTime>>()
            EcsSystem.create ^fun ctx ->
                let comps = queryExecutor.ExecuteQuery(query)
                for comp in comps do
                    comp.Position.Value <- { Position = comp.Position.Value.Position + comp.Velocity.Value.Velocity}

type MovementSystemFactory2(queryExecutor: EcsWorldQueryExecutor, query: IEcsQuery<{| Position: Position cwrite; Velocity: Velocity cread |}>) =
    interface IEcsSystem with
        member this.Update(ctx) =
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

            GL.Color3(1., 0., 0.)
            GL.LineWidth(3f)
            GL.Begin(BeginMode.Lines)
            GL.Vertex2(-1., -1.)
            GL.Vertex2(1., 1.)
            GL.End()

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


let configureEcs (ecs: EcsSchedulerBuilder) : unit =
    let renderGroup =
        ecs.CreateGroup(
            "render",
            fun services ->
                let sender = services.GetRequiredService<IGraphicsCommandSender>()
                Threading.Command sender.Send
        )
    let logicGroup = ecs.CreateGroup("logic", fun _ -> Threading.ThreadPool)

    ecs.AddSystem<MovementSystemFactory>(logicGroup)
    ecs.AddSystem(renderGroup, printingSystemFactory)

    ecs.AddTiming(renderGroup, 500.f)
    ecs.AddTiming(logicGroup, 1_000.f)

    ecs.AddSeeder(seedWorld)
    ()


let configureServices (services: IServiceCollection) : unit =
    ()

// ----



let createHostBuilder args =
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(fun logging ->
            logging.AddConsole() |> ignore
        )
        .ConfigureServices(fun services ->
            services.AddEcs(configureEcs) |> ignore
            services.AddGraphics(
                GameWindowSettings(RenderFrequency=5., UpdateFrequency=0.),
                NativeWindowSettings(Title="Playground", Size=Vector2i(1280, 720), Profile=ContextProfile.Compatability)
            ) |> ignore
        )
        .ConfigureServices(configureServices)
        .ConfigureServices(fun services -> services.AddSingleton(services) |> ignore)

        // .UseDefaultServiceProvider()
        // .UseServiceProviderFactory(fun ctx ->
        //
        //     null
        // )

[<EntryPoint>]
let main args =
    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
    let host = (createHostBuilder args).Build()
    // let services = host.Services.GetRequiredService<IServiceCollection>() |> Seq.toArray
    // printfn $">>> services:\n%A{services}\n<<<"
    host.Run()
    0
