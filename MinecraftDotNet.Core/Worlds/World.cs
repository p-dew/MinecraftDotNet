using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;

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
        
        public event BlockPlacedHandler BlockPlaced;
        public event PlayerJoinedHandler PlayerJoined;
        
        public IBlockRepository BlockRepository { get; }
        
        public IChunkRepository ChunkRepository { get; }
    }
}