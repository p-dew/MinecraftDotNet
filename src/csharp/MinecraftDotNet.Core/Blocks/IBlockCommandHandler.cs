using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Blocks
{
    public interface IBlockCommandHandler
    {
        void SetBlock(BlockInfo blockInfo, Coords3 coords);
        void SetBlockMeta(Meta meta, Coords3 coords);
    }
}