namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public interface IChunkCommandHandler
    {
        void UnloadChunk(ChunkCoords coords);
    }
}