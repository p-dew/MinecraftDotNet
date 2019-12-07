using System;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;

namespace MinecraftDotNet.Core.Worlds
{
    public delegate IChunkRepository ChunkRepositoryProvider();
    public delegate IBlockRepository BlockRepositoryProvider();
    
    public class World : IWorld
    {
        public World(ChunkRepositoryProvider chunkRepositoryProvider, BlockRepositoryProvider blockRepositoryProvider)
        {
            BlockRepository = blockRepositoryProvider();
            ChunkRepository = chunkRepositoryProvider();
        }

        public event BlockPlacedHandler BlockPlaced = (info, coords, player) => { };
        public event PlayerJoinedHandler PlayerJoined = player => { };
        
        public IBlockRepository BlockRepository { get; }
        
        public IChunkRepository ChunkRepository { get; }
    }
}