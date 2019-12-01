using System.Collections.Generic;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public class Chunk
    {
        /// <summary>
        /// Matches to X coordinate.
        /// </summary>
        public static int Width { get; } = 16;
        
        /// <summary>
        /// Matches to Y coordinate.
        /// </summary>
        public static int Height { get; } = 256;
        
        /// <summary>
        /// Matches to Z coordinate.
        /// </summary>
        public static int Depth { get; } = 16;
        
        public Chunk(BlockInfo[,,] blocks, IDictionary<Coordinates3, Meta> blockMetas)
        {
            Blocks = blocks;
            BlockMetas = blockMetas;
        }

        public BlockInfo[,,] Blocks { get; }
        
        public IDictionary<Coordinates3, Meta> BlockMetas { get; }
    }
}