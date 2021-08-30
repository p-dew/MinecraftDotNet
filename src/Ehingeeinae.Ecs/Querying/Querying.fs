namespace Ehingeeinae.Ecs.Querying

open System

open TypeShape.Core

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


//[<Struct>]
//type EcsComponent<'comp> =
//    internal
//        { Pointer: voidptr }

//type IEcsComponent<'c> =
//    internal abstract Pointer: voidptr

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

    type SetTuple<'TTuple> = delegate of 'TTuple byref * IComponentColumn * int -> 'TTuple


[<RequireQualifiedAccess>]
module EcsQuery =

    open System
    open System.Reflection
    open System.Reflection.Emit
    open System.Runtime.CompilerServices
    open FSharp.Quotations
    open FSharp.Quotations.Evaluator
    open System.Linq.Expressions
    open Microsoft.FSharp.Reflection

    type internal Marker = class end

    let getComp (cols: IComponentColumn[]) (idxCol: int) (idxEntity: int) : EcsReadComponent<'c> =
        let col = cols.[idxCol]
        let col = col |> ComponentColumn.unbox<'c>
        let arr = col.Components |> ResizeArray.getItems
        let ca = &arr.Array.[idxEntity]
        EcsReadComponent.cast &ca

    [<RequiresExplicitTypeArguments>]
    let query<'q> : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            // let fs_setTupleItem: SetTuple<'q>[] =
            //     shapeTuple.Elements
            //     |> Array.map (fun shapeElement ->
            //         match shapeElement.Member with
            //         | Shape.EcsWriteComponent shapeEcsComp ->
            //             shapeEcsComp.Accept({ new IEcsComponentVisitor<_> with
            //                 member _.Visit<'c>() =
            //                     let shapeElement = shapeElement :?> ShapeMember<'q, EcsWriteComponent<'c>> // This cast is covered by the above active pattern
            //                     SetTuple (fun (q: 'q byref) (col: IComponentColumn) (i: int) ->
            //                         let arr = col |> ComponentColumn.unbox<'c> |> fun col -> ResizeArray.getItems col.Components
            //                         let c = &arr.Array.[i]
            //                         let ec = EcsWriteComponent.cast &c
            //                         // TODO: This setter does boxing. Use something else
            //                         shapeElement.Set q ec
            //                     )
            //             })
            //         | Shape.EcsReadComponent shapeEcsComp ->
            //             shapeEcsComp.Accept({ new IEcsComponentVisitor<_> with
            //                 member _.Visit<'c>() =
            //                     let shapeElement = shapeElement :?> ShapeMember<'q, EcsReadComponent<'c>> // This cast is covered by the above active pattern
            //                     SetTuple (fun (q: 'q byref) (col: IComponentColumn) (i: int) ->
            //                         let arr = col |> ComponentColumn.unbox<'c> |> fun col -> ResizeArray.getItems col.Components
            //                         let c = &arr.Array.[i]
            //                         let ec = EcsReadComponent.cast &c
            //                         // TODO: This setter does boxing. Use something else
            //                         shapeElement.Set q ec
            //                     )
            //             })
            //         | Shape.EcsEntityId ->
            //             raise ^ NotImplementedException()
            //         | _ ->
            //             failwith "not ShapeEcsComp"
            //     )

            let cTypes = shapeTuple.Elements |> Array.map (fun e -> e.Member.Type.GetGenericArguments().[0])
            let qcTypes = FSharpType.GetTupleElements(typeof<'q>)
            let colCount = cTypes.Length

            let mid_getComp = typeof<Marker>.DeclaringType.GetMethod("getComp")

            let getQExpr: Expr<IComponentColumn[] -> int -> 'q> =
                let colsVar = Var("cols", typeof<IComponentColumn[]>)
                let cols = Expr.Var(colsVar) |> Expr.Cast<IComponentColumn[]>
                Expr.Lambda(
                    colsVar,
                    let idxEntityVar = Var("idxEntity", typeof<int>)
                    let idxEntity = Expr.Var(idxEntityVar) |> Expr.Cast<int>
                    Expr.Lambda(
                        idxEntityVar,
                        // ----
                        let items = [
                            for idxCol in 0 .. colCount - 1 do
                                let cType = cTypes.[idxCol]
                                let mi_getComp = mid_getComp.MakeGenericMethod(cType)
                                let qc = Expr.Call(mi_getComp, [cols; Expr.Value(idxCol); idxEntity])
                                qc
                        ]
                        Expr.NewTuple(items)
                        // ----
                    )
                )
                |> Expr.Cast

            // let method =
            //     DynamicMethod(
            //         "CreateQueryResultFromColumnsAndEIdx",
            //         typeof<'q>, [| typeof<IComponentColumn[]>; typeof<int> |],
            //         restrictedSkipVisibility=true
            //     )
            // let il = method.GetILGenerator()
            //
            // let locals = [
            //     for iCol in 0 .. qcTypes.Length - 1 do
            //         let qcType = qcTypes.[iCol]
            //         let cType = qcType.GetGenericArguments().[0]
            //         let componentColumnType = typedefof<ComponentColumn<_>>.MakeGenericType(cType)
            //
            //         let rarrField = componentColumnType.GetField("rarr", BindingFlags.NonPublic ||| BindingFlags.Instance)
            //         assert (rarrField <> null)
            //         let itemsField = typedefof<ResizeArray<_>>.MakeGenericType(qcType).GetField("_items", BindingFlags.NonPublic ||| BindingFlags.Instance)
            //         assert (itemsField <> null)
            //
            //         // (let) arr = (cols.[iCol] :> ComponentColumn<cType>).Components._items
            //         il.Emit(OpCodes.Ldarg_0)
            //         il.EmitWriteLine("ldarg.0")
            //         il.Emit(OpCodes.Ldc_I4, iCol)
            //         il.EmitWriteLine("ldc.i4 iCol")
            //         il.Emit(OpCodes.Ldelem_I4)
            //         il.EmitWriteLine("ldelem.i4")
            //         // il.Emit(OpCodes.Castclass, componentColumnType)
            //         // il.EmitWriteLine("castclass componentColumnType")
            //         il.Emit(OpCodes.Ldfld, rarrField)
            //         il.EmitWriteLine("ldfld rarrField")
            //         il.Emit(OpCodes.Ldfld, itemsField)
            //         il.EmitWriteLine("ldfld itemsField")
            //
            //         // let vp = (void*) ref arr.[idxEntity]
            //         il.Emit(OpCodes.Ldarg_1)
            //         il.Emit(OpCodes.Ldelema, cType)
            //         il.Emit(OpCodes.Conv_U)
            //         let localVp = il.DeclareLocal(typeof<voidptr>)
            //         il.Emit(OpCodes.Stloc, localVp)
            //
            //         // let qc = *(QC*)&vp
            //         il.Emit(OpCodes.Ldloca, localVp)
            //         il.Emit(OpCodes.Conv_U)
            //         il.Emit(OpCodes.Ldobj, qcType)
            //         let localQc = il.DeclareLocal(qcType)
            //         il.Emit(OpCodes.Stloc, localQc)
            //
            //         yield localQc
            // ]
            // for local in locals do
            //     il.Emit(OpCodes.Ldloc, local)
            //
            // let qCtor = typeof<'q>.GetConstructor(qcTypes) // TODO: Tuple.Length > 7
            // il.Emit(OpCodes.Newobj, qCtor)
            // il.Emit(OpCodes.Ret)
            //
            // let deleg = method.CreateDelegate<Func<IComponentColumn[], int, 'q>>()

            { new IEcsQuery<'q> with
                member _.Filter(archetype) =
                    archetype.ComponentTypes.IsSupersetOf(cTypes)
                member _.Fetch(storage) =
                    let cols = cTypes |> Array.map storage.GetColumn
                    let entityCount = storage.Count
                    let getQ = getQExpr.Evaluate()
                    seq {
                        for idxEntity in 0 .. entityCount - 1 do
                            yield getQ cols idxEntity
                            // let mutable tupleInstance = shapeTuple.CreateUninitialized()
                            // for idxCol in 0 .. cols.Length - 1 do
                            //     let setTupleItem = fs_setTupleItem.[idxCol]
                            //     let col = cols.[idxCol]
                            //     tupleInstance <- setTupleItem.Invoke(&tupleInstance, col, idxEntity)
                            // yield tupleInstance
                    }
            }
        | _ ->
            raise <| NotSupportedException($"Type '{typeof<'q>}' is not supported as EcsQuery representation")
