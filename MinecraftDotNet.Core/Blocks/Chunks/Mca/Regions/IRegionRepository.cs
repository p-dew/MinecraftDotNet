namespace MinecraftDotNet.Core.Blocks.Chunks.Mca.Regions
{
    public interface IRegionRepository
    {
        Region GetRegion(RegionCoords coords);
        void SaveRegion(RegionCoords coords, Region region);
    }
}