namespace MinecraftDotNet.Core.Blocks

open MinecraftDotNet.Core
open MinecraftDotNet.Core.Blocks.Chunks
open MinecraftDotNet.Core.Items
open MinecraftDotNet.Core.Blocks.Chunks.ChunkExtensions

type ChunkBlockRepository(chunkRepository: IChunkRepository) =
    
    let getChunkByBlockCoords (blockCoords: BlockCoords) =
        let chunkX = blockCoords.X / Chunk.Size.Width
        let chunkZ = blockCoords.Z / Chunk.Size.Depth
        chunkRepository.GetChunk({ X = chunkX; Z = chunkZ })
    
    interface IBlockRepository with
    
        member this.GetBlock(coords) =
            let chunk = getChunkByBlockCoords coords
            let localBlockCoords = chunk.GetLocalBlockCoords(coords)
            chunk.Blocks.[localBlockCoords.X, localBlockCoords.Y, localBlockCoords.Z]
        
        member this.GetBlockMeta(coords) =
            let chunk = getChunkByBlockCoords coords
            let localBlockCoords = chunk.GetLocalBlockCoords(coords)
            
            match chunk.BlockMetas.TryGetValue(localBlockCoords) with
            | false, _ -> 
                Meta.Empty
            | true, meta -> meta
        
        member this.SetBlock(blockInfo, coords) =
            let chunk = getChunkByBlockCoords coords
            let localBlockCoords = chunk.GetLocalBlockCoords(coords)
            chunk.Blocks.[localBlockCoords.X, localBlockCoords.Y, localBlockCoords.Z] <- blockInfo
        
        member this.SetBlockMeta(meta, coords) =
            let chunk = getChunkByBlockCoords coords
            let localBlockCoords = chunk.GetLocalBlockCoords(coords)
            chunk.BlockMetas.[localBlockCoords] <- meta
