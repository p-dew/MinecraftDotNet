namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public interface IChunkPacker
    {
        Chunk Unpack(PackedChunk packedChunk);
        
        PackedChunk Pack(Chunk chunk);
    }
}

