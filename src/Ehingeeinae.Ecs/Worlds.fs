namespace Ehingeeinae.Ecs.Worlds

open System
open System.Collections.Generic

open Microsoft.Extensions.Logging

open TypeShape.Core

open Ehingeeinae.Ecs


type EcsArchetype =
    { ComponentTypes: HashSet<Type> }
    override this.ToString() =
        let inner = this.ComponentTypes |> Seq.map (fun x -> x.FullName) |> String.concat ", "
        $"Archetype[{inner}]"

module EcsArchetype =

    let inline createOfTypes (types: Type seq) = { ComponentTypes = HashSet(types) }
//    let inline create1<'c0> () = createOfTypes [ typeof<'c0> ]
//    let inline create2<'c0, 'c1> () = createOfTypes [ typeof<'c0>; typeof<'c1> ]

type EcsArchetypeEqualityComparer() =
    static let hashsetComparer = HashSet.CreateSetComparer()
    interface IEqualityComparer<EcsArchetype> with
        member this.Equals(x, y) = hashsetComparer.Equals(x.ComponentTypes, y.ComponentTypes)
        member this.GetHashCode(obj) = hashsetComparer.GetHashCode(obj.ComponentTypes)


/// ResizeArray<'comp>
type ComponentColumn = obj

module ComponentColumn =

    let inline unbox<'comp> (col: ComponentColumn) : ResizeArray<'comp> =
        assert (match col with :? ResizeArray<'comp> -> true | _ -> false)
        downcast col

    let createOfTypes (compTypes: Type seq) : ComponentColumn[] =
        [| for compType in compTypes ->
            let t = typedefof<ResizeArray<_>>.MakeGenericType(compType)
            Activator.CreateInstance(t) |]

//    let inline create1<'c0> () : ComponentColumn[] =
//        [| ResizeArray<'c0>() |]
//    let inline create2<'c0, 'c1> () : ComponentColumn[] =
//        [| ResizeArray<'c0>(); ResizeArray<'c1>() |]


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
        |> Array.tryPick (function
            | :? ResizeArray<'c> as col -> Some col
            | _ -> None
        )
        |> function Some col -> col | None -> failwithf $"Cannot find ComponentColumn<%O{typeof<'c>}>"
//    member this.Add1(eid, comp0: 'c0) =
//        this.Ids.Add(eid)
//        this.GetColumn<'c0>().Add(comp0)
//    member this.Add2(eid, comp0: 'c0, comp1: 'c1) =
//        this.Ids.Add(eid)
//        this.GetColumn<'c0>().Add(comp0)
//        this.GetColumn<'c1>().Add(comp1)


type EcsWorld =
    { Archetypes: IDictionary<EcsArchetype, ArchetypeStorage> }

module EcsWorld =
    let createEmpty () : EcsWorld =
        let comparer = EcsArchetypeEqualityComparer()
        { Archetypes = Dictionary(comparer) }

type EcsWorldEntityManager(world: EcsWorld, logger: ILogger<EcsWorldEntityManager>) =

    let getStorage archetype =
        let archetypes = world.Archetypes
        match archetypes.TryGetValue(archetype) with
        | false, _ ->
            logger.LogDebug($"Creating new storage for archetype {archetype}")
            let columns = ComponentColumn.createOfTypes archetype.ComponentTypes
            let storage = { Ids = ResizeArray(); ComponentColumns = columns }
            archetypes.[archetype] <- storage
            storage
        | true, storage -> storage

    let mutable lastEid = 0UL
    let createNextEid () =
        let newEid = lastEid + 1UL
        lastEid <- newEid
        EcsEntityId newEid

    let cachedAddEntity = Dictionary<Type, obj>()

    let mkAddEntity () : 'TTuple -> EcsEntityId =
        let shape = shapeof<'TTuple>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'TTuple> as shape) ->
            let compTypes = shape.Elements |> Seq.map (fun e -> e.Member.Type)
            let archetype = EcsArchetype.createOfTypes compTypes
            let storage = getStorage archetype
            let mkAddComp (shape: IShapeMember<'TTuple>) =
                shape.Accept({ new IMemberVisitor<'TTuple, 'TTuple -> unit> with
                    member this.Visit(shape) =
                        fun compTuple ->
                            let comp = shape.Get(compTuple)
                            let col = storage.GetColumn<'c>()
                            col.Add(comp)
                })
            let addComps = shape.Elements |> Array.map mkAddComp
            fun (compTuple: 'TTuple) ->
                let eid = createNextEid ()
                storage.Ids.Add(eid)
                addComps |> Array.iter (fun addComp -> addComp compTuple)
                eid
        // Single value
        | Shape.Struct shape ->
            let compTypes = [ typeof<'TTuple> ]
            let archetype = EcsArchetype.createOfTypes compTypes
            let storage = getStorage archetype
            let addComp =
                shape.Accept({ new IStructVisitor<'TTuple -> unit> with
                    member this.Visit() =
                        fun comp ->
                            let col = storage.GetColumn<'TTuple>()
                            col.Add(comp)
                })
            fun comp ->
                let eid = createNextEid ()
                storage.Ids.Add(eid)
                addComp comp
                eid
        | _ ->
            raise <| NotSupportedException($"Type '%O{typeof<'TTuple>}' is not supported for component set representation")

    member this.AddEntity<'TTuple>(t: 'TTuple): EcsEntityId =
        let addEntity =
            match cachedAddEntity.TryGetValue(typeof<'TTuple>) with
            | true, (:? ('TTuple -> EcsEntityId) as f) -> f
            | _ ->
                logger.LogDebug($"Make new AddEntity<'TTuple> for type '%O{typeof<'TTuple>}'")
                let addEntity = mkAddEntity()
                cachedAddEntity.[typeof<'TTuple>] <- addEntity
                addEntity
        addEntity t


type EcsWorldManager(world: EcsWorld, logger: ILogger<EcsWorldManager>) =
    let archetypes = world.Archetypes

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


//    member this.AddEntity1<'c0>(comp0: 'c0) =
//        let archetype = EcsArchetype.create1<'c0> ()
//        let storage = getStorage ComponentColumn.create1<'c0> archetype
//        let eid = createNextEid ()
//        storage.Add1(eid, comp0)
//        eid
//
//    member this.AddEntity2<'c0, 'c1>(comp0: 'c0, comp1: 'c1) =
//        let archetype = EcsArchetype.create2<'c0, 'c1> ()
//        let storage = getStorage ComponentColumn.create2<'c0, 'c1> archetype
//        let eid = createNextEid ()
//        storage.Add2(eid, comp0, comp1)
//        eid

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


