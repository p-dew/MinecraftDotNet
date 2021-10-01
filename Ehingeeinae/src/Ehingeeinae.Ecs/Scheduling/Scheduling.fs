namespace Ehingeeinae.Ecs.Scheduling

open System
open System.Collections.Generic
open System.Diagnostics
open System.Threading
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Systems
open Microsoft.Extensions.Logging


// ----

type SystemLoopId = SystemLoopId of uint64

type ISystemExecutor =
    abstract StartExecuteSystem: system: IEcsSystem -> WaitHandle

type SystemLoop =
    { Id: SystemLoopId
      IntervalInSeconds: float32
      Executor: ISystemExecutor }

type ChainedSystem =
    { System: IEcsSystem
      Loop: SystemLoop }

type SystemConflict =
    { ConflictingSystems: IEcsSystem list }

type SystemChain =
    { Systems: ChainedSystem list
      Conflicts: SystemConflict list }

// ----

module SystemExecutor =

    let threadPool () =
        { new ISystemExecutor with
            member _.StartExecuteSystem(system) =
                let wh = new ManualResetEvent(false)
                ThreadPool.QueueUserWorkItem(fun _ ->
                    let ctx = { Empty = () }
                    system.Update(ctx)
                    wh.Set() |> ignore
                ) |> ignore
                upcast wh
        }

    let command (send: (unit -> unit) -> unit) =
        { new ISystemExecutor with
            member _.StartExecuteSystem(system) =
                let wh = new ManualResetEvent(false)
                send (fun () ->
                    let ctx = { Empty = () }
                    system.Update(ctx)
                    wh.Set() |> ignore
                )
                upcast wh
        }

// module ScheduledSystemComponent =
//
//     let isColliding (cs1: ScheduledSystemComponent list) (cs2: ScheduledSystemComponent list) : bool =
//         Seq.allPairs cs1 cs2
//         |> Seq.exists ^fun (c1, c2) ->
//             let anyIsMutable = c1.IsMutable || c2.IsMutable
//             c1.Type = c2.Type && anyIsMutable

[<RequireQualifiedAccess>]
module Seq =

    let existsBoth (predicate1: 'a -> bool) (predicate2: 'a -> bool) (source: 'a seq) : bool =
        let enumerator = source.GetEnumerator()
        let mutable contains1 = false
        let mutable contains2 = false
        while not (contains1 && contains2) && enumerator.MoveNext() do
            let x = enumerator.Current
            if not contains1 && predicate1 x then
                contains1 <- true
            if not contains2 && predicate2 x then
                contains2 <- true
        contains1 && contains2

    let containsBoth (value1: 'a) (value2: 'a) (source: 'a seq) : bool =
        existsBoth (fun x -> value1 = x) (fun x -> value2 = x) source

module ChainedSystem =

    let isSystemsConflicting (conflicts: IEcsSystem seq seq) (s1: IEcsSystem) (s2: IEcsSystem) : bool =
        conflicts
        |> Seq.exists (Seq.containsBoth s1 s2)

    let batched (conflicts: IEcsSystem seq seq) (systems: ChainedSystem seq) : ChainedSystem array array =
        [|
            let currentBatch = ResizeArray<ChainedSystem>()
            for system in systems do
                let systemsIsConflicting =
                    currentBatch
                    |> Seq.map (fun s -> s.System)
                    |> Seq.exists (isSystemsConflicting conflicts system.System)
                if systemsIsConflicting then
                    yield currentBatch.ToArray() // make a copy
                    currentBatch.Clear()
                    currentBatch.Add(system)
                else
                    currentBatch.Add(system)
            if currentBatch.Count > 0 then
                yield currentBatch.ToArray() // make a copy
        |]

module Disposable =
    let disposeAll (disposables: #IDisposable seq) : unit =
        for disposable in disposables do
            disposable.Dispose()

type SystemLoopUpdater(chain: SystemChain, logger: ILogger<SystemLoopUpdater>) =

    let systems = chain.Systems |> Seq.toArray
    let conflicts = chain.Conflicts |> Seq.map (fun c -> Seq.cache c.ConflictingSystems) |> Seq.cache

    let chainedSystemCache = Dictionary<SortedSet<uint64>, ChainedSystem array array>(SortedSet.CreateSetComparer())

    let getSystems (groupIds: SystemLoopId seq) =
        let gids = groupIds
        let groupIds = groupIds |> Seq.map (fun (SystemLoopId x) -> x)
        let groupIds = SortedSet<_>(groupIds)

        match chainedSystemCache.TryGetValue(groupIds) with
        | true, systems -> systems
        | false, _ ->
            let systemsFiltered = systems |> Seq.filter ^fun s -> Seq.contains s.Loop.Id gids
            let systemBatches = ChainedSystem.batched conflicts systemsFiltered
            chainedSystemCache.Add((groupIds, systemBatches))
            systemBatches

    member this.UpdateLoops(loopIds: SystemLoopId seq) =
        let systemBatches = getSystems loopIds
        for systemBatch in systemBatches do
            let systemWaitHandles =
                Array.init systemBatch.Length ^fun i ->
                    let system = systemBatch.[i]
                    let executor = system.Loop.Executor
                    let wh = executor.StartExecuteSystem(system.System)
                    logger.LogTrace($"Start update {system.System.GetType().Name}")
                    wh
            WaitHandle.WaitAll(systemWaitHandles) |> ignore
            systemWaitHandles |> Disposable.disposeAll

// --------


type private IntervalGroupRunning =
    { LoopId: SystemLoopId
      IntervalMs: float32
      mutable LastUpdateMs: float32 }

type TimedScheduler(updater: SystemLoopUpdater, chain: SystemChain) =

    member this.Run(?ct: CancellationToken) =
        let ct = CancellationToken.None |> defaultArg ct

        let intervals =
            chain.Systems
            |> Seq.map ^fun s -> { LoopId = s.Loop.Id; IntervalMs = s.Loop.IntervalInSeconds * 1000.f; LastUpdateMs = 0.f }
            |> Seq.toArray
        let shouldUpdate = ResizeArray(intervals.Length)

        let stopwatch = Stopwatch()
        stopwatch.Start()
        while not ct.IsCancellationRequested do
            let elapsedMs = stopwatch.Elapsed.TotalMilliseconds |> float32
            for interval in intervals do
                if interval.LastUpdateMs + interval.IntervalMs < elapsedMs then
                    shouldUpdate.Add(interval.LoopId)
                    interval.LastUpdateMs <- interval.LastUpdateMs + interval.IntervalMs
            if shouldUpdate.Count > 0 then
                updater.UpdateLoops(shouldUpdate)
                shouldUpdate.Clear()
