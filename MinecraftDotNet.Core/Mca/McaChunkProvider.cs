using System;
using System.Collections.Generic;
using System.IO;

namespace MinecraftDotNet.Core.Mca
{
    public class McaChunkProvider: IChunkProvider
    {
        public const string DefaultPath = "region/";

        public string Path { get; }
        private Dictionary<RegionCoords, Region> _regions;
        
        public McaChunkProvider() : this(DefaultPath)
        {
        }

        public McaChunkProvider(string url)
        {
            _regions = new Dictionary<RegionCoords, Region>();
            Path = url;
        }
        
        
        public Chunk GetChunk(ChunkCoords pos)
        {
            var region = GetRegion(new RegionCoords(pos));
            return region.GetChunk(pos);
        }


        private Region GetRegion(RegionCoords coords)
        {
            if (_regions.ContainsKey(coords))
            {
                return _regions[coords];
            }
            
            Region region = LoadRegion(coords);
            _regions[coords] = region;
            return region;
        }


        private Region LoadRegion(RegionCoords coords)
        {
            var region = new Region(coords);

            // Global coords (0, 0) chunk inside world
            var regionX = coords.X * Region.RegionSize;
            var regionZ = coords.Z * Region.RegionSize;
            
            var fName = $"r.{coords.X}.{coords.Z}.mca";
            using (var fStream = File.OpenRead(Path + fName))
            {
                var locations = new byte[4096];
                fStream.Read(locations, 0, 4096);
                var timeStamps = new byte[4096];
                fStream.Read(timeStamps, 0, 4096);

                for (var i = 0; i < 1024; i++)
                {
                    int chunkX = regionX + i % Region.RegionSize; // TODO: Может быть вычислять все значения из RegionSize
                    int chunkZ = regionZ + i / Region.RegionSize; // Или же, полностью следовать стандарту файла без возможномти настройки.
                    //long offset = ((BitConverter.ToUInt32(locations, i * 4) & 0xFFFFFF00) >> 8) * 4096;
                    long offset = ((long) locations[i*4 + 0] << 16 |
                                  (long) locations[i*4 + 1] << 8 |
                                  (long) locations[i*4 + 2] << 0 )
                                  * 4096;
                    if (offset == 0)
                    {
                        continue;
                    }
                    
                    fStream.Seek(offset, SeekOrigin.Begin);
                
                    var dataSizeBuf = new byte[4];
                    fStream.Read(dataSizeBuf);
                    var dataSize = (long) dataSizeBuf[0] << 24 |
                                   (long) dataSizeBuf[1] << 16 |
                                   (long) dataSizeBuf[2] << 8  |
                                   (long) dataSizeBuf[3] << 0  ;
                    
                    //var compressionType = ChunkCompression.GZip;
                    var compressionType = fStream.ReadByte() switch 
                    {
                        1 => ChunkCompression.GZip,
                        2 => ChunkCompression.Zlib,
                        _ => throw new InvalidDataException("Invalid compression type.")
                    };

                    var data = new byte[dataSize];
                    fStream.Read(data, 0, (int)dataSize);
                
                    var chunk = new PackedChunk(compressionType, data);
                    region.AddChunk(new ChunkCoords(chunkX, chunkZ), chunk);
                }
            }

            return region;
        }



    }
    
}