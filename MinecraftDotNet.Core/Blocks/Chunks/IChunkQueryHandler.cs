namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public interface IChunkQueryHandler
    {
        Chunk GetChunk(ChunkCoords coords);
    }
}