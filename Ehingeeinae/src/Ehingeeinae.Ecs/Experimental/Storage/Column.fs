namespace Ehingeeinae.Ecs.Experimental.Storage

open System
open Ehingeeinae.Collections


type IDynamicColumn =
    abstract SwapRemove: idx: int -> unit
    abstract ComponentType: Type

/// Список компонента в архетипе
type Column<'T>() =
    let components = ChunkList<'T>()
    let componentsTicks = ChunkList<ComponentTicks>()

    let replaceFromEnd (chunkList: ChunkList<_>) (idx: int) =
        let last = chunkList.[chunkList.Count - 1]
        chunkList.[idx] <- last
        chunkList.RemoveLast()

    member internal this.Components = components
    member internal this.Ticks = componentsTicks

    member this.Add(comp: 'T, ticks: ComponentTicks) =
        components.Add(comp)
        componentsTicks.Add(ticks)

    /// Удаляет элемент по индексу и ставит на его место последний
    member this.SwapRemove(idx: int) =
        replaceFromEnd components idx
        replaceFromEnd componentsTicks idx

    interface IDynamicColumn with
        member this.SwapRemove(idx) = this.SwapRemove(idx)
        member _.ComponentType = typeof<'T>


[<AutoOpen>]
module DynamicColumnExtensions =
    type IDynamicColumn with
        [<RequiresExplicitTypeArguments>]
        member this.Cast<'a>() = this :?> Column<'a>
