namespace Ehingeeinae.Ecs.Experimental.Storage

open System
open System.Collections.Generic


open Ehingeeinae.Utils
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Experimental.Storage.Archetype

[<Struct>]
type TableIndex = TableIndex of int


[<Struct>]
type EntityLocation =
    { TableIndex: TableIndex
      EntityIndex: int }

[<AutoOpen>]
module internal Internals =

    module Archetype =
        let ofTable (t: Table) =
            let comps = t.Columns |> Seq.map (fun col -> col.ComponentType)
            let typeSet = HashSet(comps)
            Archetype(typeSet)

type IEcsStorage =
    abstract AddEntity: cs: 'cs -> EntityId
    abstract AddComponents: eid: EntityId * cs: 'cs -> unit
    abstract RemoveEntity: eid: EntityId -> unit
    [<RequiresExplicitTypeArguments>]
    abstract RemoveComponents<'cs> : eid: EntityId -> unit


type StorageTable =
    { Table: Table
      Archetype: Archetype }

// Хранилище всех сущностей во всех архетипах
type Storage() =
    // Массив поколений сущности. Вероятно, не может быть уменьшен в процессе работы
    // без прекращения отслеживания устаревших Id.
    let generations = ResizeArray<Generation>()
    let locations = ResizeArray<EntityLocation>()
    let tables = ResizeArray<StorageTable>()

//    member this.TryAddTable(types: Type array) =
//        let arch = Archetype(HashSet(types))
//        let hasDuplicate =
//            tables
//            |> Seq.tryFind (fun struct(arch', _) -> arch'.Equals(arch))
//            |> Option.isSome
//        if hasDuplicate then false
//        else
//
//        ()

    member this.FindTable(compTypes: Type seq): Table =
        tables
        |> Seq.pick ^fun table ->
            if table.Archetype.Equals(compTypes)
            then Some table.Table
            else None

    member this.MatchTables(compTypes: Type seq): Table seq =
        tables
        |> Seq.choose ^fun table ->
            if table.Archetype.ContainsAll(compTypes)
            then Some table.Table
            else None

    member this.AddEntity(cs: 'cs): EntityId =
        failwith ""

    interface IEcsStorage with
        member this.AddComponents(eid, cs) = failwith "todo"
        member this.AddEntity(cs) = this.AddEntity(cs)
        member this.RemoveComponents<'cs>(eid) = failwith "todo"
        member this.RemoveEntity(eid) = failwith "todo"
