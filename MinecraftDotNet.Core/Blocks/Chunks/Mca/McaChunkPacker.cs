using System;
using System.IO;
using System.IO.Compression;
using NbtLib;

namespace MinecraftDotNet.Core.Blocks.Chunks.Mca
{
    public class McaChunkPacker : IChunkPacker
    {
        public McaChunkPacker()
        {
            
        }

        public Chunk Unpack(PackedChunk packedChunk)
        {
            using var decompressedStream = new MemoryStream();
            using var compressedStream = new MemoryStream((byte[]) packedChunk.PackedData);
            
            switch (packedChunk.CompressionType)
            {
                case CompressionType.Zlib:
                    using (var decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                    }
                    break;
                case CompressionType.GZip:
                    using (var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                    }
                    break;
            }
                
            decompressedStream.Seek(0L, SeekOrigin.Begin);
            Console.WriteLine(decompressedStream.Length);
                
            var tags = NbtConvert.ParseNbtStream(decompressedStream);
                
            Console.WriteLine(tags.ToJsonString());

            var newChunk = new Chunk();

            throw new NotImplementedException("Need to implement chunk reading");

            return newChunk;
        }

        public PackedChunk Pack(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}