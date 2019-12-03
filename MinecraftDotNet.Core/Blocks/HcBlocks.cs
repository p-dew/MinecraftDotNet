using MinecraftDotNet.Core.Items;

namespace MinecraftDotNet.Core.Blocks
{
    public static class HcBlocks
    {
        public static BlockInfo Air { get; } = new BlockInfo(new ItemInfo("air", 0));
        
        public static BlockInfo Dirt { get; } = new BlockInfo(new ItemInfo("dirt", 64));
    }
}