namespace Ehingeeinae.Ecs.Experimental.Storage

[<Struct>]
type EntityId =
    val internal id: uint32
    val internal generation: uint32
    internal new(id: uint32, generation: uint32) = { id = id; generation = generation }

    member this.Id = this.id
    member this.Generation = this.generation


