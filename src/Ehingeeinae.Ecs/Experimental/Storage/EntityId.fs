namespace Ehingeeinae.Ecs.Experimental.Storage

type RawEId = int // Raw entity Id
type Generation = uint

[<Struct>]
type EntityId =
    val internal id: RawEId
    val internal generation: Generation
    internal new(id: RawEId, generation: Generation) = { id = id; generation = generation }

    member this.Id = this.id
    member this.Generation = this.generation
