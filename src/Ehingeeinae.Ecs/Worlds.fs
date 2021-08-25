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
            sb.Append($"<{eid}>( ") |> ignore
            for col in cols do
                let c = col.[i]
                sb.Append(c).Append(", ") |> ignore
            sb.AppendLine(")") |> ignore
        sb.Append("]") |> ignore
        sb.ToString()


type EcsWorld =
    { Archetypes: IDictionary<EcsArchetype, ArchetypeStorage> }

module EcsWorld =
    let createEmpty () : EcsWorld =
        let comparer = EcsArchetypeEqualityComparer()
        { Archetypes = Dictionary(comparer) }

// --

type private AddEntityFunction<'cs> private () =
    static member val Instance =
        let shape = shapeof<'cs>
        match shape with
        | Shape.Tuple (:? ShapeTuple<'cs> as shapeTuple) ->
            let mkAddComp (shapeElement: IShapeMember<'cs>) =
                shapeElement.Accept({ new IMemberVisitor<'cs, _> with
                    member _.Visit<'c>(shapeElement) =
                        fun (col: ComponentColumn) cs ->
                            let col = col |> ComponentColumn.unbox<'c>
                            let c = shapeElement.Get(cs)
                            col.Add(c)
                })
            let addComps = shapeTuple.Elements |> Array.map mkAddComp
            let cTypes = shapeTuple.Elements |> Array.map (fun e -> e.Member.Type)
            let archetype = EcsArchetype.createOfTypes cTypes
            fun (getStorage: EcsArchetype -> ArchetypeStorage) (eids: EcsEntityId[]) ->
                fun (css: IReadOnlyList<'cs>) ->
                    assert (css.Count = eids.Length)
                    let storage = getStorage archetype
                    let cols = cTypes |> Array.map storage.GetColumn
                    for idxEntity in 0 .. eids.Length - 1 do
                        let eid = eids.[idxEntity]
                        let cs = css.[idxEntity]
                        storage.Ids.Add(eid)
                        for i in 0 .. cols.Length - 1 do
                            let col = cols.[i]
                            let addComp = addComps.[i]
                            addComp col cs
        // Single value
        | _ ->
            let addComp =
                fun (storage: ArchetypeStorage) comp ->
                    let col = storage.GetColumn<'cs>()
                    col.Add(comp)
            let compTypes = [ typeof<'cs> ]
            let archetype = EcsArchetype.createOfTypes compTypes
            fun (getStorage: EcsArchetype -> ArchetypeStorage) (eids: EcsEntityId[]) ->
                fun (css: IReadOnlyList<'cs>) ->
                    let storage = getStorage archetype
                    for idxEntity in 0 .. eids.Length - 1 do
                        let eid = eids.[idxEntity]
                        let cs = css.[idxEntity]
                        storage.Ids.Add(eid)
                        addComp storage cs

// --

type IEcsWorldEntityManager =
    abstract AddEntity: cs: 'cs -> EcsEntityId
    abstract AddEntities: css: #IReadOnlyList<'cs>-> IReadOnlyList<EcsEntityId>
    abstract RemoveEntity: eid: EcsEntityId -> unit
    abstract AddComponent: cs: 'cs * eid: EcsEntityId -> unit
    abstract RemoveComponent: cs: 'cs * eid: EcsEntityId -> unit
    abstract TryGetEntityView: eid: EcsEntityId -> IEcsEntityView option


type EcsWorldEntityManager(world: EcsWorld, logger: ILogger<EcsWorldEntityManager>) =

    let lazyActions = ResizeArray<unit -> unit>()

    let getStorage archetype =
        let archetypes = world.Archetypes
        match archetypes.TryGetValue(archetype) with
        | false, _ ->
            logger.LogDebug($"Creating new storage for archetype {archetype}")
            let storage = ArchetypeStorage(archetype)
            archetypes.[archetype] <- storage
            storage
        | true, storage -> storage

    let createNextEid =
        let mutable lastEid = 0UL
        fun () ->
            let newEid = lastEid + 1UL
            lastEid <- newEid
            EcsEntityId newEid

    /// Apply all entity-related changes
    member this.Commit() =
        lazyActions |> Seq.iter (fun f -> f ())
        lazyActions.Clear()

    interface IEcsWorldEntityManager with
        member this.AddEntities(css: #IReadOnlyList<'cs>) =
            let eids = Array.init css.Count (fun _ -> createNextEid ())
            let action () = AddEntityFunction<'cs>.Instance getStorage eids (upcast css)
            lazyActions.Add(action)
            upcast eids
        member this.AddEntity(cs) =
            (this :> IEcsWorldEntityManager)
                .AddEntities([| cs |])
            |> Seq.exactlyOne

        member this.AddComponent(cs, eid) = failwith "todo"
        member this.RemoveComponent(cs, eid) = failwith "todo"
        member this.RemoveEntity(eid) = failwith "todo"
        member this.TryGetEntityView(eid) = failwith "todo"



// ----


//type EcsWorldManager(world: EcsWorld, logger: ILogger<EcsWorldManager>) =
//    let archetypes = world.Archetypes
//
//    let getColumns (types: Type seq) : ComponentColumn[] seq =
//        seq {
//            for KeyValue (archetype, storage) in archetypes do
//                if archetype.ComponentTypes.IsSupersetOf(types) then
//                    let cols = types |> Seq.map storage.GetColumn |> Seq.toArray
//                    yield cols
//        }
//
////    let selectQuery (aq: ArchetypeQuery) =
////        let rec selectQuery (aq: ArchetypeQuery) (archetypes: EcsArchetype seq) =
////            match aq with
////            | ArchetypeQuery.HasComponentType t ->
////                archetypes |> Seq.where (fun a -> a.ComponentTypes.Contains(t))
////            | ArchetypeQuery.And (aq1, aq2) ->
////                ()
////        selectQuery aq archetypes.Keys
//
//
////    member this.AddEntity1<'c0>(comp0: 'c0) =
////        let archetype = EcsArchetype.create1<'c0> ()
////        let storage = getStorage ComponentColumn.create1<'c0> archetype
////        let eid = createNextEid ()
////        storage.Add1(eid, comp0)
////        eid
////
////    member this.AddEntity2<'c0, 'c1>(comp0: 'c0, comp1: 'c1) =
////        let archetype = EcsArchetype.create2<'c0, 'c1> ()
////        let storage = getStorage ComponentColumn.create2<'c0, 'c1> archetype
////        let eid = createNextEid ()
////        storage.Add2(eid, comp0, comp1)
////        eid
//
//    // ----
//
//    member this.QueryComponent1<'c0>(): (ArraySegment<'c0>) seq =
//        getColumns [typeof<'c0>]
//        |> Seq.map (fun cols ->
//            let col0 = cols.[0] |> ComponentColumn.unbox<'c0>
//            ResizeArray.getItems col0
//        )
//
//    member this.QueryComponent2<'c0, 'c1>(): (ArraySegment<'c0> * ArraySegment<'c1>) seq =
//        getColumns [typeof<'c0>; typeof<'c1>]
//        |> Seq.map (fun cols ->
//            let col0 = cols.[0] |> ComponentColumn.unbox<'c0>
//            let col1 = cols.[1] |> ComponentColumn.unbox<'c1>
//            ResizeArray.getItems col0, ResizeArray.getItems col1
//        )
