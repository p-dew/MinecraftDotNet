namespace Ehingeeinae.Ecs.Worlds

open System
open System.Collections.Generic

open System.Text
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

type IEcsEntityView =
    abstract Archetype: EcsArchetype
    abstract GetComponent<'c> : unit -> 'c

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
type ArchetypeStorage(archetype: EcsArchetype) =
    let ids = ResizeArray<EcsEntityId>()
    let componentColumns = ComponentColumn.createOfTypes archetype.ComponentTypes

    member _.Ids = ids
    member _.ComponentColumns = componentColumns

    member _.Count = ids.Count

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

    member this.Add(comps: 'T) =

        ()

    member this.GetEntity(eid: EcsEntityId) =
        let idx = ids |> Seq.findIndex ((=) eid)
        let view = { new IEcsEntityView with
            member _.Archetype = archetype
            member _.GetComponent<'c>(): 'c =
                let c = this.GetColumn<'c>().[idx]
                c
        }
        view

    override this.ToString() =
        let sb = StringBuilder()
        sb.AppendLine("[") |> ignore
        let cols = componentColumns |> Array.map (fun col -> col :?> System.Collections.IEnumerable |> Seq.cast<obj> |> Seq.toArray)
        for i in 0 .. ids.Count - 1 do
            sb.Append("    ") |> ignore
            let (EcsEntityId eid) = ids.[i]
            sb.Append($"<{eid}>{{ ") |> ignore
            for col in cols do
                let c = col.[i]
                sb.Append(c).Append(", ") |> ignore
            sb.AppendLine("}") |> ignore
        sb.Append("]") |> ignore
        sb.ToString()
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
            let storage = ArchetypeStorage(archetype)
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
        | Shape.Tuple (:? ShapeTuple<'TTuple> as shapeTuple) ->
            let mkAddComp (shapeElement: IShapeMember<'TTuple>) =
                shapeElement.Accept({ new IMemberVisitor<'TTuple, _> with
                    member _.Visit<'c>(shapeElement) =
                        fun (storage: ArchetypeStorage) compTuple ->
                            let comp = shapeElement.Get(compTuple)
                            let col = storage.GetColumn<'c>()
                            col.Add(comp)
                })
            let addComps = shapeTuple.Elements |> Array.map mkAddComp
            let compTypes = shapeTuple.Elements |> Seq.map (fun e -> e.Member.Type)
            let archetype = EcsArchetype.createOfTypes compTypes
            let storage = getStorage archetype
            fun (compTuple: 'TTuple) ->
                let eid = createNextEid ()
                storage.Ids.Add(eid)
                addComps |> Array.iter (fun addComp -> addComp storage compTuple)
                eid
        // Single value
        | _ ->
            let compTypes = [ typeof<'TTuple> ]
            let archetype = EcsArchetype.createOfTypes compTypes
            let storage = getStorage archetype
            let addComp =
//                shapeStruct.Accept({ new IStructVisitor<_> with
//                    member _.Visit() =
                        fun comp ->
                            let col = storage.GetColumn<'TTuple>()
                            col.Add(comp)
//                })
            fun comp ->
                let eid = createNextEid ()
                storage.Ids.Add(eid)
                addComp comp
                eid
//        | _ ->
//            raise <| NotSupportedException($"Type '%O{typeof<'TTuple>}' is not supported for component set representation")

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


