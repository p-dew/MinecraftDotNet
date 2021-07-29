using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Entities;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Worlds
{
    public delegate void BlockPlacedHandler(BlockInfo blockInfo, Coords3 coords, Player player);
    public delegate void PlayerJoinedHandler(Player player);
    
    public interface IWorld
    {
        event BlockPlacedHandler BlockPlaced;
        event PlayerJoinedHandler PlayerJoined;
        
        IBlockRepository BlockRepository { get; }
        IChunkRepository ChunkRepository { get; }
    }
}