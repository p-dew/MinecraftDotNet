namespace Ehingeeinae.Ecs.Worlds

open System
open System.Collections.Generic

open System.Text
open Ehingeeinae.Collections
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


type EcsArchetypeEqualityComparer() =
    static let hashsetComparer = HashSet.CreateSetComparer()
    interface IEqualityComparer<EcsArchetype> with
        member this.Equals(x, y) = hashsetComparer.Equals(x.ComponentTypes, y.ComponentTypes)
        member this.GetHashCode(obj) = hashsetComparer.GetHashCode(obj.ComponentTypes)


/// With erased type
type IComponentColumn =
    abstract ComponentsBoxed: obj
    abstract Accept: IComponentColumnVisitor<'R> -> 'R

and ComponentColumn<'c>() =
    let chunks = ChunkList<'c>()
    member this.Components = chunks
    interface IComponentColumn with
        member this.ComponentsBoxed = box chunks
        member this.Accept(visitor: IComponentColumnVisitor<'R>) = visitor.Visit(this)

and IComponentColumnVisitor<'R> =
    abstract Visit<'c> : ComponentColumn<'c> -> 'R


module ComponentColumn =

    let inline tryUnbox<'c> (col: IComponentColumn) : ComponentColumn<'c> option =
        match col with
        | :? ComponentColumn<'c> as col -> Some col
        | _ -> None

    [<RequiresExplicitTypeArguments>]
    let inline unbox<'c> (col: IComponentColumn) : ComponentColumn<'c> =
        assert (match col with :? ComponentColumn<'c> -> true | _ -> false)
        downcast col

    let createOfTypes (compTypes: Type seq) : IComponentColumn[] =
        [| for compType in compTypes ->
            let t = typedefof<ComponentColumn<_>>.MakeGenericType(compType)
            Activator.CreateInstance(t) :?> IComponentColumn |]

type IEcsEntityView =
    abstract Archetype: EcsArchetype
    abstract GetComponent: unit -> 'c

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

    member this.GetColumn(cType: Type): IComponentColumn =
        this.ComponentColumns
        |> Array.pick (fun col ->
            let currColType =
                col.Accept({ new IComponentColumnVisitor<_> with
                    member _.Visit(_: ComponentColumn<'c>) =
                        typeof<'c>
                })
            if currColType = cType then
                Some col
            else
                None
        )

    member this.GetColumn<'c>(): ComponentColumn<'c> =
        this.ComponentColumns
        |> Array.tryPick (function
            | :? ComponentColumn<'c> as col -> Some col
            | _ -> None
        )
        |> function
        | Some col -> col
        | None -> failwithf $"Cannot find ComponentColumn<%O{typeof<'c>}>"

    member this.GetEntity(eid: EcsEntityId) =
        let idx = ids |> Seq.findIndex ((=) eid)
        let view = { new IEcsEntityView with
            member _.Archetype = archetype
            member _.GetComponent<'c>(): 'c =
                let c = this.GetColumn<'c>().Components.[idx]
                c
        }
        view

    override this.ToString() =
        let getsColumnComponentSting =
            componentColumns
            |> Array.map ^fun col ->
                col.Accept({ new IComponentColumnVisitor<_> with
                    member _.Visit(col) = fun i -> string col.Components.[i]
                })
        let sb = StringBuilder()
        sb.AppendLine("[") |> ignore
        for i in 0 .. ids.Count - 1 do
            sb.Append("    ") |> ignore
            sb.Append($"[{ids.[i].Value}]( ") |> ignore
            for j in 0 .. componentColumns.Length - 1 do
                let s = getsColumnComponentSting.[j] i
                sb.Append(s).Append(", ") |> ignore
            sb.AppendLine(")") |> ignore
        sb.Append("]") |> ignore
        sb.ToString()
        // sprintf "%A" {| ComponentColumns = componentColumns |> Seq.map (fun col -> col.Accept({ new IComponentColumnVisitor<_> with member _.Visit(col) = sprintf "%A" col.Components })) |}


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
                        fun (col: IComponentColumn) cs ->
                            let col = col |> ComponentColumn.unbox<'c>
                            let c = shapeElement.Get(cs)
                            col.Components.Add(c)
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
                fun (storage: ArchetypeStorage) c ->
                    let col = storage.GetColumn<'cs>()
                    col.Components.Add(c)
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
    abstract AddComponent: eid: EcsEntityId * cs: 'cs -> unit
    abstract RemoveComponent: eid: EcsEntityId * cs: 'cs -> unit
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

    member this.Clear() =
        world.Archetypes.Clear()

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

        member this.AddComponent(eid, cs) = failwith "todo"
        member this.RemoveComponent(eid, cs) = failwith "todo"

        member this.RemoveEntity(eid) =
            // let action () =
            //     let r =
            //         world.Archetypes
            //         |> Seq.tryPick ^fun (KeyValue (_, storage)) ->
            //             let idx = storage.Ids.IndexOf(eid)
            //             if idx = -1
            //             then None
            //             else Some (idx, storage)
            //     match r with
            //     | None -> raise ^ KeyNotFoundException($"{eid}")
            //     | Some (idx, storage) ->
            //         storage.Ids.RemoveAt(idx)
            //         storage.ComponentColumns
            //         |> Array.iter ^fun col ->
            //             col.Accept({ new IComponentColumnVisitor<_> with
            //                 member _.Visit(col) =
            //                     col.Components.RemoveAt(idx)
            //                     true
            //             }) |> ignore
            //         ()
            // lazyActions.Add(action)
            failwith "TODO"

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
