
namespace MinecraftDotNet.Core.Mca
{
    /// <summary>
    /// Координаты региона. Верно только для данного RegionProvider
    /// </summary>
    internal class RegionCoords
    {
        public int X { get; }
        public int Z { get; }

        public RegionCoords(int x, int z)
        {
            X = x;
            Z = z;
        }

        public RegionCoords(ChunkCoords chCoords) : this(chCoords.X / Region.RegionSize,chCoords.Z / Region.RegionSize)
        {

        }
    }
}