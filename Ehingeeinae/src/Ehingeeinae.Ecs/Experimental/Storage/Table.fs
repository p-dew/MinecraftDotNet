namespace rec Ehingeeinae.Ecs.Experimental.Storage

open System
open System.Collections.Generic

open Ehingeeinae.Utils
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Experimental.Storage

module Shaping =

    open FSharp.Quotations
    open TypeShape.Core
    open Ehingeeinae.Utils.QuotationExtensions

    module Shape =

        let (|TypeSet|) (shape: TypeShape) =
            match shape with
            | Shape.Tuple shapeTuple -> shapeTuple.Elements |> Seq.map (fun s -> s.Member)
            | Shape.FSharpRecord shapeRecord -> shapeRecord.Fields |> Seq.map (fun s -> s.Member)
            | shape -> upcast [shape]
            |> Seq.toArray


    let mkAddEntities<'cs> (table: Table) : Expr<IReadOnlyList<'cs> -> IReadOnlyList<EntityId>> =
        let shapeCs = shapeof<'cs>
        let innerShapes =
            match shapeCs with
            | Shape.TypeSet shapes -> shapes

        let compTypesEqualsTableTypes =
            let compTypesSet = innerShapes |> Seq.map (fun sh -> sh.Type) |> HashSet
            fun (table: Table) ->
                let tableTypes = table.Columns |> Seq.map (fun c -> c.ComponentType)
                compTypesSet.SetEquals(tableTypes)

        if not (compTypesEqualsTableTypes table) then invalidOp "Types don't match"

        Expr.lambda1<IReadOnlyList<'cs>, IReadOnlyList<EntityId>> "css" (fun cssExpr ->
            Expr.Cast <| Expr.letMany (
                innerShapes
                |> Seq.mapi (fun i compShape ->
                    compShape.Accept({ new ITypeVisitor<_> with
                        member _.Visit<'c>() =
                            Var($"col{i}", typeof<Column<'c>>),
                            <@@
                                let colIdx = table.TryFindColumnIndex(typeof<'c>) |> Option.get
                                let col = table.GetColumn<'c>(colIdx)
                                col
                            @@>
                    })
                )
                |> Seq.toList
            ) (fun colVarExprs ->
                let addToColsExpr =
                    Expr.lambda1<'cs, unit> "cs" (fun csExpr ->
                        Expr.Cast <| Expr.sequentialMany [
                            yield! innerShapes |> Seq.mapi (fun i cShape ->
                                cShape.Accept({ new ITypeVisitor<_> with
                                    member _.Visit<'c>() =
                                        <@@
                                            let c: 'c = %%(Expr.TupleGet(csExpr, i))
                                            let col: Column<'c> = %%(colVarExprs.[i])
                                            col.Add(c, Unchecked.defaultof<_>)
                                        @@>
                                })
                            )
                        ]
                    )
                <@@
                    let css = %cssExpr
                    let eids = ResizeArray<EntityId>(css.Count)
                    for cs in css do
                        (%addToColsExpr) cs

                        let eid = EntityId()
                        table.EntityIds.Add(eid)
                        eids.Add(eid)

                    eids :> IReadOnlyList<_>
                @@>
            )
        )


[<Struct>]
type ColumnIndex = ColumnIndex of int

/// Таблица колонок компонентов в архетипе
type Table =

    val private entityIds: ResizeArray<EntityId>
    val private columns: IDynamicColumn array

    internal new(entities, columns) =
        { entityIds = entities; columns = columns }

    member this.Columns: IDynamicColumn array = this.columns
    member this.EntityIds: ResizeArray<EntityId> = this.entityIds

    member this.TryFindColumnIndex(compType: Type) =
        this.Columns
        |> Array.tryFindIndex ^fun col ->
            col.ComponentType = compType
        |> Option.map ColumnIndex

    member this.GetDynamicColumn(colIdx: ColumnIndex): IDynamicColumn =
        let (ColumnIndex idx) = colIdx
        let col = this.Columns.[idx]
        col

    member this.GetColumn<'T>(colIdx: ColumnIndex): Column<'T> =
        let col = this.GetDynamicColumn(colIdx)
        assert(col.ComponentType = typeof<'T>)
        col.Cast<'T>()

    member this.SwapRemove(idx: int): unit =
        let lastE = this.entityIds.[this.entityIds.Count - 1]
        this.entityIds.[idx] <- lastE
        this.entityIds.RemoveAt(this.entityIds.Count - 1)
        for col in this.columns do
            col.SwapRemove(idx)


    member this.AddEntities(css: IReadOnlyList<'cs>): IReadOnlyList<EntityId> =
        let addEntities = Shaping.mkAddEntities<'cs> this
        // addEntities css
        // let col0Idx = this.TryFindColumnIndex(typeof<'c0>) |> Option.get
        // let col1Idx = this.TryFindColumnIndex(typeof<'c1>) |> Option.get
        //
        // let col0 = this.GetColumn<'c0>(col0Idx)
        // let col1 = this.GetColumn<'c1>(col1Idx)
        //
        // let eids = ResizeArray<EntityId>(css.Count)
        // for cs in css do
        //     let c0 = cs.[0]
        //     let c1 = cs.[1]
        //
        //     col0.Add(c0)
        //     col1.Add(c1)
        //
        //     let eid = EntityId()
        //     this.EntityIds.Add(eid)
        //     eids.Add(eid)
        //
        // upcast eids

        failwith ""

        // foo<'cs> (fun shapes ->
        //     let col0Idx = this.TryFindColumnIndex(shapes.[0].Type) |> Option.get
        //     let col1Idx = this.TryFindColumnIndex(shapes.[1].Type) |> Option.get
        //
        //     shapes.[0].Visit({ new Visitor with
        //         member _.Visit<'c>() =
        //             let col = this.GetColumn<'c>(col0Idx)
        //
        //     })
        //     let col0 = this.GetColumn<'c0>(col0Idx)
        //     let col1 = this.GetColumn<'c1>(col1Idx)
        //
        //     let eids = ResizeArray<EntityId>(css.Count)
        // )



type TableBuilder() =
    let columns = ResizeArray<IDynamicColumn>(32)

    member this.RegisterColumn<'T>(): TableBuilder =
        let ty = typeof<'T>
        let hasColumn =
            columns
            |> Seq.tryFind (fun col -> col.ComponentType = ty)
            |> Option.isSome
        if hasColumn then failwith "Duplicate component column"

        let col = Column<'T>()
        columns.Add(col)
        this

    member this.Build() =
        let columns = columns.ToArray()
        Table(ResizeArray<_>(32), columns)
