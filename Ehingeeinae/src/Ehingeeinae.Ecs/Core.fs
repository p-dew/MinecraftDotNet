namespace Ehingeeinae.Ecs

[<Struct>]
type EcsEntityId = EcsEntityId of uint64
with
    static member Unwrap(EcsEntityId x) = x
    member this.Value = EcsEntityId.Unwrap(this)
