module Ehingeeinae.Ecs.Benchmarks.Benchmarks.QueryingBenchs

open System
open BenchmarkDotNet
open BenchmarkDotNet.Engines
open BenchmarkDotNet.Attributes
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Querying.RuntimeCompilation
open Ehingeeinae.Ecs.Worlds
open Ehingeeinae.Ecs.Benchmarks
open Microsoft.Extensions.Logging.Abstractions


type QueryingBenchmark() =
    let consumer = Consumer()

    let world = EcsWorld.createEmpty ()

    let seedWorld (worldManager: IEcsWorldEntityManager) entityCount =
        let entityCount = entityCount - 1
        worldManager.AddEntities([ for i in 0 .. entityCount do { I1 = i } ]) |> ignore
        worldManager.AddEntities([ for i in 0 .. entityCount do { I1 = i; S1 = string i } ]) |> ignore
        worldManager.AddEntities([ for i in 0 .. entityCount do { I1 = i; S1 = string i }, { I1 = i * 2 }, { S1 = string i } ]) |> ignore
        worldManager.AddEntities([ for i in 0 .. entityCount do { I1 = i }, { G1 = Guid.NewGuid() }, { F1 = float i }, { S1 = string i } ]) |> ignore
        worldManager.AddEntities([ for i in 0 .. entityCount do Comp8D.Create(decimal i) ]) |> ignore

    let q = EcsQueryCreating.mkQuery<Comp1I cread * Comp1S cread> ()

    let queryExecutor = EcsWorldQueryExecutor(world)
    let worldManager = EcsWorldEntityManager(world, NullLogger<_>.Instance)

    [<Params(30, 1200)>]
    member val EntityCount = 0 with get, set
    [<Params(5, 10000)>]
    member val QueryCount = 0 with get, set

    [<GlobalSetup>]
    member this.GlobalSetup() =
        seedWorld worldManager this.EntityCount
        worldManager.Commit()

    [<GlobalCleanup>]
    member this.GlobalCleanup() =
        worldManager.Clear()

    [<Benchmark>]
    member this.Querying1() =
        for i in 0 .. this.QueryCount - 1 do
            let comps = queryExecutor.ExecuteQuery(q)
            comps.Consume(consumer)
