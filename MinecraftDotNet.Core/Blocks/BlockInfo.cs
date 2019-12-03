using MinecraftDotNet.Core.Items;

namespace MinecraftDotNet.Core.Blocks
{
    public class BlockInfo
    {
        public BlockInfo(ItemInfo itemInfo)
        {
            ItemInfo = itemInfo;
        }
        
        public ItemInfo ItemInfo { get; }
    }
}