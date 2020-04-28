module MinecraftDotNet.Core.Math


open System.Runtime.InteropServices

// ----------------
// Vector 3
// ----------------

[<Struct>]
//[<IsByRefLike>]
[<StructLayout(LayoutKind.Sequential)>]
type Vector3<'T> =
    { X: 'T
      Y: 'T
      Z: 'T }

module Vector3 =
    val inline create: x:'a -> y:'a -> z:'a -> Vector3<'a>
    
    val inline ofTuple: 'a * 'a * 'a -> Vector3<'a>
    val inline toTuple: Vector3<'a> -> 'a * 'a * 'a
    
    val inline map: ('a -> 'b) -> Vector3<'a> -> Vector3<'b>
    val inline map2: ('a -> 'b -> 'c) -> Vector3<'a> -> Vector3<'b> -> Vector3<'c>
    val inline apply: Vector3<'a -> 'b> -> Vector3<'a> -> Vector3<'b>
    val inline fold: ('State -> 'a -> 'State) -> 'State -> Vector3<'a> -> 'State
    val inline reduce: ('a -> 'a -> 'a) -> Vector3<'a> -> 'a
    
    val inline zero< ^a when ^a : (static member Zero: ^a)> : Vector3< ^a>
    val inline one< ^a when ^a : (static member One: ^a)> : Vector3< ^a>
    val inline up< ^a when ^a: (static member Zero: ^a) and ^a: (static member One: ^a)> : Vector3< ^a>
    val inline down< ^a when ^a: (static member Zero: ^a) and ^a: (static member One: ^a) and ^a: (static member (~-): ^a -> ^a)> : Vector3< ^a>
    val inline right< ^a when ^a: (static member Zero: ^a) and ^a: (static member One: ^a)> : Vector3< ^a>
    val inline left< ^a when ^a: (static member Zero: ^a) and ^a: (static member One: ^a) and ^a: (static member (~-): ^a -> ^a)> : Vector3< ^a>
    val inline forward< ^a when ^a: (static member Zero: ^a) and ^a: (static member One: ^a)> : Vector3< ^a>
    val inline backward< ^a when ^a: (static member Zero: ^a) and ^a: (static member One: ^a) and ^a: (static member (~-): ^a -> ^a)> : Vector3< ^a>
    
    val inline length< ^a, ^b, ^c when
                       ^a: (static member ( * ): ^a -> ^a -> ^c) and
                       ^c: (static member ( + ): ^c -> ^c -> ^c) and
                       ^c: (static member Sqrt: ^c -> ^b) >
        : Vector3< ^a> -> ^b
    
    val inline negate< ^a when ^a: (static member ( ~- ): ^a -> ^a) >
        : Vector3< ^a> -> Vector3< ^a>
    
    val inline add< ^a, ^b, ^c when
                   (^a or ^b): (static member ( + ): ^a -> ^b -> ^c) >
        : Vector3< ^a> -> Vector3< ^b> -> Vector3< ^c>
    val inline subtract< ^a, ^b, ^c when
                        (^a or ^b): (static member ( - ): ^a -> ^b -> ^c) >
        : Vector3< ^a> -> Vector3< ^b> -> Vector3< ^c>
    val inline scale< ^a, ^b, ^c when
                     (^a or ^b): (static member ( * ): ^a -> ^b -> ^c)>
        : Vector3< ^a> -> ^b -> Vector3< ^c>
    
    val inline cross< ^a, ^b, ^d, ^c when
                     (^a or ^b): (static member ( * ): ^a -> ^b -> ^d) and
                     (^d): (static member ( - ): ^d -> ^d -> ^c) >
        : Vector3< ^a> -> Vector3< ^b> -> Vector3< ^c>
    
    val inline dot< ^a, ^b, ^c when
                   (^a or ^b): (static member ( * ): ^a -> ^b -> ^c) and
                   (^c): (static member ( + ): ^c -> ^c -> ^c)>
        : Vector3< ^a> -> Vector3< ^b> -> ^c
    
    val inline normalize< ^a, ^c, ^b when
                         (^a or ^c): (static member ( / ): ^a -> ^c -> ^b) and
                          ^a: (static member ( * ): ^a -> ^a -> ^b) and 
                          ^b: (static member ( + ): ^b -> ^b -> ^b) and
                          ^b: (static member Sqrt: ^b -> ^c) >
        : Vector3< ^a> -> Vector3< ^b>

type Vector3i = Vector3<int>
type Vector3f = Vector3<float32>
type Vector3d = Vector3<double>

// ----------------
// Vector 2
// ----------------

[<Struct>]
type Vector2<'a> =
    { X: 'a
      Y: 'a }

module Vector2 =
        
    val inline create: 'a -> 'a -> Vector2<'a>
    
    val inline ofTuple: 'a * 'a -> Vector2<'a>
    val inline toTuple: Vector2<'a> -> 'a * 'a
    
    val inline iter: ('a -> unit) -> Vector2<'a> -> unit
    
    val inline map: ('a -> 'b) -> Vector2<'a> -> Vector2<'b>
    
    val inline map2: ('a -> 'b -> 'c) -> Vector2<'a> -> Vector2<'b> -> Vector2<'c>
    
    val inline apply: Vector2<'a -> 'b> -> Vector2<'a> -> Vector2<'b>
    
    val inline fold: ('State -> 'a -> 'State) -> 'State -> Vector2<'a> -> 'State
    
    val inline reduce: ('a -> 'a -> 'a) -> Vector2<'a> -> 'a
    
    // Math
    val inline length< ^a, ^b, ^c when
                   ^a: (static member ( * ): ^a -> ^a -> ^c) and
                   ^c: (static member ( + ): ^c -> ^c -> ^c) and
                   ^c: (static member Sqrt: ^c -> ^b) >
        : Vector2< ^a> -> ^b
    val inline add< ^a, ^b, ^c when
                   (^a or ^b): (static member ( + ): ^a -> ^b -> ^c) >
        : Vector2< ^a> -> Vector2< ^b> -> Vector2< ^c>
    val inline negate< ^a when
                       ^a: (static member ( ~- ): ^a -> ^a) >
        : Vector2< ^a> -> Vector2< ^a>
    val inline subtract< ^a, ^b, ^c when
                        (^a or ^b): (static member ( - ): ^a -> ^b -> ^c) >
        : Vector2< ^a> -> Vector2< ^b> -> Vector2< ^c>
    val inline scale< ^a, ^b, ^c when
                     (^a or ^b): (static member ( * ): ^a -> ^b -> ^c)>
        : Vector2< ^a> -> ^b -> Vector2< ^c>
    
    val inline dot< ^a, ^b, ^c when
                   (^a or ^b): (static member ( * ): ^a -> ^b -> ^c) and
                   (^c): (static member ( + ): ^c -> ^c -> ^c)>
        : Vector2< ^a> -> Vector2< ^b> -> ^c
    
    val inline normalize< ^a, ^c, ^b when
                         (^a or ^c): (static member ( / ): ^a -> ^c -> ^b) and
                          ^a: (static member ( * ): ^a -> ^a -> ^b) and 
                          ^b: (static member ( + ): ^b -> ^b -> ^b) and
                          ^b: (static member Sqrt: ^b -> ^c) >
        : Vector2< ^a> -> Vector2< ^b>