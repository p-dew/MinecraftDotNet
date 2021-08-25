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

//module EcsComponent =
//
//    open System.Runtime.CompilerServices
//
//    let internal cast (c: 'c byref) : EcsComponent<'c> =
//        let p = Unsafe.AsPointer(&c)
//        { Pointer = p }
//
//    let getValue (comp: EcsComponent<'c>) : 'c inref =
//        let vp = comp.Pointer
//        assert (VoidPtr.isNotNull vp)
//        &Unsafe.AsRef<'c>(vp)
//
//    let updateValue (comp: EcsComponent<'c>) (value: 'c inref) : unit =
//        let vp = comp.Pointer
//        assert (VoidPtr.isNotNull vp)
//        let p = &Unsafe.AsRef<'c>(vp)
//        let value = value
//        p <- value

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
        let (|EcsWriteComponent|EcsReadComponent|NotEcsComponent|) (shape: TypeShape) =
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
            | _ -> NotEcsComponent

    type SetTuple<'TTuple> = delegate of 'TTuple byref * IComponentColumn * int -> 'TTuple


[<RequireQualifiedAccess>]
module EcsQuery =

    [<RequiresExplicitTypeArguments>]
    let query<'q> : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            let fs_setTupleItem: SetTuple<'q>[] =
                shapeTuple.Elements
                |> Array.map (fun shapeElement ->
                    match shapeElement.Member with
                    | Shape.EcsWriteComponent shapeEcsComp ->
                        shapeEcsComp.Accept({ new IEcsComponentVisitor<_> with
                            member _.Visit<'c>() =
                                let shapeElement = shapeElement :?> ShapeMember<'q, EcsWriteComponent<'c>> // This cast is covered by the above active pattern
                                SetTuple (fun (q: 'q byref) (col: IComponentColumn) (i: int) ->
                                    let arr = col |> ComponentColumn.unbox<'c> |> fun col -> ResizeArray.getItems col.ResizeArray
                                    let c = &arr.Array.[i]
                                    let ec = EcsWriteComponent.cast &c
                                    // TODO: This setter does boxing. Use something else
                                    shapeElement.Set q ec
                                )
                        })
                    | Shape.EcsReadComponent shapeEcsComp ->
                        shapeEcsComp.Accept({ new IEcsComponentVisitor<_> with
                            member _.Visit<'c>() =
                                let shapeElement = shapeElement :?> ShapeMember<'q, EcsReadComponent<'c>> // This cast is covered by the above active pattern
                                SetTuple (fun (q: 'q byref) (col: IComponentColumn) (i: int) ->
                                    let arr = col |> ComponentColumn.unbox<'c> |> fun col -> ResizeArray.getItems col.ResizeArray
                                    let c = &arr.Array.[i]
                                    let ec = EcsReadComponent.cast &c
                                    // TODO: This setter does boxing. Use something else
                                    shapeElement.Set q ec
                                )
                        })
                    | _ ->
                        failwith "not ShapeEcsComp"
                )
            let compTypes = shapeTuple.Elements |> Array.map (fun e -> e.Member.Type.GetGenericArguments().[0])
            { new IEcsQuery<'q> with
                member _.Filter(archetype) =
                    archetype.ComponentTypes.IsSupersetOf(compTypes)
                member _.Fetch(storage) =
                    let cols = compTypes |> Array.map storage.GetColumn
                    seq {
                        for i in 0 .. storage.Count - 1 do
                            let mutable tupleInstance = shapeTuple.CreateUninitialized()
                            for j in 0 .. cols.Length - 1 do
                                let setTupleItem = fs_setTupleItem.[j]
                                let col = cols.[j]
                                tupleInstance <- setTupleItem.Invoke(&tupleInstance, col, i)
                            yield tupleInstance
                    }
            }
        | _ ->
            raise <| NotSupportedException($"Type '{typeof<'q>}' is not supported as EcsQuery representation")
