using MinecraftDotNet.Core.Blocks.Chunks.Regions;
using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;

namespace MinecraftDotNet.Core.Blocks.Chunks
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
            if (region.TryGetChunk(coords, out var chunk))
            {
                return chunk;
            }

            // Если в регионе чанк запакован -> Распаковывам -> Добавляем в распакованные -> Возвращаем
            if (region.TryGetPacketChunk(coords, out var packedChunk))
            {
                var unpackedChunk = _chunkPacker.Unpack(packedChunk);

                // Add chunk to loaded chunks in region
                region.SetChunk(coords, unpackedChunk);
                
                return unpackedChunk;
            }

            // Иначе -> Генерируем -> Возвращаем
            
            var generatedChunk = _chunkGenerator.Generate(coords);
            
            region.SetChunk(coords, generatedChunk);
            
            return generatedChunk;
        }

        public void UnloadChunk(ChunkCoords coords)
        {
            throw new System.NotImplementedException();
        }
    }
}