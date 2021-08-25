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

    let foo (entityManager: IEcsWorldEntityManager) (queryExecutor: EcsWorldQueryExecutor) =
        let q = EcsQuery.query<EcsReadComponent<Position> * EcsReadComponent<Player>>
        let players = queryExecutor.ExecuteQuery(q)
        for playerPosition, _ in players do

            ()
        ()

    ()

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

let work (services: IServiceProvider) =
    let logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("work")

    let world = EcsWorld.createEmpty ()
    let worldEntityManager = EcsWorldEntityManager(world, services.GetRequiredService())
    let worldQueryExecutor = EcsWorldQueryExecutor(world)

    logger.LogInformation($"World init: %A{world}")

    (worldEntityManager :> IEcsWorldEntityManager).AddEntities([
        ({ Position = Vector2( 2f,  2f) }, { Velocity = Vector2( 1f,  1f) })
        ({ Position = Vector2(-2f, -2f) }, { Velocity = Vector2(-1f, -1f) })
    ]) |> ignore
    let eid = (worldEntityManager :> IEcsWorldEntityManager).AddEntity({ Position = Vector2( 2f,  2f) }, { Velocity = Vector2(-1f, -1f) }, StaticBody())
    (worldEntityManager :> IEcsWorldEntityManager).RemoveEntity(eid)

    worldEntityManager.Commit()

    logger.LogInformation($"World seeded: %A{world}")

    let q = EcsQuery.query<struct(EcsWriteComponent<Position> * EcsReadComponent<Velocity>)> |> EcsQuery.withFilter (-EcsQueryFilter.comp<StaticBody>)
    let comps = worldQueryExecutor.ExecuteQuery(q)
    for position, velocity in comps do
        let newPosition = { Position = position.Value.Position + velocity.Value.Velocity}
        EcsWriteComponent.setValue position &newPosition

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
