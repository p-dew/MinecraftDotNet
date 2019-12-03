using System.Collections.Generic;
using System.IO;

namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public class McaRegionRepository : IRegionRepository
    {
        private readonly string _regionsPath;
        private readonly IDictionary<RegionCoords, Region> _regions;

        public McaRegionRepository(string regionsPath)
        {
            _regionsPath = regionsPath;
            _regions = new Dictionary<RegionCoords, Region>();
        }

        public Region GetRegion(RegionCoords coords)
        {
            // Возвращаем регион, если уже загружен
            if (_regions.ContainsKey(coords))
            {
                return _regions[coords];
            }
            
            // Загружаем регион, сохраняем, возвращаем
            var region = LoadRegion(coords);
            _regions[coords] = region;
            return region;
        }

        public void SaveRegion(RegionCoords coords, Region region)
        {
            throw new System.NotImplementedException();
        }

        private Region LoadRegion(RegionCoords coords)
        {
            // Создаём новый регион, который вернём
            var region = new Region();

            // Global coords (0, 0) chunk inside world
            var regionX = coords.X * Region.Width;
            var regionZ = coords.Z * Region.Depth;
            
            var fName = $"r.{coords.X}.{coords.Z}.mca";
            using (var fStream = File.OpenRead(_regionsPath + fName))
            {
                var locations = new byte[4096];
                fStream.Read(locations, 0, 4096);
                var timeStamps = new byte[4096];
                fStream.Read(timeStamps, 0, 4096);

                for (var i = 0; i < 1024; i++)
                {
                    var chunkX = regionX + i % Region.Width; // TODO: Может быть вычислять все значения из RegionSize
                    var chunkZ = regionZ + i / Region.Depth; // Или же, полностью следовать стандарту файла без возможномти настройки.
                    //long offset = ((BitConverter.ToUInt32(locations, i * 4) & 0xFFFFFF00) >> 8) * 4096;
                    var offset = 
                        ((long) locations[i*4 + 0] << 16 |
                         (long) locations[i*4 + 1] << 8 |
                         (long) locations[i*4 + 2] << 0 )
                        * 4096;
                    if (offset == 0)
                        continue;
                    
                    fStream.Seek(offset, SeekOrigin.Begin);
                    
                    var dataSizeBuf = new byte[4];
                    fStream.Read(dataSizeBuf, 0, 4);
                    var dataSize = 
                        (long) dataSizeBuf[0] << 24 |
                        (long) dataSizeBuf[1] << 16 |
                        (long) dataSizeBuf[2] << 8  |
                        (long) dataSizeBuf[3] << 0  ;
                    
                    //var compressionType = CompressionType.GZip;
                    var compressionType = fStream.ReadByte() switch 
                    {
                        1 => CompressionType.GZip,
                        2 => CompressionType.Zlib,
                        _ => throw new InvalidDataException("Invalid compression type.")
                    };
                    
                    var compressedData = new byte[dataSize];
                    fStream.Read(compressedData, 0, (int) dataSize);
                    
                    // Создаём новый упакованный чанк
                    var packetChunk = new PackedChunk(compressedData, compressionType);
                    
                    // Добавляем его в регион
                    region.PackedChunks[new ChunkCoords(chunkX, chunkZ)] = packetChunk;
                }
            }
            
            return region;
        }
    }
}