namespace Ehingeeinae.Ecs.Experimental.Storage.Archetype

open System
open System.Collections.Generic


type Archetype(comps: HashSet<Type>) =

    static let hashSetComparer = HashSet<Type>.CreateSetComparer()

    member this.ComponentTypes = comps
    member this.ContainsAll(subSet: Type seq) = comps.IsSupersetOf(subSet)

    member this.Equals(arch: Archetype) =
        hashSetComparer.Equals(arch.ComponentTypes, this.ComponentTypes)
