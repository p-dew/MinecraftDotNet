namespace Ehingeeinae.Ecs.Experimental.Storage


/// Пара тиков последнего добавления и изменения компонента
[<Struct>]
type ComponentTicks =
    { Added: uint
      Changed: uint }


[<Struct>]
type InRefComponent<'T> =
    val arr: 'T array
    val idx: int

    internal new(arr: 'T array, idx: int) = { arr = arr; idx = idx }

    member this.Value: 'T inref = &this.arr.[this.idx]


type GetByRefState = { mutable HadAccessed: bool }

[<Struct>]
type ByRefComponent<'T> =
    val arr: 'T array
    val idx: int
    val getByRefState: GetByRefState

    internal new(arr: 'T array, idx: int, sharedState: GetByRefState) = { arr = arr; idx = idx; getByRefState = sharedState }

    member this.AsInRef() = InRefComponent(this.arr, this.idx)

    member this.Value: 'T inref = &this.arr.[this.idx]

    member this.ValueMutable: 'T byref =
        this.getByRefState.HadAccessed <- true
        &this.arr.[this.idx]
