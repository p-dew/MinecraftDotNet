namespace Ehingeeinae.Ecs.Systems

type EcsSystemContext =
    { Empty: unit }

type IEcsSystem =
    abstract Update: ctx: EcsSystemContext -> unit

module EcsSystem =
    let inline create update = { new IEcsSystem with member _.Update(ctx) = update ctx }
