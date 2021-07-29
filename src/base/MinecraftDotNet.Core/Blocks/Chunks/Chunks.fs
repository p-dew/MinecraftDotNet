namespace MinecraftDotNet.Core.Blocks.Chunks

open System.Collections.Generic
open MinecraftDotNet.Core
open MinecraftDotNet.Core.Blocks

//

type ChunkSize =
    { Width: int
      Height: int
      Depth: int }

// Chunk

type Chunk(blockMetas: IDictionary<BlockCoords, Meta>) =
    let blocks =
        Array3D.zeroCreate Chunk.Size.Width Chunk.Size.Height Chunk.Size.Depth

    new() =
        let blockMetas = Dictionary()
        Chunk(blockMetas)

    static member Size =
        { Width = 16
          Height = 32
          Depth = 16 }

    member _.Blocks: BlockInfo[,,] = blocks
    member _.BlockMetas = blockMetas

// ChunkCoords

type ChunkCoords =
    { X: int
      Z: int }
    override this.ToString() =
        $"{nameof ChunkCoords}({this.X}, {this.Z})"

// IChunkQueryHandler

type IChunkQueryHandler =
    abstract GetChunk: coords: ChunkCoords -> Chunk

// IChunkCommandHandler

type IChunkCommandHandler =
    abstract UnloadChunk: coords: ChunkCoords -> unit

// IChunkRepository

type IChunkRepository =
    inherit IChunkCommandHandler
    inherit IChunkQueryHandler

// IChunkGenerator

type IChunkGenerator =
    abstract Generate: coords: ChunkCoords -> Chunk

// ChunkExtensions

module ChunkExtensions =
    type Chunk with
        member this.GetLocalBlockCoords(globalBlockCoords: BlockCoords): BlockCoords =
            let localX = globalBlockCoords.X % Chunk.Size.Width
            let localY = globalBlockCoords.Y % Chunk.Size.Height
            let localZ = globalBlockCoords.Z % Chunk.Size.Depth
            { X = localX; Y = localY; Z = localZ }

//

module ChunkGenerators =

    // FlatChunkGenerator

    type FlatChunkGenerator(height: int, terrainBlock, airBlock) =
        interface IChunkGenerator with
            member this.Generate(coords) =
                let newChunk = Chunk()
                for x in 0 .. Chunk.Size.Width - 1 do
                    for y in 0 .. Chunk.Size.Height - 1 do
                        for z in 0 .. Chunk.Size.Depth - 1 do
                            let newBlock =
                                if y > height then
                                    airBlock
                                else
                                    terrainBlock
                            newChunk.Blocks.[x, y, z] <- newBlock
                newChunk

    // ChessChunkGenerator

    type ChessChunkGenerator(airBlock, blockProvider) =
        interface IChunkGenerator with
            member this.Generate(coords) =
                let newChunk = Chunk()
                for x in 0 .. Chunk.Size.Width - 1 do
                    for y in 0 .. Chunk.Size.Height - 1 do
                        for z in 0 .. Chunk.Size.Depth - 1 do
                            let inline f a = (a + 1) % 2 = 0
                            let newBlock =
                                if f x <> f y <> f z then
                                    airBlock ()
                                else
                                    blockProvider ()
                            newChunk.Blocks.[x, y, z] <- newBlock
                newChunk

module ChunkRepositories =

    open Microsoft.Extensions.Logging

    // MemoryChunkRepository

    type MemoryChunkRepository(chunkGenerator: IChunkGenerator, logger: ILogger<MemoryChunkRepository>) =
        let generatedChunks = Dictionary()

        let generateChunk coords =
            logger.LogDebug($"Generate new Chunk(Coords = ${coords})")
            let newChunk = chunkGenerator.Generate(coords)
            generatedChunks.[coords] <- newChunk
            newChunk

        interface IChunkRepository with
            member this.GetChunk(coords) =
                match generatedChunks.TryGetValue(coords) with
                | true, chunk -> chunk
                | false, _ -> generateChunk coords


            member this.UnloadChunk(coords) =
                generatedChunks.Remove(coords) |> ignore
