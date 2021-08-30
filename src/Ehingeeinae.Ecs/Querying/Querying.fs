namespace Ehingeeinae.Ecs.Querying

open System

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


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


// Based on Rust Amethyst Legion
type IEcsQuery<'q> =
    abstract Fetch: ArchetypeStorage -> 'q seq
    abstract Filter: EcsArchetype -> bool


[<AutoOpen>]
module private Utils =

    open TypeShape.Core

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


[<RequireQualifiedAccess>]
module EcsQuery =

    open System.Reflection

    open FSharp.Quotations
    open FSharp.Quotations.Evaluator
    open Microsoft.FSharp.Reflection
    open TypeShape.Core

    type internal Marker = class end

    let private getComponentRef (cols: IComponentColumn[]) (idxCol: int) (idxEntity: int) : 'c byref =
        let col = cols.[idxCol]
        let col = col |> ComponentColumn.unbox<'c>
        let arr = col.Components |> ResizeArray.getItems
        &arr.Array.[idxEntity]

    [<RequiresExplicitTypeArguments>]
    let getReadComponent (cols: IComponentColumn[]) (idxCol: int) (idxEntity: int) : EcsReadComponent<'c> =
        let ca = &getComponentRef cols idxCol idxEntity
        EcsReadComponent.cast &ca

    [<RequiresExplicitTypeArguments>]
    let getWriteComponent (cols: IComponentColumn[]) (idxCol: int) (idxEntity: int) : EcsWriteComponent<'c> =
        let ca = &getComponentRef cols idxCol idxEntity
        EcsWriteComponent.cast &ca

    let getEid (eids: ArraySegment<EcsEntityId>) (idxEntity: int) : EcsEntityId =
        eids.[idxEntity]

    [<RequiresExplicitTypeArguments>]
    let query<'q> : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            let cTypes = shapeTuple.Elements |> Array.map (fun e -> e.Member.Type.GetGenericArguments().[0])
            let compTypes =
                shapeTuple.Elements
                |> Array.collect ^fun shapeItem ->
                    match shapeItem.Member with
                    | Shape.EcsReadComponent _ | Shape.EcsWriteComponent _ -> [| shapeItem.Member.Type |]
                    | _ -> [| |]
            let qcTypes = FSharpType.GetTupleElements(typeof<'q>)
            let itemCount = shapeTuple.Elements.Length

            let resolveQueryExpr: Expr<ArraySegment<EcsEntityId> -> IComponentColumn[] -> int -> 'q> =
                // Из-за того, что каждая ветка в этой лямбде возвращает разный (в зависимости от кортежа 'q) тип,
                // чтобы обратно это можно было сконструировать, надо использовать quotations
                let eidsVar = Var("eids", typeof<ArraySegment<EcsEntityId>>)
                let colsVar = Var("cols", typeof<IComponentColumn[]>)
                let idxEntityVar = Var("idxEntity", typeof<int>)
                Expr.lambdaMany [eidsVar; colsVar; idxEntityVar] (
                    let eids = Expr.Var(eidsVar)
                    let cols = Expr.Var(colsVar) |> Expr.Cast<IComponentColumn[]>
                    let idxEntity = Expr.Var(idxEntityVar) |> Expr.Cast<int>
                    let items = [
                        for idxCol in 0 .. itemCount - 1 do
                            let shapeItem = shapeTuple.Elements.[idxCol]
                            match shapeItem.Member with
                            | Shape.EcsEntityId ->
                                <@@ getEid %%eids %idxEntity @@>
                            | Shape.EcsReadComponent shape ->
                                shape.Accept({ new IEcsComponentVisitor<_> with
                                    member _.Visit<'c>() =
                                        <@@ getReadComponent<'c> %cols idxCol %idxEntity @@>
                                })
                            | Shape.EcsWriteComponent shape ->
                                shape.Accept({ new IEcsComponentVisitor<_> with
                                    member _.Visit<'c>() =
                                        <@@ getWriteComponent<'c> %cols idxCol %idxEntity @@>
                                })
                            | _ ->
                                invalidOp $"Type {qcTypes.[idxCol]} (at {idxCol} position) is not supported as query element"
                    ]
                    if shapeTuple.IsStructTuple then
                        let asm = Assembly.GetExecutingAssembly() // TODO: Which assembly should be used?
                        Expr.NewStructTuple(asm, items)
                    else
                        Expr.NewTuple(items)
                )
                |> Expr.Cast

            let getQ: ArraySegment<EcsEntityId> -> IComponentColumn[] -> int -> 'q =
                // downcast (resolveQueryExpr |> FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.EvaluateQuotation)
                resolveQueryExpr.Evaluate()

            // FIXME: EcsEntityId in 'q, related with cTypes and indices
            { new IEcsQuery<'q> with
                member _.Filter(archetype) =
                    archetype.ComponentTypes.IsSupersetOf(compTypes)
                member _.Fetch(storage) =
                    let cols = cTypes |> Array.map storage.GetColumn
                    let eids = storage.Ids |> ResizeArray.getItems
                    let entityCount = storage.Count
                    seq {
                        for idxEntity in 0 .. entityCount - 1 do
                            yield getQ eids cols idxEntity
                    }
            }
        // Single value
        | _ ->
            raise <| NotSupportedException($"Type '{typeof<'q>}' is not supported as EcsQuery representation")
