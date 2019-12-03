using System.Collections.Generic;

namespace MinecraftDotNet.Core.Blocks.Chunks.Mca.Regions
{
    public class Region
    {
        public static int Width { get; } = 32;
        
        public static int Depth { get; } = 32;

        public Region()
        {
            Chunks = new Dictionary<ChunkCoords, Chunk>();
            PackedChunks = new Dictionary<ChunkCoords, PackedChunk>();
        }
        
        public IDictionary<ChunkCoords, Chunk> Chunks { get; }
        
        public IDictionary<ChunkCoords, PackedChunk> PackedChunks { get; }
    }
}