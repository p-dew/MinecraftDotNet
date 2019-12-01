using System;
using System.Collections.Generic;

namespace MinecraftDotNet.Core.Mca
{
    
    /// <summary>
    /// Промежуточный класс, хранящий сжатые данные чанков
    /// </summary>
    internal class Region
    {

        public const int RegionSize = 32;
        
        public ChunkRegion Bounds { get; }
        public RegionCoords Coords { get; }
        
        private Dictionary<ChunkCoords, Chunk> _chunks;
        private Dictionary<ChunkCoords, PackedChunk> _packed;

        internal Region(RegionCoords coords)
        {
            Coords = coords;
            Bounds = new ChunkRegion(new ChunkCoords(coords.X*RegionSize, coords.Z*RegionSize), RegionSize, RegionSize);
            _chunks = new Dictionary<ChunkCoords, Chunk>();
            _packed = new Dictionary<ChunkCoords, PackedChunk>();
        }

        internal Chunk GetChunk(ChunkCoords coord)
        {

            if (!Bounds.IsContains(coord))
            {
                throw new ArgumentException("Unable to get a chunk outside region.");
            }

            if (_chunks.ContainsKey(coord))
            {
                return _chunks[coord];
            }

            if (_packed.ContainsKey(coord))
            {
                var unpacked = _packed[coord].Unpack();
                _chunks[coord] = unpacked;
                _packed.Remove(coord);
                return unpacked;
            }
            
            throw new NotImplementedException("Generation new chunk is not implemented.");
        }

        internal void AddChunk(ChunkCoords coord, Chunk ch)
        {
            if (!Bounds.IsContains(coord))
            {
                throw new ArgumentException("Unable to add a chunk outside region.");
            }

            _chunks[coord] = ch;
        }
        
        internal void AddChunk(ChunkCoords coord, PackedChunk ch)
        {
            if (!Bounds.IsContains(coord))
            {
                throw new ArgumentException("Unable to add a chunk outside region.");
            }

            _packed[coord] = ch;
        }


    }
    
    
}