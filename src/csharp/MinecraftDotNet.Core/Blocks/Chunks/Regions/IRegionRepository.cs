namespace MinecraftDotNet.Core.Blocks.Chunks.Regions
{
    public interface IRegionRepository
    {
        IRegion GetRegion(RegionCoords coords);
        void SaveRegion(RegionCoords coords, DictRegion dictRegion);
    }
}