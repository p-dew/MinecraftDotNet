namespace Ehingeeinae.Utils

open System

[<AutoOpen>]
module FunStuff =
    let inline ( ^ ) f x = f x


type ByRefAction<'T> = delegate of 'T byref -> unit
type ByRefAction<'T1, 'T2> = delegate of 'T1 byref * 'T2 byref -> unit

type ByRefFunc<'T, 'R> = delegate of 'T byref -> 'R
type ByRefFunc<'T1, 'T2, 'R> = delegate of 'T1 byref * 'T2 byref -> 'R


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
