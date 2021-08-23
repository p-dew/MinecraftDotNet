namespace Ehingeeinae.Ecs.Querying

open System
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


[<Struct>]
type EcsComponent<'comp> =
    internal
        { Pointer: voidptr }

[<RequireQualifiedAccess>]
module VoidPtr =
    let inline isNotNull (p: voidptr) = IntPtr(p) <> IntPtr.Zero

module EcsComponent =

    open System.Runtime.CompilerServices

    let cast (c: 'c byref) : EcsComponent<'c> =
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


[<Struct>]
type EcsQueryFilter =
    | EcsQueryFilter of (EcsArchetype -> bool)
    static member ( + ) (EcsQueryFilter f1, EcsQueryFilter f2) = EcsQueryFilter (fun a -> f1 a || f2 a)
    static member ( * ) (EcsQueryFilter f1, EcsQueryFilter f2) = EcsQueryFilter (fun a -> f1 a && f2 a)
    static member (~- ) (EcsQueryFilter f) = EcsQueryFilter (fun a -> not (f a))
    static member ( <|> ) (EcsQueryFilter f1, EcsQueryFilter f2) = EcsQueryFilter (fun a -> f1 a <> f2 a)

module EcsQueryFilter =

    [<RequiresExplicitTypeArguments>]
    let comp<'c> : EcsQueryFilter = EcsQueryFilter (fun archetype -> archetype.ComponentTypes.Contains(typeof<'c>))


// Based on Rust Amethyst Legion
type IEcsQuery<'CompTuple> =
    abstract Fetch: ArchetypeStorage -> 'CompTuple seq
    abstract Filter: EcsArchetype -> bool

[<RequireQualifiedAccess>]
module EcsQuery =

    open System.Runtime.CompilerServices
    open TypeShape.Core.Core

    [<AutoOpen>]
    module private Utils =

        let assertEqualArrayLengths (arrays: _[] seq) =
            assert (
                let arrays = arrays |> Seq.toArray
                seq { for i in 0 .. arrays.Length - 1 - 1 -> i }
                |> Seq.forall (fun i -> arrays.[i].Length = arrays.[i+1].Length)
            )

        [<RequiresExplicitTypeArguments>]
        let getStorageArray<'c> (storage: ArchetypeStorage) =
            storage.GetColumn<'c>() |> ResizeArray.getItems

        let castComp (arr: ArraySegment<_>) (i: int) =
            let c = &arr.Array.[i]
            let p = Unsafe.AsPointer(&c)
            { Pointer = p }

    let withFilter (filter: EcsQueryFilter) (q: IEcsQuery<'q>) : IEcsQuery<'q> =
        let (EcsQueryFilter filter) = filter
        { new IEcsQuery<'q> with
            member _.Fetch(storage) = q.Fetch(storage)
            member _.Filter(archetype) = q.Filter(archetype) && filter archetype
        }

    [<RequiresExplicitTypeArguments>]
    let query1<'c1> =
        { new IEcsQuery<EcsComponent<'c1>> with
            member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf([typeof<'c1>])
            member _.Fetch(storage) = seq {
                let arr1 = getStorageArray<'c1> storage
                for i in 0 .. arr1.Count - 1 do
                    yield castComp arr1 i
            }
        }

    [<RequiresExplicitTypeArguments>]
    let query2<'c1, 'c2> =
        { new IEcsQuery<struct (EcsComponent<'c1> * EcsComponent<'c2>)> with
            member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf([typeof<'c1>; typeof<'c2>])
            member _.Fetch(storage) = seq {
                let arr1 = getStorageArray<'c1> storage
                let arr2 = getStorageArray<'c2> storage
                assert (arr1.Count = arr2.Count)
                for i in 0 .. arr1.Count - 1 do
                    yield castComp arr1 i, castComp arr2 i
            }
        }

    [<RequiresExplicitTypeArguments>]
    let query3<'c1, 'c2, 'c3> =
        { new IEcsQuery<struct (EcsComponent<'c1> * EcsComponent<'c2> * EcsComponent<'c3>)> with
            member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf([typeof<'c1>; typeof<'c2>; typeof<'c3>])
            member _.Fetch(storage) = seq {
                let arr1 = getStorageArray<'c1> storage
                let arr2 = getStorageArray<'c2> storage
                let arr3 = getStorageArray<'c3> storage
                assert (arr1.Count = arr2.Count && arr2.Count = arr3.Count)
                for i in 0 .. arr1.Count - 1 do
                    yield castComp arr1 i, castComp arr2 i, castComp arr3 i
            }
        }

    // ----

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

    // ----

    type SetTuple<'TTuple> = delegate of 'TTuple byref * ComponentColumn * int -> 'TTuple

    let queryN<'q> : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            let compTypes = shapeTuple.Elements |> Array.map (fun e -> e.Member.Type.GetGenericArguments().[0])
            let fs_setTupleItem: SetTuple<'q>[] =
                shapeTuple.Elements
                |> Array.map (fun shapeItem ->
                    match shapeItem.Member with
                    | Shape.EcsComponent shapeEcsComp ->
                        shapeEcsComp.Accept({ new IEcsComponentVisitor<_> with
                            member _.Visit<'c>() =
                                match shapeItem with
                                | :? ShapeMember<'q, EcsComponent<'c>> as shapeItem ->
                                    SetTuple (fun (q: 'q byref) (col: ComponentColumn) (i: int) ->
                                        let arr = col |> ComponentColumn.unbox<'c> |> ResizeArray.getItems
                                        let c = &arr.Array.[i]
                                        let ec = EcsComponent.cast &c
                                        // TODO: This setter does boxing. Use something else
                                        shapeItem.Set q ec
                                    )
                                | _ ->
                                    failwith "not EcsComp<'c>"
                        })
                    | _ ->
                        failwith "not ShapeEcsComp"
                )
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
            failwith ""


type EcsWorldQueryExecutor(world: EcsWorld) =
    let archetypes = world.Archetypes
    member this.ExecuteQuery(query: IEcsQuery<'q>): 'q seq =
        seq {
            for KeyValue(archetype, storage) in archetypes do
                if not (query.Filter(archetype)) then () else
                let comps = query.Fetch(storage) // TODO: Cache?
                yield! comps
        }


type EcsQueryComponentType<'c> = struct end

type E<'a> = struct end
type D = struct end

module EcsQueryC =
    [<RequiresExplicitTypeArguments>]
    let comp<'c> = EcsQueryComponentType<'c>()

type EcsQueryBuilder() =
    member _.Delay(f) = f ()

    member _.Yield(_: EcsQueryComponentType<'c>)
        : struct(EcsQueryComponentType<'c> * D) = Unchecked.defaultof<_>
    member _.Combine(_: struct(EcsQueryComponentType<'c1> * D), _: struct(EcsQueryComponentType<'c2> * 's))
        : struct(EcsQueryComponentType<struct('c1 * 'c2)> * E<'s>) = Unchecked.defaultof<_>

//    member _.Zero(): struct(EcsQueryComponentType<unit> * D) = Unchecked.defaultof<_>
//    member _.Zero(): 'a = Unchecked.defaultof<_>
//
//    member _.Yield(()) = ()
//
//    [<CustomOperation("comp")>]
//    [<RequiresExplicitTypeArguments>]
//    member _.Comp<'c>(_): struct (EcsQueryComponentType<'c> * D) =
//        Unchecked.defaultof<_>

//    member _.Run<'c1>(_: struct(EcsQueryComponentType<'c1> * D)) = EcsQuery.query1<'c1>
    member _.Run<'c1, 'c2>(_: struct(EcsQueryComponentType<struct('c1 * 'c2)> * E<D>)) =
//        EcsQuery.query2<'c1, 'c2>
        EcsQuery.queryN<struct(EcsComponent<'c1> * EcsComponent<'c2>)>
//    member _.Run<'c1, 'c2, 'c3>(_: struct(EcsQueryComponentType<struct('c1 * struct('c2 * 'c3))> * E<E<D>>)) = EcsQuery.query3<'c1, 'c2, 'c3>
//    member _.Run<'c1, 'c2, 'c3, 'c4 when 'c4 : struct>((c1, (c2, (c3, c4)))): 'c1 * 'c2 * 'c3 * 'c4 = (c1, c2, c3, c4)
    member _.Run(_: struct(_ * E<E<D>>)) =
        raise <| NotSupportedException("Too many component types")

[<AutoOpen>]
module EcsQueryBuilderImpl =
    let ecsQuery = EcsQueryBuilder()
