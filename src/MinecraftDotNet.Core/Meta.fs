namespace MinecraftDotNet.Core

type MetaValue = obj
type MetaKey = string

type Meta =
    | Meta of Map<MetaKey, MetaValue>
    | Empty

