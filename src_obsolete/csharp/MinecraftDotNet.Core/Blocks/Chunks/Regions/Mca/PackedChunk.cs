using System.Collections.Generic;

namespace MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca
{
    public class PackedChunk
    {
        public PackedChunk(IReadOnlyList<byte> packedData, CompressionType compressionType)
        {
            PackedData = packedData;
            CompressionType = compressionType;
        }

        public IReadOnlyList<byte> PackedData { get; }
        
        public CompressionType CompressionType { get; }
    }

    public enum CompressionType : byte
    {
        GZip = 1,
        Zlib = 2
    }
}