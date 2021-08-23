namespace Ehingeeinae.Ecs.Querying

open System

open TypeShape.Core

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


[<Struct>]
type EcsComponent<'comp> =
    internal
        { Pointer: voidptr }

module EcsComponent =

    open System.Runtime.CompilerServices

    let internal cast (c: 'c byref) : EcsComponent<'c> =
        let p = Unsafe.AsPointer(&c)
        { Pointer = p }

    let getValue (comp: EcsComponent<'c>) : 'c inref =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        &Unsafe.AsRef<'c>(vp)

    let updateValue (comp: EcsComponent<'c>) (value: 'c inref) : unit =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        let p = &Unsafe.AsRef<'c>(vp)
        let value = value
        p <- value

[<AutoOpen>]
module EcsComponentExtensions =
    type EcsComponent<'c> with
        member this.Value = EcsComponent.getValue this


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
        let (|EcsComponent|_|) (shape: TypeShape) =
            match shape.ShapeInfo with
            | TypeShapeInfo.Generic (td, ta) when td = typedefof<EcsComponent<_>> ->
                Activator.CreateInstanceGeneric<ShapeEcsComponent<_>>(ta)
                :?> IShapeEcsComponent
                |> Some
            | _ -> None

    type SetTuple<'TTuple> = delegate of 'TTuple byref * ComponentColumn * int -> 'TTuple


[<RequireQualifiedAccess>]
module EcsQuery =

    let query<'q> : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            let fs_setTupleItem: SetTuple<'q>[] =
                shapeTuple.Elements
                |> Array.map (fun shapeElement ->
                    match shapeElement.Member with
                    | Shape.EcsComponent shapeEcsComp ->
                        shapeEcsComp.Accept({ new IEcsComponentVisitor<_> with
                            member _.Visit<'c>() =
                                match shapeElement with
                                | :? ShapeMember<'q, EcsComponent<'c>> as shapeElement ->
                                    SetTuple (fun (q: 'q byref) (col: ComponentColumn) (i: int) ->
                                        let arr = col |> ComponentColumn.unbox<'c> |> ResizeArray.getItems
                                        let c = &arr.Array.[i]
                                        let ec = EcsComponent.cast &c
                                        // TODO: This setter does boxing. Use something else
                                        shapeElement.Set q ec
                                    )
                                | _ ->
                                    failwith "not EcsComp<'c>"
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
