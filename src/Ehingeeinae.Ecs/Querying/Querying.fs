namespace Ehingeeinae.Ecs.Querying

open System

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds
open TypeShape.Core.Core


// Based on Rust Amethyst Legion
type IEcsQuery<'q> =
    abstract Fetch: ArchetypeStorage -> 'q seq
    abstract Filter: EcsArchetype -> bool


[<Struct>]
type EcsReadComponent<'c> = internal { Pointer: voidptr }

[<Struct>]
type EcsWriteComponent<'c> = internal { Pointer: voidptr }

type 'c cread = EcsReadComponent<'c>
type 'c cwrite = EcsWriteComponent<'c>

module EcsReadComponent =

    open System.Runtime.CompilerServices

    let inline internal cast (c: 'c inref) : EcsReadComponent<'c> =
        let c: 'c byref = &Unsafe.AsRef(&c) // inref to byref
        let p = Unsafe.AsPointer(&c)
        { Pointer = p }

    let getValue (comp: EcsReadComponent<'c>) : 'c inref =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        &Unsafe.AsRef<'c>(vp)


module EcsWriteComponent =

    open System.Runtime.CompilerServices

    let inline internal cast (c: 'c byref) : EcsWriteComponent<'c> =
        let p = Unsafe.AsPointer(&c)
        { Pointer = p }

    let getValue (comp: EcsWriteComponent<'c>) : 'c inref =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        &Unsafe.AsRef<'c>(vp)

    let setValue (comp: EcsWriteComponent<'c>) (value: 'c inref) : unit =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        let p = &Unsafe.AsRef<'c>(vp)
        let value = value // dereference
        p <- value


[<AutoOpen>]
module EcsComponentExtensions =

    type EcsReadComponent<'c> with
        member this.Value = &EcsReadComponent.getValue this

    type EcsWriteComponent<'c> with
        member this.Value
            with get(): 'c inref = &EcsWriteComponent.getValue this
            and set(c: 'c inref) = EcsWriteComponent.setValue this &c

    let foo () =
        let comp: EcsWriteComponent<int> = Unchecked.defaultof<_>
        let x = 1
        comp.Value <- &x
        ()

// --------
// Component Shaping

type IEcsComponentVisitor<'R> =
    abstract Visit<'c> : unit -> 'R

type IShapeEcsComponent =
    abstract Component: TypeShape
    abstract Accept<'R> : IEcsComponentVisitor<'R> -> 'R

type ShapeEcsComponent<'c>() =
    interface IShapeEcsComponent with
        member _.Component = upcast shapeof<'c>
        member this.Accept(v) = v.Visit<'c>()

[<RequireQualifiedAccess>]
module Shape =

    let (|EcsWriteComponent|EcsReadComponent|EcsEntityId|NotEcsComponent|) (shape: TypeShape) =
        match shape.ShapeInfo with
        | TypeShapeInfo.Generic (td, ta) when td = typedefof<EcsReadComponent<_>> ->
            let shapeEcsComponent =
                Activator.CreateInstanceGeneric<ShapeEcsComponent<_>>(ta)
                :?> IShapeEcsComponent
            EcsReadComponent shapeEcsComponent
        | TypeShapeInfo.Generic (td, ta) when td = typedefof<EcsWriteComponent<_>> ->
            let shapeEcsComponent =
                Activator.CreateInstanceGeneric<ShapeEcsComponent<_>>(ta)
                :?> IShapeEcsComponent
            EcsWriteComponent shapeEcsComponent
        | _ when (match shape with :? TypeShape<EcsEntityId> -> true | _ -> false) ->
            EcsEntityId
        | _ -> NotEcsComponent


[<RequireQualifiedAccess>]
module Expr =

    open FSharp.Quotations

    let lambdaMany (parameters: Var list) (body: Expr) : Expr =
        let rec foo parameters =
            match parameters with
            | [] -> invalidOp ""
            | parameter :: ((_ :: _) as tail) ->
                Expr.Lambda(parameter, foo tail)
            | [parameter] -> Expr.Lambda(parameter, body)
        foo parameters

    let letMany (vars: (Var * Expr) list) (body: Expr) : Expr =
        let rec foo vars =
            match vars with
            | [] -> invalidOp ""
            | [(var, value)] -> Expr.Let(var, value, body)
            | (var, value) :: tail ->
                Expr.Let(var, value, foo tail)
        foo vars


module EcsQueryCreating =

    open FSharp.Quotations
    open FSharp.Quotations.Evaluator
    open Microsoft.FSharp.Reflection

    type FetchF<'r> = ArchetypeStorage -> int -> 'r

    let itemAsReadComponent (arr: 'c[]) i = EcsReadComponent.cast &arr.[i]
    let itemAsWriteComponent (arr: 'c[]) i = EcsWriteComponent.cast &arr.[i]

    // let foo (shape: TypeShape<'q>) (storageExpr: Expr<ArchetypeStorage>) : Type list * Expr =
    //     match shape with
    //     | Shape.EcsReadComponent shape ->
    //         shape.Accept({ new IEcsComponentVisitor<_> with
    //             member _.Visit<'c>() =
    //                 let fetchExpr: Expr<int -> EcsReadComponent<'c>> =
    //                     <@
    //                         let storage = %storageExpr
    //                         let col = storage.GetColumn<'c>()
    //                         let arr = col.Components |> ResizeArray.getItems
    //                         fun idxEntity ->
    //                             itemAsReadComponent arr.Array idxEntity
    //                     @>
    //                 [typeof<'c>], upcast fetchExpr
    //         })
    //     | Shape.EcsWriteComponent shape ->
    //         shape.Accept({ new IEcsComponentVisitor<_> with
    //             member _.Visit<'c>() =
    //                 let fetchExpr: Expr<int -> EcsWriteComponent<'c>> =
    //                     <@
    //                         let storage = %storageExpr
    //                         let col = storage.GetColumn<'c>()
    //                         let arr = col.Components |> ResizeArray.getItems
    //                         fun idxEntity ->
    //                             itemAsWriteComponent arr.Array idxEntity
    //                     @>
    //                 [typeof<'c>], upcast fetchExpr
    //         })
    //     // Single EntityId
    //     | Shape.EcsEntityId ->
    //         let fetchExpr: Expr<int -> EcsEntityId> =
    //             <@
    //                 let storage = %storageExpr
    //                 let eids = storage.Ids
    //                 fun idxEntity ->
    //                     eids.[idxEntity]
    //             @>
    //         [], upcast fetchExpr
    //     | _ ->
    //         failwith ""

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
            let storageVar = Var("storage", typeof<ArchetypeStorage>)
            Expr.lambdaMany [storageVar] (
                let storage = Expr.Var(storageVar) |> Expr.Cast<ArchetypeStorage>
                let itemFs = [
                    for idxItem in 0 .. itemCount - 1 do
                        let shapeItem = shapeMembers.[idxItem]
                        let var = Var($"item{idxItem}", FSharpType.MakeFunctionType(typeof<int>, shapeItem.Member.Type))
                        var,
                        match shapeItem.Member with
                        | Shape.EcsEntityId ->
                            <@@
                                let storage = %storage
                                let eids = storage.Ids
                                fun idxEntity ->
                                    eids.[idxEntity]
                            @@>
                        | Shape.EcsReadComponent shape ->
                            shape.Accept({ new IEcsComponentVisitor<_> with
                                member _.Visit<'c>() =
                                    <@@
                                        let storage = %storage
                                        let col = storage.GetColumn<'c>()
                                        let arr = col.Components |> ResizeArray.getItems
                                        fun idxEntity ->
                                            itemAsReadComponent arr.Array idxEntity
                                    @@>
                            })
                        | Shape.EcsWriteComponent shape ->
                            shape.Accept({ new IEcsComponentVisitor<_> with
                                member _.Visit<'c>() =
                                    <@@
                                        let storage = %storage
                                        let col = storage.GetColumn<'c>()
                                        let arr = col.Components |> ResizeArray.getItems
                                        fun idxEntity ->
                                            itemAsWriteComponent arr.Array idxEntity
                                    @@>
                            })
                        | _ ->
                            invalidOp $"Type (at {idxItem} position) is not supported as query element"
                ]
                Expr.letMany itemFs (
                    let itemFVars = itemFs |> List.map fst
                    let idxEntityVar = Var("idxEntity", typeof<int>)
                    Expr.Lambda(
                        idxEntityVar,
                        let idxEntity = Expr.Var(idxEntityVar) |> Expr.Cast<int>
                        let items = itemFVars |> List.map (fun itemFVar -> Expr.Application(Expr.Var(itemFVar), idxEntity))
                        createFromItems items
                    )
                )
            )
            |> Expr.Cast

        printfn $"fetchExpr:\n%A{fetchExpr}"

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
                            let arr = col.Components |> ResizeArray.getItems
                            fun idxEntity ->
                                itemAsReadComponent arr.Array idxEntity
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
                            let arr = col.Components |> ResizeArray.getItems
                            fun idxEntity ->
                                itemAsWriteComponent arr.Array idxEntity
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
            mkQueryTupleOrRecord shapeTuple.Elements (fun items ->
                if shapeTuple.IsStructTuple then
                    Expr.NewStructTuple(typeof<ValueTuple<_>>.Assembly, items)
                else
                    Expr.NewTuple(items)
            )
        // Other types
        | _ ->
            raise ^ NotSupportedException($"Type {typeof<'q>} is not supported")
