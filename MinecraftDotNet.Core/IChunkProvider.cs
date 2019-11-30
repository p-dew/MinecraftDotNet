using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public interface IChunkProvider
    {
        Chunk GetChunk(Coordinates2 pos);
    }
}