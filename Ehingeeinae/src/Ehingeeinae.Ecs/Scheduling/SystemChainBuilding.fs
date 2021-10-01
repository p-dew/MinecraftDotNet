module Ehingeeinae.Ecs.Scheduling.SystemChainBuilding

open System
open System.Collections.Generic
open System.ComponentModel.Design
open System.Runtime.CompilerServices

open Ehingeeinae.Ecs.Querying.RuntimeCompilation
open TypeShape.Core

open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Ecs.Utils


module SystemChain =

    let optimizeConflicts (chain: SystemChain) : SystemChain =
        chain



    let () = ()


type EcsSystemFactoryContext =
    { Queries: IEcsQueryFactory
      Resources: IEcsResourceProvider }

type IEcsSystemFactory =
    abstract CreateSystem: context: EcsSystemFactoryContext -> IEcsSystem

module EcsSystemFactory =
    let inline create createSystem = { new IEcsSystemFactory with member _.CreateSystem(qf) = createSystem qf }

type BuildingSystemComponent = { Type: Type; IsUnique: bool }

type BuildingSystemResource = { Type: Type; IsUnique: bool }

type BuildingChainedSystem =
    { UsingComponents: BuildingSystemComponent seq
      UsingResources: BuildingSystemResource seq
      ChainedSystem: ChainedSystem }

module SystemConflict =
    let isConflictingCompTypes (comps1: BuildingSystemComponent seq) (comps2: BuildingSystemComponent seq) : bool =
        Seq.allPairs comps1 comps2
        |> Seq.exists ^fun (c1, c2) ->
            let anyIsUnique = c1.IsUnique || c2.IsUnique
            c1.Type = c2.Type && anyIsUnique

    let ofBuildingSystems (buildingSystems: BuildingChainedSystem seq) : SystemConflict list =
        // TODO: Remove this ugly hack with duplicates
        // TODO: Add resources in conflict count
        [
            for bsys1 in buildingSystems do
                for bsys2 in buildingSystems do
                    if bsys1 <> bsys2 then
                        let conflicts = isConflictingCompTypes bsys1.UsingComponents bsys2.UsingComponents
                        if conflicts then
                            yield { ConflictingSystems = [ bsys1.ChainedSystem.System; bsys2.ChainedSystem.System ] }
        ]


type SystemChainBuilder(queryFactory: IEcsQueryFactory, resourceProvider: IEcsResourceProvider) =

    let buildingSystems = ResizeArray<BuildingChainedSystem>()
    let manualConflicts = ResizeArray<IEcsSystem array>()

    let nextLoopId =
        let mutable lastLoopId = uint64 -1 // bypass first id
        fun () ->
            lastLoopId <- lastLoopId + 1UL
            lastLoopId

    member this.CreateLoop(timing, executor): SystemLoop =
        let lid = SystemLoopId (nextLoopId ())
        { Id = lid; Timing = timing; Executor = executor }

    member this.AddSystem(loop: SystemLoop, systemFactory: IEcsSystemFactory) =
        let mutable systemFactoryExecuted = false

        let usingComponents = ResizeArray<Type * bool>()
        let writingQueryFactory =
            { new IEcsQueryFactory with
                member _.CreateQuery<'q>() =
                    if systemFactoryExecuted then invalidOp "Cannot call query factory after system factory is executed"

                    let query = queryFactory.CreateQuery<'q>()
                    let compTypes = QueryArgument.ofShape shapeof<'q> |> QueryArgument.getCompTypes
                    usingComponents.AddRange(compTypes)
                    query
            }

        let usingResources = ResizeArray<Type * bool>()
        let writingResourceProvider =
            { new IEcsResourceProvider with
                member _.GetShared<'T>() =
                    if systemFactoryExecuted then invalidOp "Cannot call resource provider after system factory is executed"
                    usingResources.Add(typeof<'T>, false)
                    resourceProvider.GetShared<'T>()
                member _.GetUnique<'T>() =
                    if systemFactoryExecuted then invalidOp "Cannot call resource provider after system factory is executed"
                    usingResources.Add(typeof<'T>, true)
                    resourceProvider.GetUnique<'T>()
            }

        let system =
            let factoryContext = { Queries = writingQueryFactory; Resources = writingResourceProvider }
            systemFactory.CreateSystem(factoryContext)
        systemFactoryExecuted <- true

        let buildingSystem =
            let chainedSystem = { System = system; Loop = loop }
            let usingComponents: BuildingSystemComponent seq = usingComponents |> Seq.map (fun (ty, uq) -> { Type = ty; IsUnique = uq })
            let usingResources: BuildingSystemResource seq = usingResources |> Seq.map (fun (ty, uq) -> { Type = ty; IsUnique = uq })
            { ChainedSystem = chainedSystem
              UsingComponents = usingComponents
              UsingResources = usingResources }
        buildingSystems.Add(buildingSystem)

        this

    member this.AddConflict(conflictingSystems: IEcsSystem seq) =
        manualConflicts.Add(conflictingSystems |> Seq.toArray)
        this

    member this.Build(): SystemChain =
        let chain =
            let conflicts = SystemConflict.ofBuildingSystems buildingSystems |> Seq.toList
            let systems = buildingSystems |> Seq.map (fun x -> x.ChainedSystem) |> Seq.toList
            { Conflicts = conflicts; Systems = systems }
        let chain = chain |> SystemChain.optimizeConflicts
        chain


[<AutoOpen>]
module Extensions =
    type SystemChainBuilder with
        member this.AddSystem(loop: SystemLoop, systemFactory: EcsSystemFactoryContext -> #IEcsSystem) =
            let systemFactory = { new IEcsSystemFactory with member _.CreateSystem(ctx) = upcast systemFactory ctx }
            this.AddSystem(loop, systemFactory)
