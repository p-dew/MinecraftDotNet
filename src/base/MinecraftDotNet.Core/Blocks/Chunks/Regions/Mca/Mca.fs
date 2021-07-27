namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca

open System.Collections.Generic
open MinecraftDotNet.Core.Blocks.Chunks
open MinecraftDotNet.Core.Blocks.Chunks.Regions

// PacketChunk

type CompressionType =
    | GZip = 1
    | Zlib = 2

type PacketChunk =
    { PacketData: IReadOnlyList<byte>
      CompressionType: CompressionType }

// IChunkPacker

type IChunkPacker =
    abstract Pack: chunk: Chunk -> PacketChunk
    abstract Unpack: packetChunk: PacketChunk -> Chunk

// IRegionBuilder

type IRegionBuilder =
    abstract SetChunk: coords: ChunkCoords * chunk: Chunk -> unit
    abstract SetPacketChunk: coords: ChunkCoords * packetChunk: PacketChunk -> unit
    abstract Build: unit -> IRegion

// RegionBounds

type RegionBounds(position: ChunkCoords, size: ChunkCoords) =
    
    /// <remarks>
    ///     ^           
    ///   . | #   #   # 
    ///     |           
    ///   . | #   #   # 
    ///   xSize         
    ///   . | O   #   # 
    ///     ---zSize----->
    ///   .   .   .   . 
    /// </remarks>
    new(position: ChunkCoords, xSize: int, ySize: int) =
        let size: ChunkCoords = { X = xSize; Z = ySize }
        RegionBounds(position, size)
    
    new(pos0: ChunkCoords, pos1: ChunkCoords) =
        let position: ChunkCoords = { X = min pos0.X pos1.X; Z = min pos0.Z pos1.Z }
        let size: ChunkCoords = { X = abs (pos0.X - pos1.X); Z = abs (pos0.Z - pos1.Z) }
        RegionBounds(position, size)
    
    member _.Size = size // TODO: Replace to RegionSize
    member _.Position = position
    
    member _.Contains(coords: ChunkCoords) =
        let localX = coords.X - position.X
        let localZ = coords.Z - position.Z
        (localX >= 0 && localX <= size.X)
        && (localZ >= 0 && localZ < size.Z)
