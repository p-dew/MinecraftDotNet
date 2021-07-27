namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public interface IChunkGenerator
    {
        Chunk Generate(ChunkCoords coords);
    }
}