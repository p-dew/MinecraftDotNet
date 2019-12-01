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
            Path = url;
        }
        
        
        public Chunk GetChunk(ChunkCoords pos)
        {
            var region = GetRegion(new RegionCoords(pos));
            region.GetChunk(pos);
            throw new NotImplementedException();
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
            var region = new Region();
            
            var fName = $"r.${coords.X}.${coords.Z}.mca";
            using (var stream = new FileStream(Path + fName, FileMode.Open))
            {
                byte[] locations = new byte[4096];
                stream.Read(locations);
                byte[] timeStamps = new byte[4096];
                stream.Read(timeStamps);
                for (int i = 0; i < 1024; i++)
                {
                    int chunkX = i % Region.RegionSize; // TODO: Может быть вычислять все значения из RegionSize
                    int chunkZ = i / Region.RegionSize; // Или же, полностью следовать стандарту файла без возможномти настройки.
                    long offset = (BitConverter.ToUInt32(locations, i * 4) & 0xFFFFFF00 >> 8) * 4096;
                    stream.Seek(offset, SeekOrigin.Begin);
                
                    byte[] dataSizeBuf = new byte[4];
                    stream.Read(dataSizeBuf);
                    var dataSize = BitConverter.ToUInt32(dataSizeBuf);
                    var compressionType = stream.ReadByte() != 2 ? ChunkCompression.GZip : ChunkCompression.Zlib;

                    var data = new byte[dataSize];
                    stream.Read(data);
                
                    var chunk = new PackedChunk(compressionType, data);
                    region.AddChunk(new ChunkCoords(chunkX, chunkZ), chunk);
                }
            }

            return region;
        }



    }
    
}