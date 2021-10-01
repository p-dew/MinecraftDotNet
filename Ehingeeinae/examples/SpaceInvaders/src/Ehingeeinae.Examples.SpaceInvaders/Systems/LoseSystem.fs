namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Examples.SpaceInvaders.Components

type LoseSystemOptions =
    { CriticalY: float32 }

type LoseSystem(q: IEcsQuery<struct(Position cread * Enemy cread)>, queryExecutor: EcsWorldQueryExecutor, options: LoseSystemOptions) =
    interface IEcsSystem with
        member this.Update(ctx) =
            let rs = queryExecutor.ExecuteQuery(q)
            for position, _ in rs do
                let position = position.Value
                let isCritical = position.Y > options.CriticalY
                if isCritical then
                    printfn "LOSE"
