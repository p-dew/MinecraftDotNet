namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public interface IRegionBuilder
    {
        void SetChunk(ChunkCoords coords, Chunk chunk);
        void SetPacketChunk(ChunkCoords coords, PackedChunk packedChunk);
        
        IRegion Build();
    }
}