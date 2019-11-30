namespace MinecraftDotNet.Core

open MinecraftDotNet.Core.Math

type ITerrain =
    abstract GetBlock: Vector3i -> BlockInfo
    abstract GetBlockMeta: Vector3i -> Meta
    abstract GetEntity: EntityId -> Entity

type Dimension =
    {
        Name: string
        Terrain: ITerrain
    }

type World =
    {
        Name: string
        Player: Player seq
        Dimensions: Dimension seq
    }