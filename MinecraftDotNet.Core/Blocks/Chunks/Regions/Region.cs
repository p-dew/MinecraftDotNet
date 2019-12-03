using System.Collections.Generic;
using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;

namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
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