namespace Ehingeeinae.Ecs.Scheduling

open System
open System.Threading
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Systems
open TypeShape.Core

type ScheduledSystemComponent =
    { Type: Type
      IsReadOnly: bool }

module ScheduledSystemComponent =

    let isColliding (cs1: ScheduledSystemComponent list) (cs2: ScheduledSystemComponent list) : bool =
        Seq.allPairs cs1 cs2
        |> Seq.exists ^fun (c1, c2) ->
            let anyIsWrite = not c1.IsReadOnly || not c2.IsReadOnly
            c1.Type = c2.Type && anyIsWrite


type SchedulerSystem =
    { System: IEcsSystem
      UsingComponents: ScheduledSystemComponent list
      Label: string }

module SchedulerSystem =

    let isColliding (s1: SchedulerSystem) (s2: SchedulerSystem) : bool =
        ScheduledSystemComponent.isColliding s1.UsingComponents s2.UsingComponents

    let batched (systems: SchedulerSystem list) : SchedulerSystem list list =
        // let mutable currentBatch = []
        // let mutable allBatches = []
        let currentBatch = ResizeArray()
        let allBatches = ResizeArray()
        for system in systems do
            let isColliding = currentBatch |> Seq.exists (isColliding system)
            if isColliding then
                // allBatches <- (List.rev currentBatch) :: allBatches
                // currentBatch <- [system]
                allBatches.Add(currentBatch |> Seq.toList)
                currentBatch.Clear()
            else
                // currentBatch <- system :: currentBatch
                currentBatch.Add(system)
        // if not currentBatch.IsEmpty then
        //     allBatches <- (List.rev currentBatch) :: allBatches
        if currentBatch.Count > 0 then
            allBatches.Add(currentBatch |> Seq.toList)
        // allBatches |> List.rev
        allBatches |> Seq.toList


type SchedulerSystemGroup =
    { Systems: SchedulerSystem list
      Label: string }


type IEcsQueryFactory =
    abstract CreateQuery<'q> : unit -> IEcsQuery<'q>

type FooSystemBuilder() =
    let systems: ResizeArray<SchedulerSystem> = ResizeArray()

    member this.AddSystem(label: string, systemFactory: IEcsQueryFactory -> IEcsSystem): FooSystemBuilder =
        let compTypes: ResizeArray<Type * bool> = ResizeArray()
        let queryFactory = { new IEcsQueryFactory with
            member _.CreateQuery<'q>() =
                let query = EcsQueryCreating.mkQuery<'q> ()
                match shapeof<'q> with
                | Shape.EcsReadComponent shape -> compTypes.Add((shape.Component.Type, true))
                | Shape.EcsWriteComponent shape -> compTypes.Add(shape.Component.Type, true)
                | _ -> ()
                query
        }
        let system = systemFactory queryFactory
        let schedulerSystem =
            let usingComponents = compTypes |> Seq.map (fun (compType, isReadOnly) -> { Type = compType; IsReadOnly = isReadOnly }) |> Seq.toList
            { System = system; Label = label; UsingComponents = usingComponents }
        systems.Add(schedulerSystem)
        this

    member this.Build() = systems |> Seq.toList


type EcsScheduler(systems: SchedulerSystem list) =
    member this.Run(groupLabels: string list) =
        let systems = systems |> Seq.filter ^fun s -> List.contains s.Label groupLabels
        let systemBatches = SchedulerSystem.batched (systems |> Seq.toList)
        for systemBatch in systemBatches do
            let barrier = ref 0L
            for system in systemBatch do
                ThreadPool.QueueUserWorkItem(fun _ ->
                    system.System.Update(Unchecked.defaultof<_>)
                    Interlocked.Increment(&barrier.contents) |> ignore
                ) |> ignore
            while Interlocked.Read(&barrier.contents) <> int64 systemBatch.Length do
                ignore ()
