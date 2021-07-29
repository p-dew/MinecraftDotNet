namespace MinecraftDotNet

open MinecraftDotNet.Fp

type Nopable<'u> =
    | Nop
    | Update of 'u
    
    static member get_Unit() = Nop
    
    static member inline Apply(s: ^s, u: Nopable< ^u'>): ^s =
        match u with
        | Nop -> s
        | Update u -> UpdateM.apply s u
    
    static member inline Combine(u1: Nopable< ^u'>, u2: Nopable< ^u'>): Nopable< ^u'> =
        match u1, u2 with
        | Nop, Nop -> Nop
        | u, Nop -> u
        | Nop, u -> u
        | Update u1, Update u2 -> UpdateM.combine u1 u2 |> Update

module Nopable =
    let nop = Nopable.Update