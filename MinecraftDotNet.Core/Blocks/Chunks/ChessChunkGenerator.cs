namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public class ChessChunkGenerator : IChunkGenerator
    {
        public Chunk Generate(ChunkCoords coords)
        {
            var newChunk = new Chunk();
            
            for (var x = 0; x < Chunk.Width; x++)
            for (var y = 0; y < Chunk.Height; y++)
            for (var z = 0; z < Chunk.Depth; z++)
            {
                if ((x % 2 == 0) ^ (z % 2 == 0) ^ (y % 2 == 0))
                {
                    newChunk.Blocks[x, y, z] = HcBlocks.Air;
                }
                else
                {
                    newChunk.Blocks[x, y, z] = HcBlocks.Dirt;
                }
            }

            return newChunk;
        }
    }
}