using System.Collections.Generic;

namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public class MemoryChunkRepository : IChunkRepository
    {
        private readonly IChunkGenerator _chunkGenerator;
        private readonly IDictionary<ChunkCoords, Chunk> _generatedChunks;
        
        public MemoryChunkRepository(IChunkGenerator chunkGenerator)
        {
            _chunkGenerator = chunkGenerator;
            _generatedChunks = new Dictionary<ChunkCoords, Chunk>();
        }
        
        public void UnloadChunk(ChunkCoords coords)
        {
            throw new System.NotImplementedException();
        }

        public Chunk GetChunk(ChunkCoords coords)
        {
            if (_generatedChunks.TryGetValue(coords, out var chunk))
            {
                return chunk;
            }
            else
            {
                var newChunk = _chunkGenerator.Generate(coords);
                _generatedChunks[coords] = newChunk;
                return newChunk;
            }
        }
    }
}