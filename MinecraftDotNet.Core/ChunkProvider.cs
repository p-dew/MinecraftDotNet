using System.Collections.Generic;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public class ChunkProvider: IChunkProvider
    {

        private Dictionary<Coordinates2, Chunk> _loaded;
        private Dictionary<Coordinates2, PackedChunk> _cache;
        private Dictionary<Coordinates2, Region> _regions;
        
        public Chunk GetChunk(Coordinates2 pos)
        {
            throw new System.NotImplementedException();
        }
    }
    
}