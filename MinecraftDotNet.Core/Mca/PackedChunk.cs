using System;

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
            throw new NotImplementedException();
        }
    }

    public enum ChunkCompression: byte
    {
        GZip,
        Zlib
    }
}