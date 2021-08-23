namespace Ehingeeinae.Ecs.Querying

open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Worlds


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


[<AutoOpen>]
module EcsQueryFilterExtensions =

    [<RequireQualifiedAccess>]
    module EcsQuery =

        let withFilter (filter: EcsQueryFilter) (q: IEcsQuery<'q>) : IEcsQuery<'q> =
            let (EcsQueryFilter filter) = filter
            { new IEcsQuery<'q> with
                member _.Fetch(storage) = q.Fetch(storage)
                member _.Filter(archetype) = q.Filter(archetype) && filter archetype
            }
