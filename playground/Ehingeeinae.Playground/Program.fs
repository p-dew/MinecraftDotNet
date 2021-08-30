module Ehingeeinae.Playground.Program

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

type IEcsQueryFactory =
    abstract CreateQuery<'q> : unit -> IEcsQuery<'q>

type IEcsSystem =
    abstract Update: unit -> unit

module Example2 =

    [<Struct>]
    type Position =
        { Position: Vector3 }

    [<Struct>]
    type Player = struct end

    [<Struct>]
    type Entity = struct end

    [<Struct>]
    type ToUnload = struct end

    type FooSystem(entityManager: IEcsWorldEntityManager, queryExecutor: EcsWorldQueryExecutor) =
        interface IEcsSystem with
            member this.Update() =
                let criticalLength = 10.f
                let players = EcsQuery.query<Position cread * Player cread> |> queryExecutor.ExecuteQuery
                let entities = EcsQuery.query<EcsEntityId * Position cread * Entity cread> |> queryExecutor.ExecuteQuery
                for eid, entityPosition, _ in entities do
                    let isEnoughFarForUnload =
                        players
                        |> Seq.map fst
                        |> Seq.exists ^fun playerPosition ->
                            Vector3.Distance(playerPosition.Value.Position, entityPosition.Value.Position) > criticalLength
                    if isEnoughFarForUnload then
                        entityManager.AddComponent(eid, ToUnload())

[<Struct>]
type Position =
    { Position: Vector2 }

[<Struct>]
type Velocity =
    { Velocity: Vector2 }

[<Struct>]
type StaticBody = struct end

[<Struct>]
type DecimalX10 = { D0: decimal; D1: decimal; D2: decimal; D3: decimal; D4: decimal; D5: decimal; D6: decimal; D7: decimal; D8: decimal; D9: decimal }

[<Struct>]
type LargeComponent =
    { DX0: DecimalX10; DX1: DecimalX10; DX2: DecimalX10; DX3: DecimalX10 }


let test (logger: ILogger) (entityManager: IEcsWorldEntityManager) =
    for i in 0 .. 999999 do
        if i % 1000 = 0 then logger.LogInformation($"i: {i}")
        entityManager.AddEntity(({ Position = Vector2(2f, 2f) }, { Velocity = Vector2(1f, 1f) }, Unchecked.defaultof<LargeComponent>)) |> ignore
        (entityManager :?> EcsWorldEntityManager).Commit()

[<AutoOpen>]
module Absorb =
    let mutable private _void: obj = null
    let absorb (x: 'a) = _void <- box x

let work (services: IServiceProvider) =
    let logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("work")

    let world = EcsWorld.createEmpty ()
    let worldEntityManager = EcsWorldEntityManager(world, services.GetRequiredService())
    let worldQueryExecutor = EcsWorldQueryExecutor(world)

    logger.LogInformation($"World init: %A{world}")

    (worldEntityManager :> IEcsWorldEntityManager).AddEntities([
        for i in 0 .. 30 do
            let i = float32 i
            ({ Position = Vector2( 2f * i,  2f) }, { Velocity = Vector2( 1f,  1f) })
            ({ Position = Vector2(-2f, -2f * i) }, { Velocity = Vector2(-1f, -1f) })
    ]) |> ignore
    let eid = (worldEntityManager :> IEcsWorldEntityManager).AddEntity({ Position = Vector2( 2f,  2f) }, { Velocity = Vector2(-1f, -1f) }, StaticBody())
    (worldEntityManager :> IEcsWorldEntityManager).RemoveEntity(eid)

    worldEntityManager.Commit()

    logger.LogInformation($"World seeded: %A{world}")

    let q = EcsQuery.query<Position cwrite * Velocity cread> |> EcsQuery.withFilter (-EcsQueryFilter.comp<StaticBody>)

    for i in 0 .. 99999 do
        if i % 10 = 0 then logger.LogInformation($"i: {i}")
        let comps = worldQueryExecutor.ExecuteQuery(q)
        for position, velocity in comps do
            let newPosition = { Position = position.Value.Position + velocity.Value.Velocity}
            EcsWriteComponent.setValue position &newPosition

    // let q = EcsQuery.query<Position cread * Velocity cread> |> EcsQuery.withFilter (-EcsQueryFilter.comp<StaticBody>)
    // for i in 0 .. 99 do
    //     if i % 10 = 0 then logger.LogInformation($"i: {i}")
    //     let comps = worldQueryExecutor.ExecuteQuery(q)
    //     for position, velocity in comps do
    //         let newPosition = { Position = position.Value.Position + velocity.Value.Velocity}
    //         absorb newPosition

    logger.LogInformation($"World result: %A{world}")

    ()


let configureServices (services: IServiceCollection) : unit =
    services.AddLogging(fun logging -> logging.AddConsole() |> ignore) |> ignore
    ()

// ----

type Worker(services, lifetime, loggerFactory: ILoggerFactory) =
    inherit SingleWorkHostedService(async { do work services }, lifetime, loggerFactory.CreateLogger<_>())

let createHostBuilder args =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(fun services ->
            services.AddHostedService<Worker>() |> ignore
            ()
        )
        .ConfigureServices(configureServices)

[<EntryPoint>]
let main args =
    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
    (createHostBuilder args).Build().Run()
    0
