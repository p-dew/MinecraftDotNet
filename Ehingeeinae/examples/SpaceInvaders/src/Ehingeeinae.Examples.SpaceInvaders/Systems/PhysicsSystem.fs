namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Examples.SpaceInvaders.Components


type PhysicsSystem
    (
        q: IEcsQuery<(Velocity cread * Position cwrite)>,
        queryExecutor: EcsWorldQueryExecutor,
        logicTimingStateResource: IEcsSharedResource<LogicTimingState>
    ) =
    interface IEcsSystem with
        member this.Update(ctx) =
            let dt = logicTimingStateResource.Value.DeltaTime
            for velocityComp, positionComp in queryExecutor.ExecuteQuery(q) do
                let xn = positionComp.Value.X + velocityComp.Value.dx * dt
                let yn = positionComp.Value.Y + velocityComp.Value.dy * dt
                positionComp.Value <- { X = xn; Y = yn }
