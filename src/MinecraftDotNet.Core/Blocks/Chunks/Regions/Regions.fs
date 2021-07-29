namespace MinecraftDotNet.Core.Blocks.Chunks.Regions

open MinecraftDotNet.Core.Blocks.Chunks

//

//type RegionSize =
//    {  }

// RegionCoords

[<Struct>]
type RegionCoords =
    { X: int
      Z: int }

// IRegion

type IRegion =
    abstract TryGetChunk: coords: ChunkCoords -> Chunk option
    abstract TryGetPacketChunk: coords: ChunkCoords -> PacketChunk option
    abstract SetChunk: coords: ChunkCoords * chunk: Chunk -> unit
    abstract SetPacketChunk: coords: ChunkCoords * packetChunk: PacketChunk -> unit

// IRegionRepository

type IRegionRepository =
    abstract GetRegion: coords: RegionCoords -> IRegion
    abstract SaveRegion: coords: RegionCoords * dictRegion: DictRegion

//// DictRegion
//
//type DictRegion =
//    
//    interface IRegion