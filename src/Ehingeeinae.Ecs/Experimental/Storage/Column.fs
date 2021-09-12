namespace Ehingeeinae.Ecs.Experimental.Storage

open Ehingeeinae.Collections


/// Список компонента в архетипе
type Column<'T>() =
    let mutable components = ChunkList<'T>()
    let mutable componentsTicks = ChunkList<ComponentTicks>()

    member this.Add(comp: 'T, ticks: ComponentTicks) =
        components.Add(comp)
        componentsTicks.Add(ticks)
