[<AutoOpen>]
module Ehingeeinae.Utils.GenericExtensions

open System


[<RequireQualifiedAccess>]
module Disposable =
    let disposeAll (disposables: #IDisposable seq) : unit =
        for disposable in disposables do
            disposable.Dispose()


[<RequireQualifiedAccess>]
module Seq =

    let existsBoth (predicate1: 'a -> bool) (predicate2: 'a -> bool) (source: 'a seq) : bool =
        let enumerator = source.GetEnumerator()
        let mutable contains1 = false
        let mutable contains2 = false
        while not (contains1 && contains2) && enumerator.MoveNext() do
            let x = enumerator.Current
            if not contains1 && predicate1 x then
                contains1 <- true
            if not contains2 && predicate2 x then
                contains2 <- true
        contains1 && contains2

    let containsBoth (value1: 'a) (value2: 'a) (source: 'a seq) : bool =
        existsBoth (fun x -> value1 = x) (fun x -> value2 = x) source
