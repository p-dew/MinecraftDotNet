namespace Ehingeeinae.Ecs.Querying

open System

open System.Collections.Generic

open Ehingeeinae.Utils
open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


// Based on Rust Amethyst Legion
type IEcsQuery<'q> =
    abstract Fetch: ArchetypeStorage -> 'q seq
    abstract Filter: EcsArchetype -> bool


[<Struct>]
type EcsReadComponent<'c> =
    internal
        { Column: ComponentColumn<'c>
          Index: int }

[<Struct>]
type EcsWriteComponent<'c> =
    internal
        { Column: ComponentColumn<'c>
          Index: int }

type 'c cread = EcsReadComponent<'c>
type 'c cwrite = EcsWriteComponent<'c>

module EcsReadComponent =

    let getValue (comp: EcsReadComponent<'c>) : 'c inref =
        let comps = comp.Column.Components
        let i = comp.Index
        &comps.[i]


module EcsWriteComponent =

    let getValue (comp: EcsWriteComponent<'c>) : 'c byref =
        let comps = comp.Column.Components
        let i = comp.Index
        &comps.[i]

    let setValue (comp: EcsWriteComponent<'c>) (value: 'c inref) : unit =
        let comps = comp.Column.Components
        let i = comp.Index
        let value = value // dereference
        comps.[i] <- value


[<AutoOpen>]
module EcsComponentExtensions =

    type EcsReadComponent<'c> with
        member this.Value = &EcsReadComponent.getValue this

    type EcsWriteComponent<'c> with
        member this.Value
            with get(): 'c byref = &EcsWriteComponent.getValue this
            // and set(c: 'c inref) = EcsWriteComponent.setValue this &c


type IEcsQueryFactory =
    [<RequiresExplicitTypeArguments>]
    abstract CreateQuery<'q> : unit -> IEcsQuery<'q>

type CachingEcsQueryFactory(queryFactory: IEcsQueryFactory) =
    let cache = Dictionary<Type, obj>()
    interface IEcsQueryFactory with
        member this.CreateQuery<'q>() =
            match cache.TryGetValue(typeof<'q>) with
            | true, query -> unbox query
            | false, _ ->
                let query = queryFactory.CreateQuery<'q>()
                cache.[typeof<'q>] <- box query
                query
