using System;

namespace MinecraftDotNet.Core.Worlds
{
    public class WorldBuilder
    {
        private BlockRepositoryProvider? _blockRepositoryProvider;
        private ChunkRepositoryProvider? _chunkRepositoryProvider;
        
        public WorldBuilder()
        {
        }

        public WorldBuilder UseChunkRepository(ChunkRepositoryProvider chunkRepositoryProvider)
        {
            _chunkRepositoryProvider = chunkRepositoryProvider;
            return this;
        }

        public WorldBuilder UseBlockRepository(BlockRepositoryProvider blockRepositoryProvider)
        {
            _blockRepositoryProvider = blockRepositoryProvider;
            return this;
        }

        public World Build()
        {
            if (_blockRepositoryProvider == null)
                throw new InvalidOperationException("Cannot build the world without setting BlockRepository");
            if (_chunkRepositoryProvider == null)
                throw new InvalidOperationException("Cannot build the world without setting ChunkRepository");
            
            return new World(_chunkRepositoryProvider, _blockRepositoryProvider);
        }
    }
}