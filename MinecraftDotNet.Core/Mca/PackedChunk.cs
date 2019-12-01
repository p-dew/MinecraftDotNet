using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using MinecraftDotNet.Core.Math;
using NbtLib;


namespace MinecraftDotNet.Core.Mca
{
    internal class PackedChunk
    {
        private ChunkCompression _comp { get; }


        private byte[] _data;

        internal PackedChunk(ChunkCompression comp, byte[] data)
        {
            _comp = comp;
            _data = data;
        }

        public Chunk Unpack()
        {

            using (var data = new MemoryStream()) 
            {
                using var packedData = new MemoryStream(_data);
                
                switch (_comp)
                {
                    case ChunkCompression.Zlib:
                        using (var decompressionStream = new DeflateStream(packedData, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(data);
                        }
                        break;
                    case ChunkCompression.GZip:
                        using (var decompressionStream = new GZipStream(packedData, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(data);
                        }
                        break;
                }
                
                data.Seek(0L, SeekOrigin.Begin);
                Console.WriteLine(data.Length);
                
                var tags = NbtConvert.ParseNbtStream(data);
                
                Console.WriteLine(tags.ToJsonString());
            }
            return new Chunk(new BlockInfo[16, 256, 16], new Dictionary<Coordinates3, Meta>());
        }


    }

    internal enum ChunkCompression: byte
    {
        GZip,
        Zlib
    }
}