using System.Collections.Generic;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Blocks.Chunks
{
    /// <summary>
    /// Chunk is atomic RAM-stored unit.
    /// </summary>
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
        
        public Chunk()
            : this(new Dictionary<Coords3, Meta>())
        { }
        
        /// <summary>
        /// Creates chunk data-class which has constant Width*Height*Depth size filled with nulls.
        /// </summary>
        public Chunk(IDictionary<Coords3, Meta> blockMetas)
        {
            Blocks = new BlockInfo[Width, Height, Depth];
            BlockMetas = blockMetas;
        }
        
        public BlockInfo[,,] Blocks { get; }
        
        public IDictionary<Coords3, Meta> BlockMetas { get; }
    }
}