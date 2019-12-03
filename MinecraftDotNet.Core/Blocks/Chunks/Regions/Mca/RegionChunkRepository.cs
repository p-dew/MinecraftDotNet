namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public class RegionChunkRepository: IChunkRepository
    {
        private readonly IChunkPacker _chunkPacker;
        private readonly IChunkGenerator _chunkGenerator;
        private readonly IRegionRepository _regionRepository;
        
        public RegionChunkRepository(IChunkPacker chunkPacker, IChunkGenerator chunkGenerator, IRegionRepository regionRepository)
        {
            _chunkPacker = chunkPacker;
            _chunkGenerator = chunkGenerator;
            _regionRepository = regionRepository;
        }
        
        public Chunk GetChunk(ChunkCoords coords)
        {
            var region = _regionRepository.GetRegion(new RegionCoords(coords));

            // Если в регионе чанк загружен -> возвращаем
            if (region.Chunks.ContainsKey(coords))
            {
                return region.Chunks[coords];
            }

            // Если в регионе чанк запакован -> Распаковывам -> Добавляем в распакованные -> Возвращаем
            if (region.PackedChunks.ContainsKey(coords))
            {
                var packetChunk = region.PackedChunks[coords];
                var unpacketChunk = _chunkPacker.Unpack(packetChunk);

                // Add chunk to loaded chunks in region
                region.PackedChunks.Remove(coords);
                region.Chunks.Add(coords, unpacketChunk);
                
                return unpacketChunk;
            }

            // Иначе -> Генерируем -> Возвращаем
            
            var generatedChunk = _chunkGenerator.Generate(coords);
            
            region.Chunks.Add(coords, generatedChunk);
            
            return generatedChunk;
        }

        public void UnloadChunk(ChunkCoords coords)
        {
            throw new System.NotImplementedException();
        }
    }
}