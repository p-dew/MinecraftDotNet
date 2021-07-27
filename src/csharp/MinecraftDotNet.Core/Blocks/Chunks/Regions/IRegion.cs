using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;

namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
{
    public interface IRegion
    {
        bool TryGetChunk(ChunkCoords coords, out Chunk outChunk);

        bool TryGetPacketChunk(ChunkCoords coords, out PackedChunk outPackedChunk);

        void SetChunk(ChunkCoords coords, Chunk chunk);

        void SetPacketChunk(ChunkCoords coords, PackedChunk packedChunk);
    }
}