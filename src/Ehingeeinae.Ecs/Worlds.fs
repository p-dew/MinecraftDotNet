namespace Ehingeeinae.Ecs.Worlds

open System
open System.Collections.Generic

open Ehingeeinae.Ecs


type EcsArchetype =
    { ComponentTypes: ISet<Type> }

module EcsArchetype =

    let inline createSeq (types: Type seq) = { ComponentTypes = HashSet(types) }
    let inline create1<'c0> () = createSeq [ typeof<'c0> ]
    let inline create2<'c0, 'c1> () = createSeq [ typeof<'c0>; typeof<'c1> ]


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

module EcsWorld =
    let createEmpty () : EcsWorld =
        { Archetypes = Dictionary() }


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

    member this.AddEntity1<'c0>(comp0: 'c0) =
        let archetype = EcsArchetype.create1<'c0> ()
        let storage = getStorage ComponentColumn.create1<'c0> archetype
        let eid = createNextEid ()
        storage.Add1(eid, comp0)
        eid

    member this.AddEntity2<'c0, 'c1>(comp0: 'c0, comp1: 'c1) =
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
