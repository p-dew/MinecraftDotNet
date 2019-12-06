using MinecraftDotNet.Core.Blocks.Chunks;

namespace MinecraftDotNet.Core.Graphics
{
    public interface IChunkRenderer<in TContext>
        where TContext : IChunkRenderContext
    {
        void Render(TContext context, Chunk chunk, ChunkCoords chunkCoords);
    }
}