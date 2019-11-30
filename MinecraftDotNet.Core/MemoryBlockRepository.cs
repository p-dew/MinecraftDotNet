using System.Collections.Generic;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public class MemoryBlockRepository : IBlockRepository
    {
        private readonly IDictionary<Coordinates2, Chunk> _loadedChunks;
        
        public MemoryBlockRepository()
        {
            _loadedChunks = new Dictionary<Coordinates2, Chunk>();
        }
        
        public BlockInfo GetBlock(Coordinates3 pos)
        {
            var chunkX = pos.X / Chunk.Width;
            var chunkZ = pos.Z / Chunk.Depth;
            
            var chunk = GetChunk(new Coordinates2(chunkX, chunkZ));
            
            return chunk.Blocks[pos.X % Chunk.Width, pos.Y, pos.Z % Chunk.Depth];
        }
        
        public Meta GetBlockMeta(Coordinates3 pos)
        {
            var chunkX = pos.X / Chunk.Width;
            var chunkZ = pos.Z / Chunk.Depth;
            
            var chunk = GetChunk(new Coordinates2(chunkX, chunkZ));
            
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
        
        public Chunk GetChunk(Coordinates2 pos)
        {
            if (_loadedChunks.ContainsKey(pos))
                return _loadedChunks[pos];
            
            var newChunk = GenerateChunk();
            
            _loadedChunks[pos] = newChunk;
            return newChunk;
        }
    }
}