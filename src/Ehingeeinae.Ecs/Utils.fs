[<AutoOpen>]
module internal Ehingeeinae.Ecs.Utils

open System

let inline ( ^ ) f x = f x

module ResizeArray =

    open System.Reflection
    open System.Collections.Generic

    let private cache: Dictionary<Type, FieldInfo> = Dictionary()

    let getItems (rarr: ResizeArray<'a>) : ArraySegment<'a> =
        let field =
            match cache.TryGetValue(typeof<'a>) with
            | true, field -> field
            | false, _ ->
                let field = typeof<ResizeArray<'a>>.GetField("_items", BindingFlags.NonPublic ||| BindingFlags.Instance)
                cache.[typeof<'a>] <- field
                field
        let _items: 'a[] = downcast field.GetValue(rarr)
        ArraySegment(_items, 0, rarr.Count)


type ByRefAction<'T> = delegate of 'T byref -> unit
type ByRefAction<'T1, 'T2> = delegate of 'T1 byref * 'T2 byref -> unit


module ArraySeq =

    let iter1 (chunks: 'a[] seq) (f: ByRefAction<'a>) : unit =
        chunks
        |> Seq.iter (fun arr ->
            for i in 0 .. arr.Length - 1 do
                let mutable x = &arr.[i]
                f.Invoke(&x)
        )

    let iter2 (f: ByRefAction<'a, 'b>) (chunks: (Memory<'a> * Memory<'b>) seq) : unit =
        chunks
        |> Seq.iter (fun (arr1, arr2) ->
            assert (arr1.Length = arr2.Length)
            let span1 = arr1.Span
            let span2 = arr2.Span
            for i in 0 .. arr1.Length - 1 do
                let mutable x1 = &span1.[i]
                let mutable x2 = &span2.[i]
                f.Invoke(&x1, &x2)
        )


[<RequireQualifiedAccess>]
module VoidPtr =
    let inline isNotNull (p: voidptr) = IntPtr(p) <> IntPtr.Zero
