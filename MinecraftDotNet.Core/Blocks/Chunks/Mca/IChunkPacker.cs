namespace MinecraftDotNet.Core.Blocks.Chunks.Mca
{
    public interface IChunkPacker
    {
        Chunk Unpack(PackedChunk packedChunk);
        
        PackedChunk Pack(Chunk chunk);
    }
}

