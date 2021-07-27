module MinecraftDotNet.Fp

type Reader<'state, 'a> =
    Reader of ('state -> 'a)
type Writer<'state, 'a> =
    Writer of ('state *  'a)
type State<'state, 'a> =
    State of ('state -> 'state * 'a)

type UpdateM<'state, 'update, 'a> =
    UpdateM of ('state -> 'update * 'a)

module UpdateM =
    
    let inline unit () = (^a: (static member Unit: ^a) ())
    
    let inline combine x y = (^a: (static member Combine: ^a * ^a -> ^a) (x, y))
    let inline (++) x y = combine x y
    
    let inline apply state update = (^u: (static member Apply: ^s * ^u -> ^s) (state, update))

    let inline value x = UpdateM ^fun _ -> unit (), x
    
    let run (UpdateM f: UpdateM<'s, 'u, 'a>) s = f s
    
    /// Map State and Update
    let map' sf uf (m: UpdateM<'s1, 'u1, 'a>) : UpdateM<'s2, 'u2, 'a> =
        UpdateM ^fun s ->
            let (UpdateM f) = m
            let u, x = f (sf s)
            uf u, x
    
    let inline bind f (UpdateM u1) =
        UpdateM ^fun s ->
            let u1, x = u1 s
            let (UpdateM u2) = f x
            let u2, y = u2 (apply s u1)
            combine u1 u2, y
    
    /// Map value
    let inline map mapping m =
        bind (mapping >> value) m
    
    let write (u: 'u) = (fun (_: 's) -> u, ()) |> UpdateM

    let inline read () = (fun (s: 'state) -> unit (), s) |> UpdateM

type UpdateBuilder() =
    
    member inline _.Zero() = UpdateM.value ()
    
    member inline _.Return(x) = UpdateM.value x
    
    member inline _.Delay(f) = UpdateM.bind f (UpdateM.value ())
    
    member inline _.Combine(x1, x2) = UpdateM.bind (fun() -> x2) x1
    
    member inline _.Bind(x, f) = UpdateM.bind f x
    
    [<CustomOperation "sub">]
    member _.Sub() = ()

let updateM = UpdateBuilder()


//type MyState =
//    { Count: int }
//
//type MyUpdate =
//    | Nop
//    | Set of int
//    | Add of int
//    static member Increase = Add  1
//    static member Decrease = Add -1
//    static member Reset = Set 0
//    
//    static member Unit = Nop
//    static member Apply(s, u) =
//        match u with
//        | Nop -> s
//        | Add x -> { s with Count = s.Count + x }
//        | Set x -> { s with Count = x }
//    static member Combine(u1, u2) =
//        match u1, u2 with
//        | Nop, Nop -> Nop | Nop, u -> u | u, Nop -> u
//        | Add x1, Add x2 -> Add (x1 + x2)
//        | _, Set x -> Set x
//        | Set x1, Add x2 -> Set (x1 + x2)
//
//let read () = UpdateMonad.read Nop
//
//let u = updateM {
//    do! UpdateMonad.write (Set 3)
//    do! UpdateMonad.write (Add 2)
//    let! x = read ()
//    return x.Count + 10
//}
