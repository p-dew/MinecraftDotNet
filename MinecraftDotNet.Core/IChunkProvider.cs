namespace MinecraftDotNet.Core
{
    public interface IChunkProvider
    {
        Chunk GetChunk(ChunkCoords pos);
    }
}