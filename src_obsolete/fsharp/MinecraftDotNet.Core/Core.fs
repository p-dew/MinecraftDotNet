namespace MinecraftDotNet.Core

open System.Collections.Generic

type MetaValue = obj
type MetaKey = string

type Meta =
    | Meta of Map<MetaKey, MetaValue>
    | Empty

module Meta =
    let empty = Empty
    
    let create (pairs: (MetaKey * MetaValue) seq) =
        Map.ofSeq pairs |> Meta
    
    let add key value meta =
        match meta with
        | Empty -> create (seq { key, value })
        | Meta map -> Map.add key value map |> Meta
    
    let containsKey key meta =
        match meta with
        | Empty -> false
        | Meta map -> Map.containsKey key map
    
    let get key meta =
        match meta with
        | Empty -> None
        | Meta map -> Map.tryFind key map
    
    /// Synonym of `Meta.add`
    let set key (value: MetaValue) meta = add key value meta
    
    let mapValue key (mapping: MetaValue -> MetaValue) meta =
        match get key meta with
        | None -> meta
        | Some value ->
            let value' = mapping value
            set key value' meta