using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public class ChessChunkGenerator : IChunkGenerator
    {
        private readonly BlockProvider _blockProvider;

        public ChessChunkGenerator(BlockProvider blockProvider)
        {
            _blockProvider = blockProvider;
        }

        public delegate BlockInfo BlockProvider(Coords3 coords);

        public Chunk Generate(ChunkCoords coords)
        {
            var newChunk = new Chunk();
            
            for (var x = 0; x < Chunk.Width; x++)
            for (var y = 0; y < Chunk.Height; y++)
            for (var z = 0; z < Chunk.Depth; z++)
            {
                if (((x + 1) % 2 == 0) ^ ((z + 1) % 2 == 0) ^ ((y + 1) % 2 == 0))
                {
                    newChunk.Blocks[x, y, z] = HcBlocks.Air;
                }
                else
                {
                    newChunk.Blocks[x, y, z] = _blockProvider(new Coords3(x, y, z));
                }
            }

            return newChunk;
        }
    }
}