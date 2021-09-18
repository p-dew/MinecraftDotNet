namespace Ehingeeinae.Ecs.Experimental.Storage

open System
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Experimental.Storage

[<Struct>]
type ColumnIndex = ColumnIndex of int

/// Таблица колонок компонентов в архетипе
type Table =

    val entityIds: ResizeArray<EntityId>
    val columns: IDynamicColumn array

    internal new(entities, columns) =
        { entityIds = entities; columns = columns }

    member this.Columns = this.columns
    member this.Entities = this.entityIds

    member this.FindColumnIndex<'T>() =
        let ty = typeof<'T>
        this.Columns
        |> Array.tryFindIndex ^fun col ->
            col.ComponentType = ty
        |> Option.map ColumnIndex

    member this.GetColumn<'T>(cid: ColumnIndex) =
        let (ColumnIndex idx) = cid
        let col = this.Columns.[idx]
        assert(col.ComponentType = typeof<'T>)
        col.Cast<'T>()

    member this.SwapRemove(idx: int): unit =
        let lastE = this.entityIds.[this.entityIds.Count - 1]
        this.entityIds.[idx] <- lastE
        this.entityIds.RemoveAt(this.entityIds.Count - 1)
        for col in this.columns do
            col.SwapRemove(idx)


type TableBuilder() =
    let mutable columns = ResizeArray<IDynamicColumn>(32)

    member this.RegisterColumn<'T>(): unit =
        let ty = typeof<'T>
        let hasColumn =
            columns
            |> Seq.tryFind (fun col -> col.ComponentType = ty)
            |> Option.isSome
        if hasColumn then failwith "Duplicate component column"

        let element = Column<'T>() :> IDynamicColumn
        columns.Add(element)

    member this.Build() =
        let columns = columns.ToArray()
        Table(ResizeArray<_>(32), columns)
