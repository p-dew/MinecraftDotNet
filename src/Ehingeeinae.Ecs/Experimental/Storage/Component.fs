namespace Ehingeeinae.Ecs.Experimental.Storage


/// Пара тиков последнего добавления и изменения компонента
[<Struct>]
type ComponentTicks =
    { Added: uint
      Change: uint }


[<Struct>]
type CompInRef<'T> =
    val arr: 'T array
    val idx: int

    new(arr: 'T array, idx: int) = { arr = arr; idx = idx }

    member this.Value
        with get(): 'T inref = &this.arr.[this.idx]


type GetByRefState = { mutable HasAccess: bool }

[<Struct>]
type CompByRef<'T> =
    val arr: 'T array
    val idx: int
    val getByRefState: GetByRefState

    new(arr: 'T array, idx: int) = { arr = arr; idx = idx; getByRefState = { GetByRefState.HasAccess = false } }

    member this.AsCompInRef() = CompInRef(this.arr, this.idx)

    member this.Value
        with get(): 'T inref = &this.arr.[this.idx]

    member this.ValueMutable
        with get(): 'T byref =
            this.getByRefState.HasAccess <- true
            &this.arr.[this.idx]

