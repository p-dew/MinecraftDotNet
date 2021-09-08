namespace Ehingeeinae.Ecs.Benchmarks

open System


type [<Struct>] Comp1I = { I1: int }
type [<Struct>] Comp1G = { G1: Guid }
type [<Struct>] Comp1S = { S1: string }
type [<Struct>] Comp1F = { F1: float }

type [<Struct>] Comp8D =
    { D1: decimal; D2: decimal; D3: decimal; D4: decimal; D5: decimal; D6: decimal; D7: decimal; D8: decimal }
    static member Create(d: decimal) = { D1 = d*1m; D2 = d*2m; D3 = d*3m; D4 = d*4m; D5 = d*5m; D6 = d*6m; D7 = d*7m; D8 = d*8m }

type [<Struct>] Comp1I1S = { I1: int; S1: string }

type [<Struct>] Comp1I2S1F = { I1: int; S1: string; S2: string; F1: float }

type [<Struct>] Comp2I1S = { I1: int; I2: int; S1: string }
