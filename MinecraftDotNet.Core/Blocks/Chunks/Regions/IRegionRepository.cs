namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
{
    public interface IRegionRepository
    {
        Region GetRegion(RegionCoords coords);
        void SaveRegion(RegionCoords coords, Region region);
    }
}