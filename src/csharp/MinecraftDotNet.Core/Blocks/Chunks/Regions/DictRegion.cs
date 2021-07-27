using System.Collections.Generic;
using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;

namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
{
    public class DictRegion : IRegion
    {
        public static int Width { get; } = 32;
        
        public static int Depth { get; } = 32;
        
        private readonly IDictionary<ChunkCoords, Chunk> _chunks;
        private readonly IDictionary<ChunkCoords, PackedChunk> _packedChunks;
        
        public DictRegion()
        {
            _chunks = new Dictionary<ChunkCoords, Chunk>();
            _packedChunks = new Dictionary<ChunkCoords, PackedChunk>();
        }
        
        public bool TryGetChunk(ChunkCoords coords, out Chunk outChunk)
        {
            return _chunks.TryGetValue(coords, out outChunk);
        }
        
        public bool TryGetPacketChunk(ChunkCoords coords, out PackedChunk outPackedChunk)
        {
            return _packedChunks.TryGetValue(coords, out outPackedChunk);
        }
        
        public void SetChunk(ChunkCoords coords, Chunk chunk)
        {
            _chunks[coords] = chunk;
            _packedChunks.Remove(coords);
        }
        
        public void SetPacketChunk(ChunkCoords coords, PackedChunk packedChunk)
        {
            _packedChunks[coords] = packedChunk;
            _chunks.Remove(coords);
        }
    }
}