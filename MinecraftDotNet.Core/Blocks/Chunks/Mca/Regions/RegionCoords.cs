
namespace MinecraftDotNet.Core.Blocks.Chunks.Mca.Regions
{
    /// <summary>
    /// Координаты региона. Верно только для данного RegionProvider
    /// </summary>
    public class RegionCoords
    {
        public RegionCoords(ChunkCoords chCoords) 
            : this(chCoords.X / Region.Width,chCoords.Z / Region.Depth)
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