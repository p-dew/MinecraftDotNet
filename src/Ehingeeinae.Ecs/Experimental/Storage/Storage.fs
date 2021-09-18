namespace Ehingeeinae.Ecs.Experimental.Storage

open System
open System.Collections.Generic
open Ehingeeinae.Ecs.Experimental.Storage.Archetype

[<Struct>]
type TableId = TableId of int


[<Struct>]
type EntityLocation =
    { Table: TableId
      Index: int }

[<AutoOpen>]
module internal Internal =
    let archetypeOfTable (t: Table) =
        let comps = t.Columns |> Array.map (fun col -> col.ComponentType)
        let typeSet = HashSet(comps)
        Archetype(typeSet)


// Хранилище всех сущностей во всех архетипах
type Storage() =
    // Массив поколений сущности. Вероятно, не может быть уменьшен в процессе работы
    // без прекращения отслеживания устаревших Id.
    let generations = ResizeArray<Generation>()
    let locations = ResizeArray<EntityLocation>()
    let tables = ResizeArray<struct(Archetype * Table)>()

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

    member this.FindTable(comps: Type array) =

        ()
