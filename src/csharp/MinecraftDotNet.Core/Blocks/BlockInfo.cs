using MinecraftDotNet.Core.Items;

namespace MinecraftDotNet.Core.Blocks
{
    public class BlockInfo
    {
        public BlockInfo(ItemInfo itemInfo, BlockSides sides)
        {
            ItemInfo = itemInfo;
            Sides = sides;
        }
        
        public ItemInfo ItemInfo { get; }
        
        public BlockSides Sides { get; }
    }
}