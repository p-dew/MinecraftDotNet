namespace MinecraftDotNet.Core
{
    public class ChunkRegion
    {
        public ChunkCoords Size { get; } // TODO: replace to RegionSize or not...
        private ChunkCoords _pos { get; }

        public ChunkRegion(ChunkCoords pos, int xSize, int zSize)
        {
            _pos = pos;
            Size = new ChunkCoords(xSize, zSize);
        }

        public ChunkRegion(ChunkCoords pos0, ChunkCoords pos1)
        {
            _pos = new ChunkCoords(System.Math.Min(pos0.X, pos1.X), System.Math.Min(pos0.Z, pos1.Z));
            Size = new ChunkCoords(System.Math.Abs(pos0.X - pos1.X), System.Math.Abs(pos0.Z - pos1.Z));
        }

        /// <summary>
        /// проверяет принадлежат ли координаты чанка этому региону
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public bool IsContains(ChunkCoords coords)
        {
            int localX = coords.X - _pos.X;
            int localZ = coords.Z - _pos.Z;
            return (localX >= 0 && localX < Size.X) 
                   && (localZ >= 0 && localZ < Size.Z);
        }
    }
}