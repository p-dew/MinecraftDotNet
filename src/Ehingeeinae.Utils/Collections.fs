namespace Ehingeeinae.Collections

open System
open System.Runtime.CompilerServices

//////////////
// Enumerating

type IByRefEnumerator<'T> =
    abstract Next: unit -> bool
    abstract Current: 'T byref

type IByRefEnumerable<'T, 'TImpl> when 'TImpl :> IByRefEnumerator<'T> =
    abstract GetByRefEnumerator: unit -> 'TImpl

type IDynamicByRefEnumerable<'T> = IByRefEnumerable<'T, IByRefEnumerator<'T>>

[<Extension>]
type ByRefEnumerableExtensions =
    [<Extension>]
    static member ToDyn<'a, 'impl when 'impl :> IByRefEnumerator<'a> and 'impl : struct>(x: IByRefEnumerable<'a, 'impl>) =
        { new IDynamicByRefEnumerable<'a> with
            member _.GetByRefEnumerator() =
                upcast x.GetByRefEnumerator() }


module ByRefEnumerator =
    let inline moveNext (enumerator: #IByRefEnumerator<'a> byref) =
        let mutable e = &enumerator
        e.Next()

    let inline current (enumerator: #IByRefEnumerator<'a> byref) =
        let mutable e = &enumerator
        &e.Current

    let rec private containsLoop (item: 'a) (enumerator: #IByRefEnumerator<'a> byref) =
        let mutable e = &enumerator
        if moveNext &e then
            let c = current &e
            if c = item
            then true
            else containsLoop item &enumerator
        else
            false

    let contains (item: 'a) (enumerator: #IByRefEnumerator<'a> byref) =
        let mutable e = &enumerator
        containsLoop item &e

    [<Struct>]
    type ArrayByRefEnumerator<'a> =
        val mutable array: 'a array
        val mutable idxCurrent: int
        new(array: 'a array) = { idxCurrent = -1; array = array }
        interface IByRefEnumerator<'a> with
            [<IsReadOnly>]
            member this.Current =
                &this.array.[this.idxCurrent]
            member this.Next() =
                if this.idxCurrent + 1 >= this.array.Length then
                    false
                else
                    this.idxCurrent <- (this.idxCurrent + 1)
                    true


module ByRefEnumerable =
    let toDereferencedSeq (source: IByRefEnumerable<'a, _>): 'a seq =
        let mutable enumerator = source.GetByRefEnumerator()
        seq {
            while enumerator.Next() do
                let value: 'a = enumerator.Current
                yield value
        }

    let inline getByRefEnumerator (enumerable: IByRefEnumerable<_, _>) =
        enumerable.GetByRefEnumerator()

    let ofArray (arr: 'a array) =
        { new IByRefEnumerable<'a, ByRefEnumerator.ArrayByRefEnumerator<'a>> with
            member _.GetByRefEnumerator() = ByRefEnumerator.ArrayByRefEnumerator<'a>(arr) }


// Enumerating
//////////////
// ChunkList

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
    member _.Array = array
    member _.Item with get(idx: int) = &array.[idx]


[<AutoOpen>]
module ChunkListConstants =
    [<Literal>]
    let DefaultChunkCapacity = 4096
    [<Literal>]
    let DefaultInitialCapacity = 4

module private Internal =
    let inline initChunks (capacity: int) (limit: int) (doRealloc: bool) : ResizeArray<ChunkListChunk<'a>> =
        let tailLen = capacity % limit
        let hasTail = tailLen <> 0
        let chunkCount = if hasTail then capacity / limit + 1 else capacity / limit
        let mutable chunks = ResizeArray(chunkCount)
        if doRealloc && hasTail then
            failwith "TODO" // TODO: Обработка неполных чанков
            for _ in 0 .. chunkCount - 2 do chunks.Add(Array.zeroCreate limit |> ChunkListChunk)
            chunks.Add(Array.zeroCreate tailLen |> ChunkListChunk)
        else
            for _ in 0 .. chunkCount - 1 do chunks.Add(Array.zeroCreate limit |> ChunkListChunk)
        chunks


type ChunkList<'a> =
    /// Chunks
    val mutable internal chunks: ResizeArray<ChunkListChunk<'a>>
    val mutable internal count: int
    /// Max chunk size
    val mutable internal chunkCapacity: int
    /// When true then non filled chunks will be reallocated during a filling, as ResizeArray does it
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

    member this.Item
        with get(idx: int) =
            if idx < 0 || idx >= this.count then
                raise <| IndexOutOfRangeException()
            &this.ItemUnchecked(idx)

    member this.ChunkCapacity = this.chunkCapacity

    member this.Count = this.count

    member this.IsEmpty = this.count = 0

    member private this.ReserveOne() =
        let add () = this.chunks.Add(ChunkListChunk(this.chunkCapacity))
        if this.chunks.Count = 0 then
            add ()
        else
            let tail = this.count % this.chunkCapacity
            if tail = 0 && this.count > 0 then
                add ()

    member this.Clear(): unit =
        let mutable e = ByRefEnumerable.getByRefEnumerator this
        while ByRefEnumerator.moveNext &e do
            let mutable c = &(ByRefEnumerator.current &e)
            c <- Unchecked.defaultof<_>
        this.count <- 0

    member this.Add(item) =
        this.ReserveOne()
        this.ItemUnchecked(this.count) <- item
        this.count <- this.count + 1

    member this.RemoveLast() =
        if this.count > 0 then
            let newCount = this.count - 1
            this.count <- newCount
            this.ItemUnchecked(newCount) <- Unchecked.defaultof<_>

    interface IByRefEnumerable<'a, ChunkListByRefEnumerator<'a>> with
        member this.GetByRefEnumerator() = ChunkListByRefEnumerator(this)


and [<Struct>] ChunkListByRefEnumerator<'a> =
    val mutable private list: ChunkList<'a>
    val mutable private current: int

    new(list: ChunkList<'a>) =
        { list = list; current = -1 }

    interface IByRefEnumerator<'a> with
        member this.Current = &this.list.ItemUnchecked(this.current)
        member this.Next() =
            if this.current + 1 >= this.list.Count then
                false
            else
                this.current <- (this.current + 1)
                true
