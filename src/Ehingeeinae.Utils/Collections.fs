namespace Ehingeeinae.Collections

open System.Collections.Generic
open System.Runtime.CompilerServices

//////////////
// Enumerating

// ~ Next: unit -> Option<'a byref>
type IByrefEnumerator<'a> =
    abstract Next: unit -> bool
    abstract Current: 'a byref

type IByrefEnumerable<'a, 'impl> when 'impl :> IByrefEnumerator<'a> =
    abstract GetByrefEnumerator: unit -> 'impl

type IDynamicByrefEnumerable<'a> = IByrefEnumerable<'a, IByrefEnumerator<'a>>

[<Extension>]
type IByrefEnumerableExtensions =
    [<Extension>]
    static member ToDyn<'a, 'impl when 'impl :> IByrefEnumerator<'a> and 'impl : struct>(x: IByrefEnumerable<'a, 'impl>) =
        { new IDynamicByrefEnumerable<'a> with
            member _.GetByrefEnumerator() =
                x.GetByrefEnumerator() :> IByrefEnumerator<'a> }


module ByrefEnumerator =
    let inline moveNext (enumerator: #IByrefEnumerator<'a> byref) =
        let mutable e = &enumerator
        e.Next()

    let inline current (enumerator: #IByrefEnumerator<'a> byref) =
        let mutable e = &enumerator
        &e.Current

    let rec private containsLoop (item: 'a) (enumerator: #IByrefEnumerator<'a> byref) =
        let mutable e = &enumerator
        if moveNext &e then
            let c = current &e
            if c = item then true
            else containsLoop item &enumerator
        else false

    let inline contains (item: 'a) (enumerator: #IByrefEnumerator<'a> byref) =
        let mutable e = &enumerator
        containsLoop item &e

    [<Struct>]
    type ArrayByrefEnumerator<'a> =
        val mutable array: 'a array
        val mutable current: int
        new(array: 'a array) = { current = -1; array = array }
        interface IByrefEnumerator<'a> with
            member this.Current =
                &this.array.[this.current]
            member this.Next() =
                if this.current + 1 >= this.array.Length then false
                else this.current <- (this.current + 1); true




module ByrefEnumerable =
    let toDerefferedSeq (source: #IByrefEnumerable<'a, _>): 'a seq =
        let mutable enumerator = source.GetByrefEnumerator()
        seq {
            while enumerator.Next() do
                let value: 'a = enumerator.Current
                yield value
        }

    let inline getByrefEnumerator (enumerable: IByrefEnumerable<_, _>) =
        enumerable.GetByrefEnumerator()

    let ofArray (arr: 'a array) =
        { new IByrefEnumerable<'a, ByrefEnumerator.ArrayByrefEnumerator<'a>> with
            member _.GetByrefEnumerator() = ByrefEnumerator.ArrayByrefEnumerator<'a>(arr) }


// Enumerating
//////////////
/// ChunkList

(*

// DIRECT ITERATION
let mut i = 0
while i < length {
    let chIdx = i / CHUNK_SIZE;
    let elIdx = i % CHUNK_SIZE;
    yield arr[chIdx][elIdx]
    i += 1
}

// SMART ITERATION
Потенциально конечно быстрее, но сложно сказать будет ли быстрее чем два int деления, так что, пока оставлю так

*)


[<Struct>]
type ChunkListChunk<'a>(array: 'a array) =
    new(len: int) = ChunkListChunk(Array.zeroCreate len)
    member inline _.Array = array
    member inline _.Item with get(idx: int) = &array.[idx]


[<AutoOpen>]
module ChunkListConstants =
    [<Literal>]
    let DefaultChunkCapacity = 4096
    [<Literal>]
    let DefaultInitialCapacity = 4

module private Internal =
    let inline initChunks (cap: int) (limit: int) (realloc: bool) : List<ChunkListChunk<'a>> =
        let tailLen = cap % limit
        let hasTail = tailLen <> 0
        let chunkCount = if hasTail then cap / limit + 1 else cap / limit
        let mutable chunks = List(chunkCount)
        if realloc && hasTail then
            failwith "TODO"
            for _ in 0 .. chunkCount - 2 do chunks.Add(Array.zeroCreate limit |> ChunkListChunk)
            chunks.Add(Array.zeroCreate tailLen |> ChunkListChunk)
        else
            for _ in 0 .. chunkCount - 1 do chunks.Add(Array.zeroCreate limit |> ChunkListChunk)
        chunks

type ChunkList<'a> =
    /// Чанки
    val mutable internal chunks: List<ChunkListChunk<'a>>
    val mutable internal count: int
    /// Максимальный размер чанка
    val mutable internal chunkCapacity: int // TODO: rename
    /// Когда true, не заполненные чанки будут реаллоцироваться по мере заполнения как ArrayList до достижения лимита
    val internal realloc: bool

    // Реаллоцирование пока не реализовано и не факт что будет
    private new(initialMinCapacity: int, chunkCapacity: int, realloc: bool) =
        let initialChunks = Internal.initChunks initialMinCapacity chunkCapacity realloc
        { chunks = initialChunks
          count = 0
          chunkCapacity = chunkCapacity
          realloc = realloc }

    new(initialMinCapacity: int, chunkCapacity: int) = ChunkList(initialMinCapacity, chunkCapacity, false)
    new(initialMinCapacity: int) = ChunkList(initialMinCapacity, DefaultChunkCapacity, false)
    new() = ChunkList(DefaultInitialCapacity, DefaultChunkCapacity, false)

    member inline internal this.ItemUnchecked
        with get(idx: int) =
            let chunkIdx = idx / this.chunkCapacity
            let elementIdx = idx % this.chunkCapacity
            let chunk = this.chunks.[chunkIdx]
            &chunk.[elementIdx]

    member inline this.Item
        with get(idx: int) =
            if idx < 0 || idx >= this.count then
                failwith "Out of ChunkList index"
            &this.ItemUnchecked(idx)

    member this.ChunkCapacity = this.chunkCapacity

    member this.Count = this.count

    member this.IsEmpty = this.count = 0

    member private this.ReserveOne() =
        let add () = this.chunks.Add(ChunkListChunk(this.chunkCapacity))
        if this.chunks.Count = 0 then add ()
        else
            let tail = this.count % this.chunkCapacity
            if tail = 0 && this.count > 0 then add ()
            else do ()

    member this.Clear() =
        let mutable e = ByrefEnumerable.getByrefEnumerator this
        while ByrefEnumerator.moveNext &e do
            let mutable c = &(ByrefEnumerator.current &e)
            c <- Unchecked.defaultof<_>
        this.count = 0

    member this.Add(item) =
        this.ReserveOne()
        this.ItemUnchecked this.count <- item
        this.count <- this.count + 1

    member this.RemoveLast() =
        if this.count > 0 then
            let newCount = this.count - 1
            this.count <- newCount
            this.ItemUnchecked newCount <- Unchecked.defaultof<_>

    interface IByrefEnumerable<'a, ChunkListByrefEnumerator<'a>> with
        member this.GetByrefEnumerator() = ChunkListByrefEnumerator(this)



and [<Struct>] ChunkListByrefEnumerator<'a> =
    val mutable list: ChunkList<'a>
    val mutable current: int

    new(list: ChunkList<'a>) =
        { list = list; current = -1 }

    interface IByrefEnumerator<'a> with
        member this.Current = &this.list.ItemUnchecked(this.current)
        member this.Next() =
            if this.current + 1 >= this.list.Count then false
            else this.current <- (this.current + 1); true



