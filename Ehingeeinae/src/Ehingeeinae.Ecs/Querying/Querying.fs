namespace Ehingeeinae.Ecs.Querying

open System

open System.Collections.Generic
open Ehingeeinae.Ecs
// open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Worlds
open TypeShape.Core.Core


// Based on Rust Amethyst Legion
type IEcsQuery<'q> =
    abstract Fetch: ArchetypeStorage -> 'q seq
    abstract Filter: EcsArchetype -> bool


[<Struct>]
type EcsReadComponent<'c> = internal { Pointer: voidptr }

[<Struct>]
type EcsWriteComponent<'c> = internal { Pointer: voidptr }

type 'c cread = EcsReadComponent<'c>
type 'c cwrite = EcsWriteComponent<'c>

module EcsReadComponent =

    open System.Runtime.CompilerServices

    let inline internal cast (c: 'c inref) : EcsReadComponent<'c> =
        let c: 'c byref = &Unsafe.AsRef(&c) // inref to byref
        let p = Unsafe.AsPointer(&c)
        { Pointer = p }

    let getValue (comp: EcsReadComponent<'c>) : 'c inref =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        &Unsafe.AsRef<'c>(vp)


module EcsWriteComponent =

    open System.Runtime.CompilerServices

    let inline internal cast (c: 'c byref) : EcsWriteComponent<'c> =
        let p = Unsafe.AsPointer(&c)
        { Pointer = p }

    let getValue (comp: EcsWriteComponent<'c>) : 'c byref =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        &Unsafe.AsRef<'c>(vp)

    let setValue (comp: EcsWriteComponent<'c>) (value: 'c inref) : unit =
        let vp = comp.Pointer
        assert (VoidPtr.isNotNull vp)
        let p = &Unsafe.AsRef<'c>(vp)
        let value = value // dereference
        p <- value


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
