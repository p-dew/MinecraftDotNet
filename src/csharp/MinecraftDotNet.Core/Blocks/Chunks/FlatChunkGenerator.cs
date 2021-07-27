namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public class FlatChunkGenerator : IChunkGenerator
    {
        private readonly int _height;
        
        public FlatChunkGenerator(int height)
        {
            _height = height;
        }
        
        public Chunk Generate(ChunkCoords coords)
        {
            var newChunk = new Chunk();
            
            for (var x = 0; x < Chunk.Width; x++)
            for (var y = 0; y < Chunk.Height; y++)
            for (var z = 0; z < Chunk.Depth; z++)
            {
                if (y > _height)
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