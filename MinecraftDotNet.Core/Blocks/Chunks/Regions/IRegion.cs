using System.Diagnostics.CodeAnalysis;
using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;

namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
{
    public interface IRegion
    {
        bool TryGetChunk(ChunkCoords coords, [MaybeNullWhen(false)] out Chunk? outChunk);
        bool TryGetPacketChunk(ChunkCoords coords, [MaybeNullWhen(false)] out PackedChunk? outPackedChunk);

        void SetChunk(ChunkCoords coords, Chunk chunk);
        void SetPacketChunk(ChunkCoords coords, PackedChunk packedChunk);
    }
}