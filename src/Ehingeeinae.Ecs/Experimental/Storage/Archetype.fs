namespace Ehingeeinae.Ecs.Experimental.Storage.Archetype

open System
open System.Collections.Generic


type Archetype(comps: HashSet<Type>) =
    member this.ComponentTypes = comps
    member this.ContainsAll(subSet: Type seq) = comps.IsSupersetOf(subSet)
    member this.ContainsAll(subSet: Type array) = comps.IsSupersetOf(subSet)

    member this.Equals(arch: Archetype) = comps.SetEquals(arch.ComponentTypes)



