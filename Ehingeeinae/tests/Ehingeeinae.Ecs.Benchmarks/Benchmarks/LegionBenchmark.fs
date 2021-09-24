module Ehingeeinae.Ecs.Benchmarks.Benchmarks.LegionBench

open BenchmarkDotNet.Attributes
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Worlds
open Microsoft.Extensions.Logging.Abstractions

type [<Struct>] A = { A: float32 }
type [<Struct>] B = { B: float32 }
type [<Struct>] C = { C: float32 }
type [<Struct>] D = { D: float32 }
type [<Struct>] E = { E: float32 }
type [<Struct>] F = { F: float32 }

type [<Struct>] Tag = { Tag: float32 }

type [<Struct>] Position = { Position: float32 }
type [<Struct>] Rotation = { Rotation: float32 }


let seedWorld (worldManager: IEcsWorldEntityManager) (countUseful: int) (countBackground: int) : unit =
    let countUseful = (countUseful + 3) / 4
    let countBackground = (countBackground + 3) / 4

    worldManager.AddEntities([ for i in 0 .. countBackground-1 -> { A = float32 i } ]) |> ignore
    worldManager.AddEntities([ for i in 0 .. countBackground-1 -> { B = float32 i } ]) |> ignore
    worldManager.AddEntities([ for i in 0 .. countBackground-1 -> { C = float32 i } ]) |> ignore
    worldManager.AddEntities([ for i in 0 .. countBackground-1 -> let i = float32 i in {A=i},{B=i},{C=i},{D=i},{E=i},{F=i} ]) |> ignore

    worldManager.AddEntities([ for i in 0 .. countUseful-1 -> { Position = float32 i }, { Rotation = float32 i } ]) |> ignore
    worldManager.AddEntities([ for i in 0 .. countUseful-1 -> { Position = float32 i }, { Rotation = float32 i }, { A = float32 i } ]) |> ignore
    worldManager.AddEntities([ for i in 0 .. countUseful-1 -> { Position = float32 i }, { Rotation = float32 i }, { B = float32 i } ]) |> ignore
    worldManager.AddEntities([ for i in 0 .. countUseful-1 -> { Position = float32 i }, { Rotation = float32 i }, { A = float32 i }, { B = float32 i } ]) |> ignore


let benchIterSimple =
    let query = EcsQueryCreating.mkQuery<Position cread * Rotation cwrite> ()
    fun (queryExecutor: EcsWorldQueryExecutor) ->
        let comps = queryExecutor.ExecuteQuery(query)
        for pos, rot in comps do
            let newRot = { Rotation = pos.Value.Position }
            EcsWriteComponent.setValue rot &newRot


let benchIterComplex =
    let query = EcsQueryCreating.mkQuery<Position cread * Rotation cwrite> () |> EcsQuery.withFilter -EcsQueryFilter.comp<A>
    fun (queryExecutor: EcsWorldQueryExecutor) ->
        let comps = queryExecutor.ExecuteQuery(query)
        for pos, rot in comps do
            let newRot = { Rotation = pos.Value.Position }
            EcsWriteComponent.setValue rot &newRot


type LegionBenchmarkSimple() =

    let mutable queryExecutor = Unchecked.defaultof<EcsWorldQueryExecutor>

    [<GlobalSetup>]
    member this.GlobalSetup() =
        let world = EcsWorld.createEmpty ()
        let worldManager = EcsWorldEntityManager(world, NullLogger<_>.Instance)
        do seedWorld worldManager 2000 10000
        do worldManager.Commit()
        queryExecutor <- EcsWorldQueryExecutor(world)

    [<Benchmark>]
    member this.BenchIterComplex() = benchIterComplex queryExecutor


type LegionBenchmarkComplex() =

    let mutable queryExecutor = Unchecked.defaultof<EcsWorldQueryExecutor>

    [<GlobalSetup>]
    member this.GlobalSetup() =
        let world = EcsWorld.createEmpty ()
        let worldManager = EcsWorldEntityManager(world, NullLogger<_>.Instance)
        do seedWorld worldManager (200 * 2000) 10000
        do worldManager.Commit()
        queryExecutor <- EcsWorldQueryExecutor(world)

    [<Benchmark>]
    member this.BenchIterComplex() = benchIterComplex queryExecutor
