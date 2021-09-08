namespace Ehingeeinae.Ecs.Scheduling

open System
open System.Collections.Generic
open System.Diagnostics
open System.Threading
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Systems

type ScheduledSystemComponent =
    { Type: Type
      IsMutable: bool }

module ScheduledSystemComponent =

    let isColliding (cs1: ScheduledSystemComponent list) (cs2: ScheduledSystemComponent list) : bool =
        Seq.allPairs cs1 cs2
        |> Seq.exists ^fun (c1, c2) ->
            let anyIsMutable = c1.IsMutable || c2.IsMutable
            c1.Type = c2.Type && anyIsMutable

type GroupId = GroupId of Guid

[<RequireQualifiedAccess>]
type Threading =
    | Thread of ((unit -> unit) -> unit)
    | ThreadPool

type GroupInfo =
    { Id: GroupId
      Name: string
      Threading: Threading }

type SchedulerSystem =
    { System: IEcsSystem
      UsingComponents: ScheduledSystemComponent list option
      GroupInfo: GroupInfo }

module SchedulerSystem =

    let isColliding (s1: SchedulerSystem) (s2: SchedulerSystem) : bool =
        match s1.UsingComponents, s2.UsingComponents with
        | Some cs1, Some cs2 ->
            ScheduledSystemComponent.isColliding cs1 cs2
        | _ -> true

    let batched (systems: SchedulerSystem seq) : SchedulerSystem array array =
        [|
            let currentBatch = ResizeArray()
            for system in systems do
                let isColliding = currentBatch |> Seq.exists (isColliding system)
                if isColliding then
                    yield currentBatch.ToArray() // make a copy
                    currentBatch.Clear()
                    currentBatch.Add(system)
                else
                    currentBatch.Add(system)
            if currentBatch.Count > 0 then
                yield currentBatch.ToArray() // make a copy
        |]


type SystemGroupUpdater(systems: SchedulerSystem seq) =
    let systems = systems |> Seq.toArray

    // let groupSystemCache = Dictionary<HashSet<GroupId>, SchedulerSystem array array>(HashSet.CreateSetComparer())
    // let groupSystemCache = ResizeArray<GroupId seq * SchedulerSystem array array>()

    let getSystems (groupIds: GroupId seq) =
        // let r =
        //     groupSystemCache
        //     |> Seq.tryPick ^fun (gids, systems) ->
        //         let isSame = (Seq.compareWith Operators.compare groupIds gids) = 0
        //         if isSame
        //         then Some systems
        //         else None
        // match r with
        // | Some systemBatches -> systemBatches
        // | None ->
            let systemsFiltered = systems |> Seq.filter ^fun s -> Seq.contains s.GroupInfo.Id groupIds
            let systemBatches = SchedulerSystem.batched systemsFiltered
            // groupSystemCache.Add((groupIds, systemBatches))
            systemBatches

    member this.UpdateGroups(groupIds: GroupId seq) =
        let systemBatches = getSystems groupIds
        for systemBatch in systemBatches do
            let mutable completed = 0
            use waitHandle = new ManualResetEvent(false)
            for system in systemBatch do
                match system.GroupInfo.Threading with
                | Threading.Thread runner ->
                    runner ^fun () ->
                        let ctx = { Empty = () }
                        system.System.Update(ctx)
                        let completed' = Interlocked.Increment(&completed)
                        if completed' = systemBatch.Length then waitHandle.Set() |> ignore

                | Threading.ThreadPool ->
                    ThreadPool.QueueUserWorkItem(fun _ ->
                        let ctx = Unchecked.defaultof<_>
                        system.System.Update(ctx)
                        let completed' = Interlocked.Increment(&completed)
                        if completed' = systemBatch.Length then waitHandle.Set() |> ignore
                    ) |> ignore

            waitHandle.WaitOne() |> ignore


// --------


type IntervalGroup =
    { GroupId: GroupId
      IntervalMs: float32 }

type private IntervalGroupRunning =
    { GroupId: GroupId
      IntervalMs: float32
      mutable LastUpdateMs: float32 }

type TimedScheduler(updater: SystemGroupUpdater, intervals: IntervalGroup seq) =

    member this.Run(?ct: CancellationToken) =
        let ct = CancellationToken.None |> defaultArg ct

        let intervals =
            intervals
            |> Seq.map ^fun i -> { GroupId = i.GroupId; IntervalMs = i.IntervalMs; LastUpdateMs = 0.f }
            |> Seq.toArray
        let shouldUpdate = ResizeArray(intervals.Length)

        let stopwatch = Stopwatch()
        stopwatch.Start()
        while not ct.IsCancellationRequested do
            let elapsedMs = stopwatch.Elapsed.TotalMilliseconds |> float32
            for interval in intervals do
                if interval.LastUpdateMs + interval.IntervalMs < elapsedMs then
                    shouldUpdate.Add(interval.GroupId)
                    interval.LastUpdateMs <- interval.LastUpdateMs + interval.IntervalMs
            if shouldUpdate.Count > 0 then
                updater.UpdateGroups(shouldUpdate)
                shouldUpdate.Clear()
