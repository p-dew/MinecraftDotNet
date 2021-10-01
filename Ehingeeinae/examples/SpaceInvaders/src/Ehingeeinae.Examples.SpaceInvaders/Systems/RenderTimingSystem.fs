namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open System.Diagnostics
open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Systems


type LogicTimingState =
    { DeltaTimeMs: float32 }

type LogicTimingSystem(timings: IEcsUniqueResource<LogicTimingState>) =
    let stopwatch = Stopwatch()
    let mutable started = false
    interface IEcsSystem with
        member this.Update(ctx) =
            if not started then stopwatch.Start(); started <- true
            let elapsedMs = stopwatch.Elapsed.TotalMilliseconds
            stopwatch.Restart()
            timings.Value <- { timings.Value with DeltaTimeMs = float32 elapsedMs }


type RenderTimingState =
    { DeltaTimeMs: float32 }

type RenderTimingSystem(timings: IEcsUniqueResource<RenderTimingState>) =
    let stopwatch = Stopwatch()
    let mutable started = false
    interface IEcsSystem with
        member this.Update(ctx) =
            if not started then stopwatch.Start(); started <- true
            let elapsedMs = stopwatch.Elapsed.TotalMilliseconds
            stopwatch.Restart()
            timings.Value <- { timings.Value with DeltaTimeMs = float32 elapsedMs }
