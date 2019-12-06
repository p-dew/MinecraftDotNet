namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public class RegionBounds
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     ^           
        ///   . | #   #   # 
        ///     |           
        ///   . | #   #   # 
        ///   xSize         
        ///   . | O   #   # 
        ///     ---zSize----->
        ///   .   .   .   . 
        ///                 
        /// </remarks>
        public RegionBounds(ChunkCoords position, int xSize, int zSize)
        {
            Position = position;
            Size = new ChunkCoords(xSize, zSize);
        }

        public RegionBounds(ChunkCoords pos0, ChunkCoords pos1)
        {
            Position = new ChunkCoords(System.Math.Min(pos0.X, pos1.X), System.Math.Min(pos0.Z, pos1.Z));
            Size = new ChunkCoords(System.Math.Abs(pos0.X - pos1.X), System.Math.Abs(pos0.Z - pos1.Z));
        }

        public ChunkCoords Size { get; } // TODO: replace to RegionSize or not...
        
        public ChunkCoords Position { get; }

        /// <summary>
        /// Проверяет принадлежат ли координаты чанка этому региону
        /// </summary>
        public bool Contains(ChunkCoords coords)
        {
            int localX = coords.X - Position.X;
            int localZ = coords.Z - Position.Z;
            return (localX >= 0 && localX < Size.X) 
                   && (localZ >= 0 && localZ < Size.Z);
        }
    }
}