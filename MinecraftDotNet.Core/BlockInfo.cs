namespace MinecraftDotNet.Core
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