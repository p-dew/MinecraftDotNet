namespace Ehingeeinae.Ecs.Querying

open Ehingeeinae.Ecs.Worlds

type EcsWorldQueryExecutor(world: EcsWorld) =
    let archetypes = world.Archetypes
    member this.ExecuteQuery(query: IEcsQuery<'q>): 'q seq =
        seq {
            for KeyValue(archetype, storage) in archetypes do
                if not (query.Filter(archetype)) then () else
                let comps = query.Fetch(storage) // TODO: Cache?
                yield! comps
        }
