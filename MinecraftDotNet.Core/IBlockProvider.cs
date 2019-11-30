using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public interface IBlockProvider
    {
        BlockInfo GetBlock(Coordinates3 pos);
        Meta GetBlockMeta(Coordinates3 pos);
    }
}