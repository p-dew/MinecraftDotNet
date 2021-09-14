namespace Ehingeeinae.Ecs.Experimental.Storage

open System
open Ehingeeinae.Ecs.Experimental.Storage

[<Struct>]
type ComponentId =
    { Index: int; Type: Type }

type TableBuilder() =
    let mutable columns = ResizeArray<struct(ComponentId * IDynamicColumn)>(32)

    member this.RegisterColumn<'T>() =
        let ty = typeof<'T>
        let hasColumn =
            columns
            |> Seq.tryFind (fun struct(cid, _) -> cid.Type = ty)
            |> Option.isSome
        if hasColumn then failwith "Duplicate component column"

        let newIdx = columns.Count
        let cid = { Index = newIdx; Type = ty }
        let element = struct(cid, Column<'T>() :> IDynamicColumn)
        columns.Add(element)
        ()

    member this.Build() =
        let columns = columns.ToArray()
        Table(ResizeArray<_>(32), columns)

/// Таблица колонок компонентов в архетипе
and Table =

    val entities: ResizeArray<EntityId>
    val columns: struct(ComponentId * IDynamicColumn) array

    internal new(entities: ResizeArray<EntityId>, columns: struct(ComponentId * IDynamicColumn) array) =
        { entities = entities; columns = columns }

    member this.Columns = this.columns
    member this.Entities = this.entities

    member this.FindColumn<'T>() =
        let ty = typeof<'T>
        this.Columns
        |> Seq.tryPick (fun struct(cid, col) ->
            if cid.Type = ty
            then Some struct(cid, (col :?> Column<'T>))
            else None)

    member this.GetColumn<'T>(cid: ComponentId) =
        let idx = cid.Index
        let struct(cid, col) = this.Columns.[idx]
        assert(typeof<'T> = cid.Type)
        col :?> Column<'T>

    member this.SwapRemove(idx: int) =
        let lastE = this.entities.[this.entities.Count - 1]
        this.entities.[idx] <- lastE
        this.entities.RemoveAt(this.entities.Count - 1)
        for struct(_, col) in this.columns do
            col.SwapRemove(idx)
        ()
