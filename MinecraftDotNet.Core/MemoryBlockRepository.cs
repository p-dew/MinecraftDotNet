using System.Collections.Generic;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public class MemoryBlockRepository : IBlockRepository
    {
        private readonly IDictionary<ChunkCoords, Chunk> _loadedChunks;
        
        public MemoryBlockRepository()
        {
            _loadedChunks = new Dictionary<ChunkCoords, Chunk>();
        }
        
        public BlockInfo GetBlock(Coordinates3 pos)
        {
            var chunk = GetChunk(pos.ToChunkCoords());
            
            return chunk.Blocks[pos.X % Chunk.Width, pos.Y, pos.Z % Chunk.Depth];
        }
        
        public Meta GetBlockMeta(Coordinates3 pos)
        {
            var chunk = GetChunk(pos.ToChunkCoords());
            
            var localBlockCoords = new Coordinates3(pos.X % Chunk.Width, pos.Y, pos.Z % Chunk.Depth);
            
            if (chunk.BlockMetas.ContainsKey(localBlockCoords))
                return chunk.BlockMetas[localBlockCoords];
            else
                return Meta.Empty;
        }
        
        private static Chunk GenerateChunk()
        {
            var newBlocks = new BlockInfo[Chunk.Width, Chunk.Height, Chunk.Depth];
            for (var x = 0; x < Chunk.Width; x++)
            for (var y = 0; y < Chunk.Height; y++)
            for (var z = 0; z < Chunk.Depth; z++)
            {
                if (y > 80)
                {
                    newBlocks[x, y, z] = HcBlocks.Air;
                }
                else
                {
                    newBlocks[x, y, z] = HcBlocks.Dirt;
                }
            }
            
            return new Chunk(newBlocks, new Dictionary<Coordinates3, Meta>());
        }
        
        public Chunk GetChunk(ChunkCoords pos)
        {
            if (_loadedChunks.ContainsKey(pos))
                return _loadedChunks[pos];
            
            var newChunk = GenerateChunk();
            
            _loadedChunks[pos] = newChunk;
            return newChunk;
        }
    }
}