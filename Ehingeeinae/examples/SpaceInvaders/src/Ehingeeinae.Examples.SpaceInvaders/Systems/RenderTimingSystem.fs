namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open System.Diagnostics
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Systems


type LogicTimingState =
    { DeltaTime: float32 }

type LogicTimingSystem(timings: IEcsUniqueResource<LogicTimingState>) =
    let stopwatch = Stopwatch()
    let mutable started = false
    interface IEcsSystem with
        member this.Update(ctx) =
            if not started then stopwatch.Start(); started <- true
            let elapsedSeconds = stopwatch.Elapsed.TotalSeconds
            stopwatch.Restart()
            timings.Value <- { timings.Value with DeltaTime = float32 elapsedSeconds }


type RenderTimingState =
    { DeltaTime: float32 }

type RenderTimingSystem(timings: IEcsUniqueResource<RenderTimingState>) =
    let stopwatch = Stopwatch()
    let mutable started = false
    interface IEcsSystem with
        member this.Update(ctx) =
            if not started then stopwatch.Start(); started <- true
            let elapsedSeconds = stopwatch.Elapsed.TotalSeconds
            stopwatch.Restart()
            timings.Value <- { timings.Value with DeltaTime = float32 elapsedSeconds }
