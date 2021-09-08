namespace Ehingeeinae.Ecs.Scheduling

open System
open System.Diagnostics
open System.Threading
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Systems

type ScheduledSystemComponent =
    { Type: Type
      IsReadOnly: bool }

module ScheduledSystemComponent =

    let isColliding (cs1: ScheduledSystemComponent list) (cs2: ScheduledSystemComponent list) : bool =
        Seq.allPairs cs1 cs2
        |> Seq.exists ^fun (c1, c2) ->
            let anyIsWrite = not c1.IsReadOnly || not c2.IsReadOnly
            c1.Type = c2.Type && anyIsWrite

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

    let batched (systems: SchedulerSystem list) : SchedulerSystem list list =
        let currentBatch = ResizeArray()
        let allBatches = ResizeArray()
        for system in systems do
            let isColliding = currentBatch |> Seq.exists (isColliding system)
            if isColliding then
                allBatches.Add(currentBatch |> Seq.toList)
                currentBatch.Clear()
                currentBatch.Add(system)
            else
                currentBatch.Add(system)
        if currentBatch.Count > 0 then
            allBatches.Add(currentBatch |> Seq.toList)
        allBatches |> Seq.toList


type SystemGroupUpdater(systems: SchedulerSystem seq) =
    member this.UpdateGroups(groupIds: GroupId seq) =
        let systems = systems |> Seq.filter ^fun s -> Seq.contains s.GroupInfo.Id groupIds

        let systemBatches = SchedulerSystem.batched (systems |> Seq.toList)
        for systemBatch in systemBatches do
            let mutable completed = 0
            use waitHandle = new ManualResetEvent(false)
            for system in systemBatch do
                match system.GroupInfo.Threading with
                | Threading.Thread runner ->
                    runner ^fun () ->
                        let ctx = Unchecked.defaultof<_>
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
            updater.UpdateGroups(shouldUpdate)
            shouldUpdate.Clear()
