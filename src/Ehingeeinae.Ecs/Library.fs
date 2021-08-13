namespace Ehingeeinae.Ecs

open System
open System.Collections.Generic


module ResizeArray =

    open System
    open System.Reflection
    open System.Collections.Generic

    let private cache: Dictionary<Type, FieldInfo> = Dictionary()

    let getItems (rarr: ResizeArray<'a>) : ArraySegment<'a> =
        let field =
            match cache.TryGetValue(typeof<'a>) with
            | true, field -> field
            | false, _ ->
                let field = typeof<ResizeArray<'a>>.GetField("_items", BindingFlags.NonPublic ||| BindingFlags.Instance)
                cache.[typeof<'a>] <- field
                field
        let _items: 'a[] = downcast field.GetValue(rarr)
        ArraySegment(_items, 0, rarr.Count)


type ByRefAction<'T> = delegate of 'T byref -> unit
type ByRefAction<'T1, 'T2> = delegate of 'T1 byref * 'T2 byref -> unit

module ArraySeq =

    let iter1 (chunks: 'a[] seq) (f: ByRefAction<'a>) : unit =
        chunks
        |> Seq.iter (fun arr ->
            for i in 0 .. arr.Length - 1 do
                let mutable x = &arr.[i]
                f.Invoke(&x)
        )

    let iter2 (f: ByRefAction<'a, 'b>) (chunks: (Memory<'a> * Memory<'b>) seq) : unit =
        chunks
        |> Seq.iter (fun (arr1, arr2) ->
            assert (arr1.Length = arr2.Length)
            let span1 = arr1.Span
            let span2 = arr2.Span
            for i in 0 .. arr1.Length - 1 do
                let mutable x1 = &span1.[i]
                let mutable x2 = &span2.[i]
                f.Invoke(&x1, &x2)
        )




[<Struct>]
type EcsEntityId = EcsEntityId of uint64

type EcsWorldEntity =
    { Id: EcsEntityId
      Components: obj seq }


type EcsArchetype =
    { ComponentTypes: ISet<Type> }

module EcsArchetype =
    let inline createSeq (types: Type seq) = { ComponentTypes = HashSet(types) }
    let inline create1<'c0> () = createSeq [ typeof<'c0> ]
    let inline create2<'c0, 'c1> () = createSeq [ typeof<'c0>; typeof<'c1> ]


(*

Id | ComponentColumns
---|---------------------
 0 | [ cA0; cA1; cA2 ]
 1 | [ cB0; cB1; cB2 ]
 2 | [ cC0; cC1; cC2 ]


   | ComponentColumns
Id |-----------------------
   | CompA | CompB | CompB
----------------------------
 0 |   cA0 |   cB0 |   cC0 |
 1 |   cA1 |   cB1 |   cC1 |
 2 |   cA2 |   cB2 |   cC2 |

*)

/// ResizeArray<'comp>
type ComponentColumn = obj

module ComponentColumn =
    // TODO: Rename
    let inline unbox<'comp> (col: ComponentColumn) : ResizeArray<'comp> =
        assert (match col with :? ResizeArray<'comp> -> true | _ -> false)
        downcast col

    let inline create1<'c0> () : ComponentColumn[] =
        [| ResizeArray<'c0>() |]
    let inline create2<'c0, 'c1> () : ComponentColumn[] =
        [| ResizeArray<'c0>(); ResizeArray<'c1>() |]


type ArchetypeStorage =
    { Ids: ResizeArray<EcsEntityId>
      ComponentColumns: ComponentColumn[] }

    /// <returns>ResizeArray&lt;compType&gt;</returns>
    member this.GetColumn(compType: Type): ComponentColumn =
        this.ComponentColumns
        |> Array.pick (fun col ->
            assert (col.GetType().GetGenericTypeDefinition() = typedefof<ResizeArray<_>>)
            if col.GetType().GetGenericArguments().[0] = compType then
                Some col
            else
                None
        )
    member this.GetColumn<'c>(): ResizeArray<'c> =
        this.ComponentColumns
        |> Array.pick (function
            | :? ResizeArray<'c> as col -> Some col
            | _ -> None
        )
    member this.Add1(eid, comp0: 'c0) =
        this.Ids.Add(eid)
        this.GetColumn<'c0>().Add(comp0)
    member this.Add2(eid, comp0: 'c0, comp1: 'c1) =
        this.Ids.Add(eid)
        this.GetColumn<'c0>().Add(comp0)
        this.GetColumn<'c1>().Add(comp1)


type EcsWorld =
    { Archetypes: IDictionary<EcsArchetype, ArchetypeStorage> }

type ArchetypeQuery =
    | HasComponentType of Type
    | HasNoComponentType of Type
    | And of ArchetypeQuery * ArchetypeQuery
    | Or of ArchetypeQuery * ArchetypeQuery

type EcsWorldManager(world: EcsWorld) =
    let archetypes = world.Archetypes

    let mutable lastEid = 0UL
    let createNextEid () =
        let newEid = lastEid + 1UL
        lastEid <- newEid
        EcsEntityId newEid

    let getStorage (createColumns: unit -> ComponentColumn[]) archetype =
        match archetypes.TryGetValue(archetype) with
        | false, _ ->
            let columns: ComponentColumn[] = createColumns ()
            let storage = { Ids = ResizeArray(); ComponentColumns = columns }
            archetypes.[archetype] <- storage
            storage
        | true, storage -> storage

    let getColumns (types: Type seq) : ComponentColumn[] seq =
        seq {
            for KeyValue (archetype, storage) in archetypes do
                if archetype.ComponentTypes.IsSupersetOf(types) then
                    let cols = types |> Seq.map storage.GetColumn |> Seq.toArray
                    yield cols
        }

//    let selectQuery (aq: ArchetypeQuery) =
//        let rec selectQuery (aq: ArchetypeQuery) (archetypes: EcsArchetype seq) =
//            match aq with
//            | ArchetypeQuery.HasComponentType t ->
//                archetypes |> Seq.where (fun a -> a.ComponentTypes.Contains(t))
//            | ArchetypeQuery.And (aq1, aq2) ->
//                ()
//        selectQuery aq archetypes.Keys

    member this.AddEntity1(comp0: 'c0) =
        let archetype = EcsArchetype.create1<'c0> ()
        let storage = getStorage ComponentColumn.create1<'c0> archetype
        let eid = createNextEid ()
        storage.Add1(eid, comp0)
        eid

    member this.AddEntity2(comp0: 'c0, comp1: 'c1) =
        let archetype = EcsArchetype.create2<'c0, 'c1> ()
        let storage = getStorage ComponentColumn.create2<'c0, 'c1> archetype
        let eid = createNextEid ()
        storage.Add2(eid, comp0, comp1)
        eid

    // ----

    member this.QueryComponent1<'c0>(): (ArraySegment<'c0>) seq =
        getColumns [typeof<'c0>]
        |> Seq.map (fun cols ->
            let col0 = cols.[0] |> ComponentColumn.unbox<'c0>
            ResizeArray.getItems col0
        )

    member this.QueryComponent2<'c0, 'c1>(): (ArraySegment<'c0> * ArraySegment<'c1>) seq =
        getColumns [typeof<'c0>; typeof<'c1>]
        |> Seq.map (fun cols ->
            let col0 = cols.[0] |> ComponentColumn.unbox<'c0>
            let col1 = cols.[1] |> ComponentColumn.unbox<'c1>
            ResizeArray.getItems col0, ResizeArray.getItems col1
        )


[<Struct>]
type EcsComponent<'comp when 'comp : unmanaged> =
    internal
        { Pointer: nativeptr<'comp> }


module rec a =

    open System.Numerics

    type IComponentGetter = abstract Get<'c when 'c : unmanaged> : unit -> EcsComponent<'c>

    type QAnd<'q1, 'q2, 'a> = abstract Map: 'q1 * 'q2 -> 'a
    type QOr<'q1, 'q2, 'a> = abstract Map: Choice<'q1, 'q2> -> 'a
    type QNot<'c> = struct end
    type QComp<'c> = struct end

    module EcsQuery =
        let qComp<'c> = QComp<'c>()
        let qAnd f = { new QAnd<'q1, 'q2, 'a> with member _.Map(q1, q2) = f (q1, q2) }
        let qOr f = { new QOr<'q1, 'q2, 'a> with member _.Map(e) = f e }


//        let notComp<'c when 'c : unmanaged> () : EcsQuery<unit> =
//            failwith ""


    [<Struct>]
    type Position = { Position: Vector2 }

    [<Struct>]
    type Velocity = { Velocity: Vector2 }

    [<Struct>]
    type Static = struct end

    let foo () =
        let q =
//            EcsQuery.qAnd ()
            ()
        ()





//type IEcsSystem =
//    abstract Update: world: EcsWorld -> unit
//
//
////type EcsSystemBuilder() =
//
//
//type EcsSystemUpdateEffect =
//    | UpdateComponent of (obj -> obj)
////    | Task of 'arg * ('result -> 'a)
//
//type EcsSystemUpdate = EcsWorld -> EcsSystemUpdateEffect seq
//
//module EcsSystemUpdate =
//    let iterateQuery (q: EcsQuery<'a>) : EcsWorld -> EcsEntity<'a> seq =
//        fun world -> q world
//
//    let mergeSeq (us: EcsSystemUpdate seq) : EcsSystemUpdate =
//        fun world -> us |> Seq.map (fun x -> x world) |> Seq.collect id
//
//    let bindQuery (binding: EcsEntity<'a> -> EcsSystemUpdate) (q: EcsQuery<'a>) : EcsSystemUpdate =
//        fun world ->
//            let es = q world
//            let upds = es |> Seq.map binding
//            mergeSeq upds world
//
//
//type EcsSystemUpdateBuilder() =
//    member _.Bind(q: EcsQuery<'a>, f: EcsEntity<'a> -> EcsSystemUpdate): EcsSystemUpdate = EcsSystemUpdate.bindQuery f q
//    member _.Yield(x): EcsSystemUpdate =
//        fun world -> seq [EcsSystemUpdateEffect.UpdateComponent x]
//
//[<AutoOpen>]
//module EcsSystemUpdateBuilderImpl =
//    let ecsSystemUpdate = EcsSystemUpdateBuilder()
//
//module pg =
//
//    open System.Numerics
//
//    type Position = { X: int; Y: int }
//    type Color = { Color: string }
//    type Velocity = { Velocity: Vector3 }
//
//    [<Struct>]
//    type Transform =
//        { Position: Vector3
//          Rotation: Vector3
//          Scale: Vector3 }
//
//    let foo () =
//        let q = ecsQuery {
//            let! position = EcsQuery.queryComponent<Position>
//            and! color = EcsQuery.queryComponent<Color>
//            and! velocity = EcsQuery.queryComponent<Velocity>
//            and! transform = EcsQuery.queryComponent<Transform>
//            return transform, position, color, velocity
//        }
////        ecsSystemUpdate {
////            let! transform, velocity = q
////            return [
////                { transform with Position = transform.Position + velocity.Velocity * dt }
////            ]
////        }
//        ()
//
////    module UnloadMarkerSystem =
////        let update =
////            let distance = 10f
////            ecsSystemUpdate {
////                let! playerPositions = ecsQuery {
////                    let! position = EcsQuery.queryComponent<Position>
////                    and! _ = EcsQuery.queryComponent<Player>
////                    return position
////                }
////                let! nonPlayerPositions = ecsQuery {
////                    let! position = EcsQuery.queryComponent<Position>
////                    and! _ = EcsQuery.queryComponent<Player> |> EcsQuery.not
////                    return position
////                }
////                for nonPlayerPosition in nonPlayerPositions do
////                    let isEnoughFar = playerPositions |> Seq.forall (fun pPos -> pPos.DistanceTo(nonPlayerPosition) > distance)
////
////                    ()
////            }
//
////    type MySystem() =
////        interface IEcsSystem with
////            member this.Update(world) =
////                let q = ecsQuery {
////                    let! position = EcsQuery.queryComponent<Position>
////                    and! color = EcsQuery.queryComponent<Color>
////                    and! velocity = EcsQuery.queryComponent<Velocity>
////                    and! transform = EcsQuery.queryComponent<Transform>
////                    return transform, position, color, velocity
////                }
////                let entities = q world
////                for entity in entities do
////                    let eid = entity.Id
////                    let transform, position, color, velocity = entity.Component
////                    transform.Position <- Vector3(0f, 0f, 0f)
////                    ()
//
////    let update =
////        ecsUpdate {
////            let q = ecsQuery { ... }
////            for e in q do
////
////        }
