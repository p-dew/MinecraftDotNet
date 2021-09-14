namespace Ehingeeinae.Ecs.Experimental.Storage

open System.Collections
open System.Collections.Generic
open Ehingeeinae.Collections


type IDynamicColumn =
    abstract SwapRemove: int -> unit
//    abstract Cast<'U> : unit -> Column<'U>

/// Список компонента в архетипе
and Column<'T>() =
    let mutable components = ChunkList<'T>()
    let mutable componentsTicks = ChunkList<ComponentTicks>()

    member internal this.Components = components
    member internal this.Ticks = componentsTicks

    member this.Add(comp: 'T, ticks: ComponentTicks) =
        components.Add(comp)
        componentsTicks.Add(ticks)

    /// Удаляет элемент по индексу и ставит на его место последний
    member this.SwapRemove(idx: int) =
        let lastC = components.[components.Count - 1]
        components.[idx] <- lastC
        components.RemoveLast()
        let lastT = componentsTicks.[componentsTicks.Count - 1]
        componentsTicks.[idx] <- lastT
        componentsTicks.RemoveLast()

    interface IDynamicColumn with
        member this.SwapRemove(idx) = this.SwapRemove(idx)
//        member this.Cast<'a>() : Column<'a> = this :?> Column<'a>






