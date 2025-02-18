namespace Ehingeeinae.Ecs.Querying.RuntimeCompilation

open System

open TypeShape.Core

open Ehingeeinae.Utils
open Ehingeeinae.Utils.QuotationExtensions
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds
open Ehingeeinae.Ecs.Querying

// --------
// Component Shaping

type IEcsComponentVisitor<'R> =
    abstract Visit<'c> : unit -> 'R

type IShapeEcsComponent =
    abstract Component: TypeShape
    abstract IsMutable: bool
    abstract Accept<'R> : IEcsComponentVisitor<'R> -> 'R

type ShapeEcsComponent<'c>(isMutable: bool) =
    interface IShapeEcsComponent with
        member _.Component = upcast shapeof<'c>
        member _.IsMutable = isMutable
        member this.Accept(v) = v.Visit<'c>()

[<RequireQualifiedAccess>]
module Shape =

    let (|EcsWriteComponent|EcsReadComponent|EcsEntityId|NotEcs|) (shape: TypeShape) =
        match shape.ShapeInfo with
        | TypeShapeInfo.Generic (td, tArgs) when td = typedefof<EcsReadComponent<_>> ->
            let shapeEcsComponent =
                Activator.CreateInstanceGeneric<ShapeEcsComponent<_>>(tArgs, [| false |])
                :?> IShapeEcsComponent
            EcsReadComponent shapeEcsComponent
        | TypeShapeInfo.Generic (td, tArgs) when td = typedefof<EcsWriteComponent<_>> ->
            let shapeEcsComponent =
                Activator.CreateInstanceGeneric<ShapeEcsComponent<_>>(tArgs, [| true |])
                :?> IShapeEcsComponent
            EcsWriteComponent shapeEcsComponent
        | _ when (match shape with :? TypeShape<EcsEntityId> -> true | _ -> false) ->
            EcsEntityId
        | _ -> NotEcs


[<RequireQualifiedAccess>]
type QueryArgumentPrimitive =
    | EntityId
    | Component of shape: IShapeEcsComponent

[<RequireQualifiedAccess>]
type QueryArgument =
    | Single of QueryArgumentPrimitive
    | Tuple of shapeTuple: IShapeTuple * items: QueryArgumentPrimitive array
    | Record of shapeRecord: IShapeFSharpRecord * items: QueryArgumentPrimitive array

module QueryArgument =

    let primitiveOfShape (shape: TypeShape) =
        match shape with
        | Shape.EcsWriteComponent shape -> Some ^ QueryArgumentPrimitive.Component shape
        | Shape.EcsReadComponent shape -> Some ^ QueryArgumentPrimitive.Component shape
        | Shape.EcsEntityId -> Some QueryArgumentPrimitive.EntityId
        | _ -> None

    let ofShape (shape: TypeShape) =
        match primitiveOfShape shape with
        | Some primitive -> QueryArgument.Single primitive
        | None ->
            let itemsFromMembers (itemsShapes: TypeShape seq) = [|
                for itemShape in itemsShapes do
                    match primitiveOfShape itemShape with
                    | Some item -> item
                    | None -> raise <| NotSupportedException($"Type '{shape.Type}' is not supported")
            |]
            match shape with
            | Shape.FSharpRecord shapeRecord ->
                let itemsShapes = shapeRecord.Fields |> Seq.map (fun f -> f.Member)
                let argumentItems = itemsFromMembers itemsShapes
                QueryArgument.Record (shapeRecord, argumentItems)
            | Shape.Tuple shapeTuple ->
                let itemsShapes = shapeTuple.Elements |> Seq.map (fun e -> e.Member)
                let argumentItems = itemsFromMembers itemsShapes
                QueryArgument.Tuple (shapeTuple, argumentItems)
            | _ ->
                raise <| NotSupportedException($"Type '{shape.Type}' is not supported")

    let getCompTypes (arg: QueryArgument) : (Type * bool) list =
        let getCompTypesOfPrimitive (primitive: QueryArgumentPrimitive) =
            match primitive with
            | QueryArgumentPrimitive.Component shape -> [ shape.Component.Type, shape.IsMutable ]
            | QueryArgumentPrimitive.EntityId -> [ ]
        match arg with
        | QueryArgument.Single primitive -> getCompTypesOfPrimitive primitive
        | QueryArgument.Record (_, items) | QueryArgument.Tuple (_, items) ->
            items |> Seq.collect getCompTypesOfPrimitive |> Seq.toList

module EcsQueryCreating =

    open Ehingeeinae.Collections
    open FSharp.Quotations
    open FSharp.Quotations.Evaluator
    open Microsoft.FSharp.Reflection

    type FetchF<'r> = ArchetypeStorage -> int -> 'r

    let mkQueryTupleOrRecord<'q> (shapeMembers: IShapeMember<'q>[]) createFromItems =

        let itemCount = shapeMembers.Length

        let compTypes =
            shapeMembers
            |> Array.collect ^fun shapeItem ->
                match shapeItem.Member with
                | Shape.EcsReadComponent shape -> shape.Accept({ new IEcsComponentVisitor<_> with member _.Visit<'c>() = [| typeof<'c> |] })
                | Shape.EcsWriteComponent shape -> shape.Accept({ new IEcsComponentVisitor<_> with member _.Visit<'c>() = [| typeof<'c> |] })
                | Shape.EcsEntityId -> [| |]
                | _ -> failwithf $"Unsupported type {shapeItem.Member.Type}"

        let (fetchExpr: Expr<ArchetypeStorage -> int -> 'q>) =
            // Из-за того, что каждая ветка в этой лямбде возвращает разный (в зависимости от кортежа 'q) тип,
            // чтобы обратно это можно было сконструировать, надо использовать quotations
            // let storageVar = Var("storage", typeof<ArchetypeStorage>)
            // Expr.lambdaMany [storageVar] (
            //     let storage = Expr.Var(storageVar) |> Expr.Cast<ArchetypeStorage>
            //     let itemFs = [
            //         for idxItem in 0 .. itemCount - 1 do
            //             let shapeItem = shapeMembers.[idxItem]
            //             let var = Var($"item{idxItem}", FSharpType.MakeFunctionType(typeof<int>, shapeItem.Member.Type))
            //             var,
            //             match shapeItem.Member with
            //             | Shape.EcsEntityId ->
            //                 <@@
            //                     let storage = %storage
            //                     let eids = storage.Ids
            //                     fun idxEntity ->
            //                         eids.[idxEntity]
            //                 @@>
            //             | Shape.EcsReadComponent shape ->
            //                 shape.Accept({ new IEcsComponentVisitor<_> with
            //                     member _.Visit<'c>() =
            //                         <@@
            //                             let storage = %storage
            //                             let col = storage.GetColumn<'c>()
            //                             let arr = col.Components |> ResizeArray.getItems
            //                             fun idxEntity ->
            //                                 itemAsReadComponent arr.Array idxEntity
            //                         @@>
            //                 })
            //             | Shape.EcsWriteComponent shape ->
            //                 shape.Accept({ new IEcsComponentVisitor<_> with
            //                     member _.Visit<'c>() =
            //                         <@@
            //                             let storage = %storage
            //                             let col = storage.GetColumn<'c>()
            //                             let arr = col.Components |> ResizeArray.getItems
            //                             fun idxEntity ->
            //                                 itemAsWriteComponent arr.Array idxEntity
            //                         @@>
            //                 })
            //             | _ ->
            //                 invalidOp $"Type (at {idxItem} position) is not supported as query element"
            //     ]
            //     Expr.letMany itemFs (
            //         let itemFVars = itemFs |> List.map fst
            //         let idxEntityVar = Var("idxEntity", typeof<int>)
            //         Expr.Lambda(
            //             idxEntityVar,
            //             let idxEntity = Expr.Var(idxEntityVar) |> Expr.Cast<int>
            //             let items = itemFVars |> List.map (fun itemFVar -> Expr.Application(Expr.Var(itemFVar), idxEntity))
            //             createFromItems items
            //         )
            //     )
            // )
            // |> Expr.Cast

            let itemsFStorageIdx = [|
                for idxItem in 0 .. itemCount - 1 do
                    let shapeItem = shapeMembers.[idxItem]
                    match shapeItem.Member with
                    | Shape.EcsEntityId ->
                        <@@
                            fun (storage: ArchetypeStorage) ->
                                let eids = storage.Ids
                                fun idxEntity ->
                                    eids.[idxEntity]
                        @@>
                    | Shape.EcsReadComponent shape ->
                        shape.Accept({ new IEcsComponentVisitor<_> with
                            member _.Visit<'c>() =
                                <@@
                                    fun (storage: ArchetypeStorage) ->
                                        let col = storage.GetColumn<'c>()
                                        fun idxEntity ->
                                            { EcsReadComponent.Column = col; Index = idxEntity }
                                @@>
                        })
                    | Shape.EcsWriteComponent shape ->
                        shape.Accept({ new IEcsComponentVisitor<_> with
                            member _.Visit<'c>() =
                                <@@
                                    fun (storage: ArchetypeStorage) ->
                                        let col = storage.GetColumn<'c>()
                                        fun idxEntity ->
                                            { EcsWriteComponent.Column = col; Index = idxEntity }
                                @@>
                        })
                    | _ ->
                        invalidOp $"Invalid type {shapeItem.Member.Type}"
                    ()
            |]
            let itemsFStorageIdxVars = [| for idxItem in 0 .. itemCount - 1 -> Var($"itemFStorageIdx{idxItem}", itemsFStorageIdx.[idxItem].Type) |]
            Expr.letMany [
                yield! Array.zip itemsFStorageIdxVars itemsFStorageIdx
            ] (fun _ ->
                let itemsFIdxVars = [| for idxItem in 0 ..itemCount - 1 -> Var($"itemFIdx{idxItem}", itemsFStorageIdx.[idxItem].Type.GetGenericArguments().[1]) |]
                let storageVar = Var("storage", typeof<ArchetypeStorage>)
                Expr.Lambda(
                    storageVar,
                    let storageArgExpr = Expr.Var(storageVar)
                    Expr.letMany [
                        for idxItem in 0 .. itemCount - 1 do
                            let itemFStorageIdxVar = itemsFStorageIdxVars.[idxItem]
                            let itemFIdxVar = itemsFIdxVars.[idxItem]
                            itemFIdxVar, Expr.Application(Expr.Var(itemFStorageIdxVar), storageArgExpr)
                    ] (fun _ ->
                        let idxEntityVar = Var("idxEntity", typeof<int>)
                        Expr.Lambda(
                            idxEntityVar,
                            let idxEntity = Expr.Var(idxEntityVar)
                            createFromItems [
                                for idxItem in 0 .. itemCount - 1 do
                                    let itemFIdxVar = itemsFIdxVars.[idxItem]
                                    Expr.Application(Expr.Var(itemFIdxVar), idxEntity)
                            ]
                        )
                    )
                )
            )
            |> Expr.Cast

        (*

        let itemFStorageIdx0 =
            fun storage ->
                let arr = storage.GetColumn()
                fun idxEntity ->
                    arr.[idxEntity]
        let itemFStorageIdx1 = ...

        fun storage ->
            let itemFIdx0 = itemFStorageIdx0 storage
            let itemFIdx1 = itemFStorageIdx1 storage
            fun idxEntity ->
                (itemFIdx0 idxEntity, itemFIdx1 idxEntity)

        // ----

        fun storage ->
            let itemFIdx0 =
                let arr = storage.GetColumn()
                fun idxEntity ->
                    arr.[idxEntity]
            let itemFIdx1 = ...
            fun idxEntity ->
                itemFIdx0 idxEntity, itemFIdx1 idxEntity

        *)

        // printfn $"fetchExpr:\n%A{fetchExpr}"

        printfn $"fetchExpr:\n<<<<\n{ExprToCode.ExprDisplay.display fetchExpr}\n>>>>"

        let fetch: ArchetypeStorage -> int -> 'q =
            // downcast (fetchExpr |> FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.EvaluateQuotation)
            fetchExpr.Evaluate()

        { new IEcsQuery<'q> with
            member _.Filter(archetype) =
                archetype.ComponentTypes.IsSupersetOf(compTypes)
            member _.Fetch(storage) =
                let entityCount = storage.Count
                seq {
                    let fetch = fetch storage
                    for idxEntity in 0 .. entityCount - 1 do
                        yield fetch idxEntity
                }
        }

    let mkQuerySingleValue<'q> (compTypes: Type seq) (fetchExpr: Expr<ArchetypeStorage -> int -> 'q>) =
        let fetch = fetchExpr.Evaluate()
        { new IEcsQuery<'q> with
            member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf(compTypes)
            member _.Fetch(storage) =
                let entityCount = storage.Count
                seq {
                    let fetch = fetch storage
                    for idxEntity in 0 .. entityCount - 1 do
                        yield fetch idxEntity
                }
        }

    [<RequiresExplicitTypeArguments>]
    let mkQuery<'q> () : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        // Single Read Component
        | Shape.EcsReadComponent shape ->
            let compTypes, fetchExpr = shape.Accept({ new IEcsComponentVisitor<_> with
                member _.Visit<'c>() =
                    let compTypes = [ typeof<'c> ]
                    let fetchExpr: Expr<_ -> _ -> EcsReadComponent<'c>> =
                        <@ fun (storage: ArchetypeStorage) ->
                            let col = storage.GetColumn<'c>()
                            fun idxEntity ->
                                { Column = col; Index = idxEntity }
                        @>
                    compTypes, (Expr.Cast fetchExpr)
            })
            mkQuerySingleValue compTypes fetchExpr
        // Single Write Component
        | Shape.EcsWriteComponent shape ->
            let compTypes, fetchExpr = shape.Accept({ new IEcsComponentVisitor<_> with
                member _.Visit<'c>() =
                    let compTypes = [ typeof<'c> ]
                    let fetchExpr: Expr<_ -> _ -> EcsWriteComponent<'c>> =
                        <@ fun (storage: ArchetypeStorage) ->
                            let col = storage.GetColumn<'c>()
                            fun idxEntity ->
                                { Column = col; Index = idxEntity }
                        @>
                    compTypes, (Expr.Cast fetchExpr)
            })
            mkQuerySingleValue compTypes fetchExpr
        // Single EntityId
        | Shape.EcsEntityId ->
            let compTypes = []
            let fetchExpr: Expr<_ -> _ -> EcsEntityId> =
                <@ fun (storage: ArchetypeStorage) ->
                    let eids = storage.Ids
                    fun idxEntity ->
                        eids.[idxEntity]
                @>
            mkQuerySingleValue compTypes (Expr.Cast fetchExpr)
        // Record
        | Shape.FSharpRecord (:? ShapeFSharpRecord<'q> as shapeRecord) ->
            mkQueryTupleOrRecord shapeRecord.Fields (fun items -> Expr.NewRecord(typeof<'q>, items))
        // Tuple
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            let createFromItems =
                if shapeTuple.IsStructTuple then
                    fun items -> Expr.NewStructTuple(typeof<ValueTuple<_>>.Assembly, items)
                else
                    fun items -> Expr.NewTuple(items)
            mkQueryTupleOrRecord shapeTuple.Elements createFromItems

        // Other types
        | _ ->
            raise ^ NotSupportedException($"Type {typeof<'q>} is not supported")

type CompiledEcsQueryFactory() =
    interface IEcsQueryFactory with
        member this.CreateQuery<'q>() =
            EcsQueryCreating.mkQuery<'q> ()
