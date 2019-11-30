namespace MinecraftDotNet.Core

open MinecraftDotNet.Core.Math


type ModelType =
    | Block
    | Plain
    | Polygon


type BlockInfo =
    {
        ItemInfo: ItemInfo
        BlastResistance: int
    }

module Blocks =
    let stone =
        {
            ItemInfo = 
        }

type IBlockProvider =
    abstract GetBlock: Vector3i -> BlockInfo
    abstract GetBlockMeta: Vector3i -> Meta

type ChunkCoordinates = Vector2i

type Chunk =
    private {
        Blocks: BlockInfo[,,]
        BlockMetas: Map<Vector3i, Meta>
        Entities: Entity seq
    }


type IEntityProvider =
    abstract GetEntity: ChunkCoordinates * EntityId -> Entity


type IChunkProvider =
    abstract GetChunk: chunkPos:ChunkCoordinates -> Chunk


type IChunkRepository =
    inherit IChunkProvider
    abstract SetBlock: Vector3i * BlockInfo -> unit


type ChunkProvider(chunkRepository: IChunkRepository) =
    interface IChunkProvider with
        member this.GetChunk(pos) = chunkRepository.GetChunk pos


type MemoryChunkRepository() =
    let genChunk() =
        {
            Blocks = [| [| {  } |] |]
        }
    interface IChunkRepository with
        member this.GetChunk(chunkPos) =
            ()