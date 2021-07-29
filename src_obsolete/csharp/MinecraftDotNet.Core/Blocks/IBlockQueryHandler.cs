using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Blocks
{
    public interface IBlockQueryHandler
    {
        BlockInfo GetBlock(Coords3 coords);
        Meta GetBlockMeta(Coords3 coords);
    }
}