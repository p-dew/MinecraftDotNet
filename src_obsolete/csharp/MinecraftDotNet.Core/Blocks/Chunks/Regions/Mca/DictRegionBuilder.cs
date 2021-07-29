namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public class DictRegionBuilder : IRegionBuilder
    {
        private readonly DictRegion _region;
        
        public DictRegionBuilder()
        {
            _region = new DictRegion();
        }
        
        public void SetChunk(ChunkCoords coords, Chunk chunk)
        {
            _region.SetChunk(coords, chunk);
        }

        public void SetPacketChunk(ChunkCoords coords, PackedChunk packedChunk)
        {
            _region.SetPacketChunk(coords, packedChunk);
        }

        public IRegion Build()
        {
            return _region;
        }
    }
}