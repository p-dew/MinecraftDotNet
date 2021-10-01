namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Examples.SpaceInvaders.Components
open OpenTK.Mathematics
open OpenTK.Windowing.GraphicsLibraryFramework


type PlayerControlSystem
    (
        inputStateResource: IEcsSharedResource<InputState>,
        q: IEcsQuery<struct(Player cread * Velocity cwrite)>,
        queryExecutor: EcsWorldQueryExecutor
    ) =
    interface IEcsSystem with
        member this.Update(ctx) =
            let r = queryExecutor.ExecuteQuery(q)
            let inputState = inputStateResource.Value
            for _, velocityComp in r do
                let mutable v = Vector2.Zero
                if inputState.KeyboardState.IsKeyDown(Keys.Up) then
                    v <- v + Vector2.UnitY
                if inputState.KeyboardState.IsKeyDown(Keys.Down) then
                    v <- v - Vector2.UnitY
                if inputState.KeyboardState.IsKeyDown(Keys.Left) then
                    v <- v + Vector2.UnitX
                if inputState.KeyboardState.IsKeyDown(Keys.Right) then
                    v <- v - Vector2.UnitX
                velocityComp.Value <- { dx = v.X; dy = v.Y }
