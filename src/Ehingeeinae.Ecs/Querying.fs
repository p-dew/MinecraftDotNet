namespace Ehingeeinae.Ecs.Querying

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


[<Struct>]
type EcsComponent<'comp> =
    internal
        { Pointer: voidptr }

module EcsComponent =

    open System
    open System.Runtime.CompilerServices

    let getValue (comp: EcsComponent<'c>) : 'c =
        let p = &Unsafe.AsRef(comp.Pointer)
        p

    let updateValue (comp: EcsComponent<'c>) (value: 'c inref) : unit =
        let vp = comp.Pointer
        assert (vp <> IntPtr.Zero.ToPointer())
        let p = &Unsafe.AsRef<'c>(vp)
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


type IEcsQuery<'CompTuple> =
    abstract Fetch: ArchetypeStorage -> 'CompTuple seq
    abstract Filter: EcsArchetype -> bool

[<RequireQualifiedAccess>]
module EcsQuery =

    open System.Runtime.CompilerServices
    open TypeShape.Core.Core

    [<AutoOpen>]
    module private Utils =

        open System

        type Unit' = struct end
        let unit' = Unit'()

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

    let queryN<'q> : IEcsQuery<'q> =
        let shape = shapeof<'q>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'q> as shapeTuple) ->
            let compTypes = shapeTuple.Elements |> Seq.map (fun e -> e.Member.Type)
            { new IEcsQuery<'q> with
                member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf(compTypes)
                member _.Fetch(storage) =
                    shapeTuple.Elements
                    |> Seq.iter (fun e ->
                        e.Accept({ new IMemberVisitor<'q, Unit'> with
                            member _.Visit(shapeItem: ShapeMember<_, 'c>) =
                                let col = storage.GetColumn<'c>()
                                let arr = ResizeArray.getItems col
                                shapeTuple.CreateUninitialized()
                                unit'
                        }) |> ignore
                    )
                    ()
            }
        | _ ->
            failwith ""

    // ecsQuery { comp<A>; comp<B> }

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
//type EcsQueryComponentDummy = struct end

type E<'a> = struct end
type D = struct end

module EcsQueryC =
    [<RequiresExplicitTypeArguments>]
    let comp<'c> = EcsQueryComponentType<'c>()

type EcsQueryBuilder() =
    member _.Delay(f) = f ()

    member _.Yield(c: EcsQueryComponentType<'c>): EcsQueryComponentType<'c> * D = EcsQueryComponentType(), D()
    member _.Combine(_: EcsQueryComponentType<'c1> * D, c2: EcsQueryComponentType<'c2> * 's)
        : EcsQueryComponentType<'c1 * 'c2> * E<'s>
        = EcsQueryComponentType(), E()

//    member _.Run<'c1 when 'c1 : struct>(q: 'c1) = q
//    member _.Run<'c1, 'c2 when 'c2 : struct>(q: 'c1 * 'c2) = q
//    member _.Run<'c1, 'c2, 'c3 when 'c3 : struct>((c1, (c2, c3))): 'c1 * 'c2 * 'c3 = (c1, c2, c3)
//    member _.Run<'c1, 'c2, 'c3, 'c4 when 'c4 : struct>((c1, (c2, (c3, c4)))): 'c1 * 'c2 * 'c3 * 'c4 = (c1, c2, c3, c4)

[<AutoOpen>]
module EcsQueryBuilderImpl =
    let ecsQuery = EcsQueryBuilder()


module pg =

    let foo () =
        let q = ecsQuery {
            EcsQueryC.comp<int8>
            EcsQueryC.comp<int16>
            EcsQueryC.comp<int32>
        }

        ()
