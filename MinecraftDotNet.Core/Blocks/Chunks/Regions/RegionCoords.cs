
namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
{
    /// <summary>
    /// Координаты региона. Верно только для данного RegionProvider
    /// </summary>
    public class RegionCoords
    {
        public RegionCoords(ChunkCoords chCoords) 
            : this(chCoords.X / DictRegion.Width,chCoords.Z / DictRegion.Depth)
        { }

        public RegionCoords(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }
        
        public int Z { get; }
    }
}