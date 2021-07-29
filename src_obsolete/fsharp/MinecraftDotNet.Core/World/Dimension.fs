module MinecraftDotNet.Core.World.Dimension

open Reader

type Seed = int

let create name =
    { Name = name
      Chunks = dict [] }

type IChunkGenerator =
    abstract Generate: ChunkCoords -> Chunk

type IChunkReader =
    abstract Read: ChunkCoords -> Chunk option

let readChunk coords =
    let inner (env: #IChunkReader) =
        env.Read coords
    Reader inner

let generateChunk coords =
    let inner (env: #IChunkGenerator) =
        env.Generate coords
    Reader inner

let getChunk (coords: ChunkCoords) (dim: Dimension) =
    reader {
        match dim.Chunks.TryGetValue(coords) |> Option.ofOut with
        | Some chunk -> return chunk
        | None ->
            match! readChunk coords with
            | Some chunk -> return chunk
            | None -> return! generateChunk coords
    }
