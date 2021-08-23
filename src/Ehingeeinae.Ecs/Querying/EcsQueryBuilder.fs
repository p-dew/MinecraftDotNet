[<AutoOpen>]
module Ehingeeinae.Ecs.Querying.Builders

open System
open System.ComponentModel

[<AutoOpen>]
[<EditorBrowsable(EditorBrowsableState.Never)>]
module Internal =

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

    // Run overloads for unfold tuples

    member _.Run<'c1>(_: struct(EcsQueryComponentType<'c1> * D)) =
        Optimized.EcsQuery.query1<'c1>
        // EcsQuery.query<EcsComponent<'c1>>

    member _.Run<'c1, 'c2>(_: struct(EcsQueryComponentType<struct('c1 * 'c2)> * E<D>)) =
        Optimized.EcsQuery.query2<'c1, 'c2>
        // EcsQuery.query<struct(EcsComponent<'c1> * EcsComponent<'c2>)>

    member _.Run<'c1, 'c2, 'c3>(_: struct(EcsQueryComponentType<struct('c1 * struct('c2 * 'c3))> * E<E<D>>)) =
        Optimized.EcsQuery.query3<'c1, 'c2, 'c3>
        // EcsQuery.query<struct(EcsComponent<'c1> * EcsComponent<'c2> * EcsComponent<'c3>)>

    member _.Run<'c1, 'c2, 'c3, 'c4>(_: struct(EcsQueryComponentType<struct('c1 * struct('c2 * struct('c3 * 'c4)))> * E<E<E<D>>>)) =
        EcsQuery.query<struct(EcsComponent<'c1> * EcsComponent<'c2> * EcsComponent<'c3> * EcsComponent<'c4>)>

    member _.Run(_: struct(_ * E<E<E<E<_>>>>)) =
        raise <| NotSupportedException("Too many component types")


let ecsQuery = EcsQueryBuilder()
