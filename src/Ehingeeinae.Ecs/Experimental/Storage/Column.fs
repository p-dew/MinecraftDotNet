namespace Ehingeeinae.Ecs.Experimental.Storage

open System.Collections
open System.Collections.Generic
open Ehingeeinae.Collections


/// Список компонента в архетипе
type Column<'T>() =
    let mutable components = ChunkList<'T>()
    let mutable componentsTicks = ChunkList<ComponentTicks>()

    member this.Add(comp: 'T, ticks: ComponentTicks) =
        components.Add(comp)
        componentsTicks.Add(ticks)

    member this.Components = components
    member this.Ticks = componentsTicks
