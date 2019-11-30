using System.Collections.Generic;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public class Chunk
    {
        public static int Width { get; } = 16;
        public static int Height { get; } = 256;
        public static int Depth { get; } = 16;
        
        public Chunk(BlockInfo[,,] blocks, Dictionary<Coordinates3, Meta> blockMetas)
        {
            Blocks = blocks;
            BlockMetas = blockMetas;
        }
        
        public BlockInfo[,,] Blocks { get; }
        public IReadOnlyDictionary<Coordinates3, Meta> BlockMetas { get; }
    }
}